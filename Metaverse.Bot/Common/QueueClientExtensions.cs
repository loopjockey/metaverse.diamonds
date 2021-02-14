using Azure.Storage.Queues;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Metaverse.Bot.Common
{
    public static class QueueClientExtensions
    {
        public static async Task SendJsonMessageAsync<TType>(this QueueClient queueClient, TType message) 
        {
            var jsonText = JsonConvert.SerializeObject(message);
            var plainTextBytes = Encoding.UTF8.GetBytes(jsonText);
            var base64Data = Convert.ToBase64String(plainTextBytes);
            try
            {
                await queueClient.SendMessageAsync(base64Data);
            }
            catch (StorageException) 
            {
                await queueClient.CreateIfNotExistsAsync();
                await queueClient.SendMessageAsync(base64Data);
            }
        }
    }
}
