using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FinalTestFunctions
{
    public static class OrderItemsReserver
    {
        [FunctionName("OrderItemsReserver")]
        public static async Task Run(
            [ServiceBusTrigger("orderitems", Connection = "QueueConnectionString")] string myQueueItem,
            MessageReceiver messageReceiver,
            string lockToken,
            ILogger log)
        {
            dynamic data = JsonConvert.DeserializeObject(myQueueItem);

            try
            {
                log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

                var storageClient = new BlobServiceClient(
                    "");
                var blobContainerClient = storageClient.GetBlobContainerClient("items");
                var blobClient = blobContainerClient.GetBlobClient(Guid.NewGuid().ToString());

                await blobClient.UploadAsync(new BinaryData(Encoding.UTF8.GetBytes(myQueueItem)));
            }
            catch (Exception e)
            {
                log.Log(LogLevel.Error, e, "An error occurred while uploading order items to the blob storage");

                await messageReceiver.DeadLetterAsync(lockToken);

                throw;
            }
        }
    }
}
