using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using ShapeForwardAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ShapeForwardAPI.Controllers
{
    [RoutePrefix("api/shape-forward")]
    public class ShapeForwardController : ApiController
    {
        private readonly HttpClient _httpClient;

        private Queue<List<ShapeInput>> _batchQueue = new Queue<List<ShapeInput>>();
        private bool _api1Busy = false;
        private bool _api2Busy = false;

        private string _api1Url = "http://localhost:81/api/shape-processing/calculate-batch";
       private string _api2Url = "http://localhost:82/api/shape-processing/calculate-batch";
        
        public ShapeForwardController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:81/");
        }

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Post([FromBody] ShapeInput input)
        {
            var json = JsonConvert.SerializeObject(input);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/shape-processing", content);
            if (!response.IsSuccessStatusCode)
            {
                return Content(response.StatusCode, "Error processing shape");
            }

            var resultJson = await response.Content.ReadAsStringAsync();
            return Ok(JsonConvert.DeserializeObject<ShapeInput>(resultJson));
        }

        [HttpPut]
        [Route("{key:int}")]
        public async Task<IHttpActionResult> Put(int key, [FromBody] ShapeInput input, [FromUri] string userId)
        {
            var json = JsonConvert.SerializeObject(input);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/shape-processing/{key}?userId={userId}", content);
            if (!response.IsSuccessStatusCode)
            {
                return Content(response.StatusCode, "Error updating shape");
            }

            var resultJson = await response.Content.ReadAsStringAsync();
            return Ok(JsonConvert.DeserializeObject<ShapeInput>(resultJson));
        }

        [HttpDelete]
        [Route("{key:int}")]
        public async Task<IHttpActionResult> Delete(int key, [FromUri] string userId)
        {
            var response = await _httpClient.DeleteAsync($"api/shape-processing/{key}?userId={userId}");
            if (!response.IsSuccessStatusCode)
            {
                return Content(response.StatusCode, "Error deleting shape");
            }
            return Ok();
        }

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> Get([FromUri] string userId)
        {

           
            var response = await _httpClient.GetAsync($"api/shape-processing?userId={userId}");
            if (!response.IsSuccessStatusCode)
            {
                return Content(response.StatusCode, "Error retrieving shapes");
            }

            var resultJson = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<ShapeInput[]>(resultJson);
            return Ok(list);
        }

        [HttpPost]
        [Route("calculate")]
        public async Task<IHttpActionResult> CalculateUncalculatedBalanced([FromUri] string userId)
        {
            
            var getResponse = await _httpClient.GetAsync($"api/shape-processing?userId={userId}");
            if (!getResponse.IsSuccessStatusCode)
                return Content(getResponse.StatusCode, "Error retrieving shapes");

            var getJson = await getResponse.Content.ReadAsStringAsync();
            var allShapes = JsonConvert.DeserializeObject<List<ShapeInput>>(getJson);

            var uncalculated = allShapes
                .Where(s => !s.IsCalculated)
                .ToList();

            if (!uncalculated.Any())
                return Ok("No uncalculated shapes found.");

            
            var shapeBatches = uncalculated
                .Select((s, i) => new { s, Index = i })
                .GroupBy(x => x.Index / 2)
                .Select(g => g.Select(x => x.s).ToList())
                .ToList();

            var processingTasks = new List<Task>();

            foreach (var batch in shapeBatches)
            {

                _batchQueue.Enqueue(batch); 
             
            }


            TryAssignBatchToApi(1 , userId);
            TryAssignBatchToApi(2 , userId);

            return Ok("All batches sent for calculation.");
        }

        private async Task TryAssignBatchToApi(int apiNumber, string userId)
        {
            if (_batchQueue.Count == 0)
                return;


            var batch = _batchQueue.Dequeue();
            if (apiNumber == 1 && !_api1Busy )
            {
                _api1Busy = true;

                
               await SendBatchToApi(batch, _api1Url, 1 , userId);
            }
            else if (apiNumber == 2 && !_api2Busy)
            {
                _api2Busy = true;
                
                await SendBatchToApi(batch, _api2Url, 2 , userId);
            }
        }

        private async Task SendBatchToApi(List<ShapeInput> batch, string apiUrl , int apiNumber, string userId)
        { 
            try
            {

                
                var json = JsonConvert.SerializeObject(batch);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                content.Headers.Add("X-Api-Name", $"ShapeProcessing{apiNumber}");

                
                var response = await _httpClient.PostAsync($"{apiUrl}?userId={userId}", content);
                response.EnsureSuccessStatusCode();

            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if(apiNumber == 1)
                {
                    
                    _api1Busy = false;
                    await TryAssignBatchToApi(1, userId);
                }
                else
                {
                    _api2Busy = false;
                    await TryAssignBatchToApi(2, userId);
                }
            }
        }



    }
    }
