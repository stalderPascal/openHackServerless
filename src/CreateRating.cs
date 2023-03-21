using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bfyoc.Functions
{
    public static class CreateRating
    {
        [FunctionName("CreateRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            
            var productId = data.productId;
            var userId = data.userId;
            var rating = (int)data.rating;
            
            // Validation
            if (rating < 0 || rating > 5)
            {
                return new BadRequestResult();
            }

            // Create Rating
            var r = new UserRating 
            {
                ProductId = productId,
                UserId = userId,
                LocationName = data.locationName,
                Rating = rating,
                UserNotes = data.userNotes
            };

            // Store Rating
            
            
            return new OkObjectResult(r);
        }
    }

    public class UserRating
    {
        public UserRating()
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
        }

        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public string LocationName { get; set; }
        public int Rating { get; set; }
        public string UserNotes { get; set; }
        public DateTime Timestamp { get; private set; }
        public Guid Id { get; private set; }
    }
}
