using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using ShapeProcessingAPI.Models;

namespace ShapeProcessingAPI.Controllers
{
    [RoutePrefix("api/shape-processing")]
    public class ShapeProcessingController : ApiController
    {
        private readonly ApplicationDbContext _context;

        public ShapeProcessingController()
        {
            _context = new ApplicationDbContext();
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Post([FromBody] ShapeInput input)
        {
            if (input == null)
                return BadRequest("Input cannot be null.");

            if (string.IsNullOrEmpty(input.UserId))
                return BadRequest("UserId is required.");

            if (input.Shape == "Square")
            {
                input.Height = input.Width;
            }

            _context.ShapeInputs.Add(input);
            _context.SaveChanges();

            return Ok(input);
        }

        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Put(int id, [FromBody] ShapeInput input, [FromUri] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("UserId is required.");

            var userShape = _context.ShapeInputs.FirstOrDefault(s => s.Id == id && s.UserId == userId);
            if (userShape == null)
                return NotFound();

            if (!string.IsNullOrEmpty(input.Shape))
                userShape.Shape = input.Shape;

            if (input.Width != 0)
                userShape.Width = input.Width;

            if (input.Height != null)
                userShape.Height = input.Shape == "Square" ? input.Width : input.Height;

            
            userShape.CalculationResult = input.CalculationResult;
            userShape.IsCalculated = input.IsCalculated;

            _context.SaveChanges();

            return Ok(userShape);
        }


        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id, [FromUri] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("UserId is required.");

            var userShape = _context.ShapeInputs.FirstOrDefault(s => s.Id == id && s.UserId == userId);

            if (userShape == null)
                return NotFound();

            _context.ShapeInputs.Remove(userShape);
            _context.SaveChanges();

            return Ok();
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult Get([FromUri] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                System.Diagnostics.Debug.WriteLine("UserId null geldi");
                return BadRequest("UserId cannot be null.");
            }

            var list = _context.ShapeInputs
                               .Where(s => s.UserId == userId)
                               .ToList();

            return Ok(list);
        }


        [HttpPost]
        [Route("calculate-batch")]
        public IHttpActionResult CalculateBatch([FromBody] List<ShapeInput> shapes, [FromUri] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("UserId is required.");

            foreach (var input in shapes)
            {
                var shape = _context.ShapeInputs.FirstOrDefault(s => s.Id == input.Id && s.UserId == userId);
                if (shape == null)
                    continue;

               

                switch (shape.Shape)
                {
                    case "Square":
                        shape.Height = shape.Width;
                        shape.CalculationResult = (int)(shape.Width * shape.Width);
                        break;
                    case "Rectangle":
                        shape.CalculationResult = (int)(shape.Width * shape.Height);
                        break;
                    case "Triangle":
                        shape.CalculationResult = (int)(shape.Width * shape.Height) / 2;
                        break;
                    default:
                        shape.CalculationResult = 0;
                        break;
                }

                var apiName = Request.Headers.Contains("X-Api-Name")
                  ? Request.Headers.GetValues("X-Api-Name").FirstOrDefault()
                  : "UnknownApi";

                DbLogger.Log(apiName, userId, $"İşleniyor -> ID: {shape.Id}, Shape: {shape.Shape} ");


                shape.IsCalculated = true;
            }

            _context.SaveChanges();

           


            return Ok("Batch calculation completed.");
        }





        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
