using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using ShapeForwardAPI.Models;
using System.Collections.Generic;
using System.Linq;

namespace ShapeForwardAPI.Controllers
{
    [RoutePrefix("api/shape-forward")]
    public class ShapeForwardController : ApiController
    {
        private readonly HttpClient _httpClient;

        private static readonly string[] processingApiUrls = new[]
        {
            "http://localhost:5002/api/shape-processing/calculate-batch",
            "http://localhost:5003/api/shape-processing/calculate-batch"
        };

        private static int _lastUsedApiIndex = 0; 

        public ShapeForwardController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5002/");
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
                
                var apiUrl = processingApiUrls[_lastUsedApiIndex];
                _lastUsedApiIndex = (_lastUsedApiIndex + 1) % processingApiUrls.Length;

                var json = JsonConvert.SerializeObject(batch);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var task = Task.Run(async () =>
                {
                    var client = new HttpClient();
                    try
                    {
                        var response = await client.PostAsync($"{apiUrl}?userId={userId}", content);
                        if (!response.IsSuccessStatusCode)
                        {
                            
                            System.Diagnostics.Debug.WriteLine($"Batch gönderilemedi: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"API çağrısı hatası: {ex.Message}");
                    }
                });

                processingTasks.Add(task);
            }

            await Task.WhenAll(processingTasks);

            return Ok("All batches sent for calculation.");
        }



    }
    }
