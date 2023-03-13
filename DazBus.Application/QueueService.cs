using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace DazBus.Application;

public static class QueueService
{
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

    public static async Task ReceiveAllQueueMessagesAsync(string nameSpace, string queueName)
    {
        var credential = new DefaultAzureCredential();
        await using var client = new ServiceBusClient(nameSpace, credential);


        var receiver = client.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            SubQueue = SubQueue.DeadLetter,
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var messageCount = await GetQueueMessageCount(nameSpace, queueName);
        Console.Write($"Clearing {queueName}, message count: {messageCount} / ");

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

    public static async Task ResendAllDeadLettersFromQueueAsync(string nameSpace, string queueName)
    {
        var credential = new DefaultAzureCredential();
        await using var client = new ServiceBusClient(nameSpace, credential);

        var receiver = client.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            SubQueue = SubQueue.DeadLetter
        });

        // receiver.AbandonMessageAsync()
        // var counter = 0;
        // var receivedMessages = receiver.ReceiveMessagesAsync().GetAsyncEnumerator();
        // while (await receivedMessages.MoveNextAsync())
        // {
        //     receivedMessages.Current.
        //     counter++;
        //     Console.Write(".");
        // }
        // Console.WriteLine($"Got {counter} for {queueName}");
    }

    public static async Task DeleteAllDeadLettersForAllQueuesAsync(string nameSpace)
    {
        var taskList = new List<Task>();
        var queues = GetQueuesMessageCount(nameSpace).GetAsyncEnumerator();
        while (await queues.MoveNextAsync())
        {
            if (queues.Current.Item2 > 0)
            {
                Console.WriteLine($"Queue: {queues.Current.Item1}, Count: {queues.Current.Item2}");
                if (queues.Current.Item2 == 0) continue;
                var task = ReceiveAllQueueMessagesAsync(nameSpace, queues.Current.Item1);
                taskList.Add(task);
            }
        }
        await Task.WhenAll(taskList);
    }

    public static async Task GetDeadLetterCountForAllQueuesAsync(string nameSpace)
    {
        var queues = GetQueuesMessageCount(nameSpace).GetAsyncEnumerator();
        while (await queues.MoveNextAsync())
        {
            if (queues.Current.Item2 > 0)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
            }

            Console.WriteLine($"Queue: {queues.Current.Item1,-50} Dead letter count: {queues.Current.Item2}");
            Console.ForegroundColor = ConsoleColor.White;
        }
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
}