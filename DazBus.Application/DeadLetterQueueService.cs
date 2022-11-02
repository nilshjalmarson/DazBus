using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace DazBus.Application;

public class DeadLetterQueueService : IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusAdministrationClient _adminClient;

    public DeadLetterQueueService(string nameSpace)
    {
        var credential = new DefaultAzureCredential();
        _client = new ServiceBusClient(nameSpace, credential);
        _adminClient = new ServiceBusAdministrationClient(nameSpace, credential);
    }

    public static async Task<int> GetQueueMessageCount(string nameSpace, string queueName)
    {
        var credential = new DefaultAzureCredential();
        var client = new ServiceBusAdministrationClient(nameSpace, credential);
        var subscriptionPropertiesTask = await client.GetQueueRuntimePropertiesAsync(queueName);
        return Convert.ToInt32(subscriptionPropertiesTask.Value.DeadLetterMessageCount);
    }

    public static async Task<string> PeekQueueMessageAsync(string nameSpace, string queueName)
    {
        var credential = new DefaultAzureCredential();
        await using var client = new ServiceBusClient(nameSpace, credential);

        var receiver = client.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            SubQueue = SubQueue.DeadLetter
        });

        var receivedMessage = await receiver.PeekMessageAsync();

        var body = receivedMessage.Body.ToString();
        return body;
    }

    public static async Task<string> ReceiveQueueMessageAsync(string nameSpace, string queueName)
    {
        var credential = new DefaultAzureCredential();
        await using var client = new ServiceBusClient(nameSpace, credential);

        var receiver = client.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            SubQueue = SubQueue.DeadLetter,
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var receivedMessage = await receiver.ReceiveMessageAsync();

        var body = receivedMessage.Body.ToString();
        System.Text.Json.JsonSerializer.Deserialize<dynamic>(body);

        return body;
    }

    public static async Task ReceiveAllQueueMessagesAsync(string nameSpace, string queueName, int messageCount)
    {
        var credential = new DefaultAzureCredential();
        await using var client = new ServiceBusClient(nameSpace, credential);

        var receiver = client.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            SubQueue = SubQueue.DeadLetter,
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var receivedMessages = await receiver.ReceiveMessagesAsync(messageCount);
        Console.Write($"Got {receivedMessages.Count} for queue {queueName}");
    }

    public async Task ReceiveAllSubscriptionMessagesAsync(string nameSpace, string queueName, int messageCount)
    {
        var receiver = _client.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            SubQueue = SubQueue.DeadLetter,
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var receivedMessages = await receiver.ReceiveMessagesAsync(messageCount);
        Console.Write($"Got {receivedMessages.Count} for queue {queueName}");
    }

    public static async Task ReceiveAllQueueForAllQueuesMessagesAsync(string nameSpace)
    {
        var tasklist = new List<Task>();
        var queues = GetQueuesMessageCount(nameSpace).GetAsyncEnumerator();
        while (await queues.MoveNextAsync())
        {
            Console.WriteLine($"Queue: {queues.Current.Item1}, Count: {queues.Current.Item2}");
            if (queues.Current.Item2 <= 0) continue;
            var task = ReceiveAllQueueMessagesAsync(nameSpace, queues.Current.Item1, queues.Current.Item2);
            tasklist.Add(task);
        }

        await Task.WhenAll(tasklist);
    }

    public async Task ReceiveAllMessagesForAllTopicSubscriptionsAsync(string nameSpace)
    {
        var taskList = new List<Task>();
        var topicsEnumerator = GetAllTopicsAsync().GetAsyncEnumerator();
        while (await topicsEnumerator.MoveNextAsync())
        {
            Console.WriteLine($"Topic: {topicsEnumerator.Current.TopicName}, Subscription: {topicsEnumerator.Current.SubscriptionName}, Count: {topicsEnumerator.Current.Count}");
            if (topicsEnumerator.Current.Count <= 0) continue;
            Console.WriteLine($"Get all for {topicsEnumerator.Current.TopicName}/{topicsEnumerator.Current.SubscriptionName}");
        }

        await Task.WhenAll(taskList);
    }

    private static async IAsyncEnumerable<Tuple<string, int>> GetQueuesMessageCount(string nameSpace)
    {
        var credential = new DefaultAzureCredential();
        var client = new ServiceBusAdministrationClient(nameSpace, credential);
        var queues = client.GetQueuesAsync();
        var enumerator = queues.GetAsyncEnumerator();
        while (await enumerator.MoveNextAsync())
        {
            var subscriptionPropertiesTask = await client.GetQueueRuntimePropertiesAsync(enumerator.Current.Name);
            var count = Convert.ToInt32(subscriptionPropertiesTask.Value.DeadLetterMessageCount);
            yield return new Tuple<string, int>(enumerator.Current.Name, count);
        }
    }

    private async IAsyncEnumerable<SubscriptionCount> GetAllTopicsAsync()
    {
        var topics = _adminClient.GetTopicsAsync();
        var enumerator = topics.GetAsyncEnumerator();
        while (await enumerator.MoveNextAsync())
        {
            var allSubscriptionsAsync = GetAllSubscriptionsAsync(enumerator.Current.Name).GetAsyncEnumerator();
            while (await allSubscriptionsAsync.MoveNextAsync())
            {
                yield return allSubscriptionsAsync.Current;
            }
        }
    }

    private async IAsyncEnumerable<SubscriptionCount> GetAllSubscriptionsAsync(string topicName)
    {
        var subscriptions = _adminClient.GetSubscriptionsAsync(topicName);
        var enumerator = subscriptions.GetAsyncEnumerator();
        while (await enumerator.MoveNextAsync())
        {
            var subscriptionPropertiesTask = await _adminClient.GetSubscriptionRuntimePropertiesAsync(enumerator.Current.TopicName, enumerator.Current.SubscriptionName);
            var count = Convert.ToInt32(subscriptionPropertiesTask.Value.DeadLetterMessageCount);
            yield return new SubscriptionCount(enumerator.Current.TopicName, enumerator.Current.SubscriptionName, count);
        }
    }

    public record SubscriptionCount(string TopicName, string SubscriptionName, int Count);

    public ValueTask DisposeAsync()
    {
        return _client.DisposeAsync();
    }
}