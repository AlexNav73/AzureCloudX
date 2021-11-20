using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FinalTestFunctions
{
    public static class DeliveryOrderProcessor
    {
        [FunctionName("DeliveryOrderProcessor")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                var client = new CosmosClient("");

                await client.CreateDatabaseIfNotExistsAsync("final-test");

                var database = client.GetDatabase("final-test");
                await database.CreateContainerIfNotExistsAsync(new ContainerProperties("orders", "/OrderId"));

                var container = database.GetContainer("orders");

                var body = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(body);

                data.OrderId = (string)data.Id;
                data.id = (string)data.Id;

                log.LogInformation($"order details is about to be uploaded: {data}");

                await container.CreateItemAsync(data, new PartitionKey((string)data.Id));
            }
            catch (Exception e)
            {
                log.Log(LogLevel.Error, e, "An error occurred while uploading order details");

                throw;
            }


            return new OkObjectResult("Order details uploaded to Cosmos DB!");
        }
    }
}
