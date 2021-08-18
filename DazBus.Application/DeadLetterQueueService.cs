using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;

namespace DazBus.Application
{
    public static class DeadLetterQueueService
    {
        public static int GetMessageCount(string connectionString, string topic, string subscriptionName)
        { 
            var client = new ServiceBusAdministrationClient(connectionString);
            var subscriptionPropertiesTask = client.GetSubscriptionRuntimePropertiesAsync(topic, subscriptionName);
            Task.WaitAll(subscriptionPropertiesTask);
            var subscriptionProperties = subscriptionPropertiesTask.Result;
            return Convert.ToInt32(subscriptionProperties.Value.DeadLetterMessageCount);
        }
    }
}