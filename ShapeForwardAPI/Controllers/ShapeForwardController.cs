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
        public async Task<IHttpActionResult> CalculateUncalculated([FromUri] string userId)
        {
            var getResponse = await _httpClient.GetAsync($"api/shape-processing?userId={userId}");
            if (!getResponse.IsSuccessStatusCode)
                return Content(getResponse.StatusCode, "Error retrieving shapes");

            var getJson = await getResponse.Content.ReadAsStringAsync();
            var allShapes = JsonConvert.DeserializeObject<List<ShapeInput>>(getJson);

            var uncalculatedShapes = allShapes
                .Where(s => s.IsCalculated == false)
                .ToList();

            if (!uncalculatedShapes.Any())
                return Ok("No uncalculated shapes.");

            foreach (var shape in uncalculatedShapes)
            {
                
                if (shape.Shape == "Square" || shape.Shape == "Rectangle")
                    shape.CalculationResult = (int)(shape.Width * shape.Height);
                else if (shape.Shape == "Triangle")
                    shape.CalculationResult = (int)((shape.Width * shape.Height) / 2);
                else
                    shape.CalculationResult = 0;

                shape.IsCalculated = true;

                var json = JsonConvert.SerializeObject(shape);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var putResponse = await _httpClient.PutAsync($"api/shape-processing/{shape.Id}?userId={userId}", content);
                if (!putResponse.IsSuccessStatusCode)
                    continue; 
            }

            return Ok("Calculation completed.");
        }
      }
    }
