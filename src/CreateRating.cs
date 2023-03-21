using System;
using System.IO;
using System.Net.Http;
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
        static readonly HttpClient client = new HttpClient();

        [FunctionName("CreateRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [CosmosDB("bfyoc", "ratings", ConnectionStringSetting = "CosmosDbConnectionString")] IAsyncCollector<dynamic> documentsOut,
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

            using HttpResponseMessage productValidationResponse = await client.GetAsync($"https://serverlessohapi.azurewebsites.net/api/GetProduct?productId={productId}");
            var productValidationMessage = await productValidationResponse.Content.ReadAsStringAsync();
            if (productValidationMessage == "Please pass a valid productId on the query string")
            {
                return new BadRequestResult();
            }

            using HttpResponseMessage userValidationResponse = await client.GetAsync($"https://serverlessohapi.azurewebsites.net/api/GetUser?userId={userId}");
            var userValidationMessage = await userValidationResponse.Content.ReadAsStringAsync();
            if (userValidationMessage == "Please pass a valid userId on the query string")
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
            await documentsOut.AddAsync(r);
            
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
