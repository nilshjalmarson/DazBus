using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;

namespace AzDaBus.Application
{
    public class DeadLetterQueueService
    {
        public static int GetMessageCount(string connectionString, string topic, string subscriptionName)
        {
            var subPath = EntityNameHelper.FormatSubscriptionPath(topic, subscriptionName);
            var deadLetterPath = EntityNameHelper.FormatDeadLetterPath(subPath);

            var managementClient = new ManagementClient(connectionString);
            var queueInfoTask = managementClient.GetSubscriptionRuntimeInfoAsync(topic, subscriptionName);
            
            Task.WaitAll(queueInfoTask);
            var queue = queueInfoTask.Result;
            return Convert.ToInt32(queue.MessageCount);
        }
        
        public static Message PeekMessageAsync(string connectionString, string topic, string subscriptionName)
        {
            var subPath = EntityNameHelper.FormatSubscriptionPath(topic, subscriptionName);
            var deadLetterPath = EntityNameHelper.FormatDeadLetterPath(subPath);
            var receiver = new MessageReceiver(connectionString, deadLetterPath);

            var message = receiver.PeekAsync().Result;
            return message;
        }
    }
}