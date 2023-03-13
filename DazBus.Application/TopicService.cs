using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace DazBus.Application;

public static class TopicService
{
    public static async Task<int> GetTopicSubscriptionMessageCountAsync(string nameSpace, string topicName, string subscriptionName)
    {
        var credential = new DefaultAzureCredential();
        var client = new ServiceBusAdministrationClient(nameSpace, credential);
        var subscriptionPropertiesTask = await client.GetSubscriptionRuntimePropertiesAsync(topicName, subscriptionName);
        return Convert.ToInt32(subscriptionPropertiesTask.Value.DeadLetterMessageCount);
    }

    private static async IAsyncEnumerable<Tuple<string, int>> GetTopicSubscriptionsMessageCount(string nameSpace, string topic)
    {
        var credential = new DefaultAzureCredential();
        var client = new ServiceBusAdministrationClient(nameSpace, credential);
        var subscriptions = client.GetSubscriptionsAsync(topic);
        var enumerator = subscriptions.GetAsyncEnumerator();
        while (await enumerator.MoveNextAsync())
        {
            var subscriptionProperties = await client.GetSubscriptionRuntimePropertiesAsync(topic, enumerator.Current.SubscriptionName);
            var count = Convert.ToInt32(subscriptionProperties.Value.DeadLetterMessageCount);
            yield return new Tuple<string, int>(enumerator.Current.SubscriptionName, count);
        }
    }

    public static async Task GetDeadLetterCountForAllTopicsAsync(string nameSpace)
    {
        var credential = new DefaultAzureCredential();
        var client = new ServiceBusAdministrationClient(nameSpace, credential);

        var topics = client.GetTopicsAsync();
        var topicEnumerator = topics.GetAsyncEnumerator();
        while (await topicEnumerator.MoveNextAsync())
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"Topic: {topicEnumerator.Current.Name}");
            Console.ForegroundColor = ConsoleColor.White;

            await foreach (var (subscriptionName, count) in GetTopicSubscriptionsMessageCount(nameSpace, topicEnumerator.Current.Name))
            {
                if (count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                }

                Console.WriteLine($"\tSubscription: {subscriptionName,-50} Dead letter count: {count}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }

    public static async Task ReceiveAllSubscriptionMessagesAsync(string nameSpace, string topicName, string subscriptionName)
    {
        var credential = new DefaultAzureCredential();
        await using var client = new ServiceBusClient(nameSpace, credential);


        var receiver = client.CreateReceiver(topicName, subscriptionName, new ServiceBusReceiverOptions
        {
            SubQueue = SubQueue.DeadLetter,
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var messageCount = await GetTopicSubscriptionMessageCountAsync(nameSpace, topicName, subscriptionName);
        Console.Write($"Clearing {topicName} : {subscriptionName}, message count: {messageCount} / ");

        try
        {
            var counter = 0;
            var pos = Console.GetCursorPosition();
            var receivedMessages = receiver.ReceiveMessagesAsync().GetAsyncEnumerator();
            while (counter < messageCount && await receivedMessages.MoveNextAsync())
            {
                counter++;

                Console.SetCursorPosition(pos.Left, pos.Top);
                Console.Write(counter);
            }

            Console.WriteLine();
        }
        finally
        {
            await receiver.DisposeAsync();
            await client.DisposeAsync();
        }
    }
}
