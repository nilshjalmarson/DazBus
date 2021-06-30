using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DLQSpike
{
    public class Program
    {
        public static void Main(string topic, string subscriptionName, DateTime messagesOlderThen)
        {
            var app = new Program();
            Console.WriteLine($"Topic: {topic}, Subscription name: {subscriptionName}");
            Console.WriteLine(messagesOlderThen);
            app.Run(topic, subscriptionName, messagesOlderThen).GetAwaiter().GetResult();
        }

        private async Task Run(string topic, string subscriptionName, DateTime messagesOlderThen)
        {
            // messagesOlderThen = DateTime.Parse("2020-09-01");
            // if (string.IsNullOrWhiteSpace(topic)) topic = "cv";
            //
            // if (string.IsNullOrWhiteSpace(subscriptionName)) subscriptionName = "cvservice-user-updated";

            Console.WriteLine("(D)ev (T)est or (P)rod?.");
            var env = Console.Read();

            var connectionString = string.Empty;
            switch (env)
            {
                case 'D':
                    connectionString =
                        "Endpoint=sb://devtrrservicebus.servicebus.windows.net/;SharedAccessKeyName=MessagingAccessKey;SharedAccessKey=IGJdJZoSGBt8qeHy1g2OqOTHaXdHyDSPTmXoqbu/n+M=";
                    break;
                case 'T':
                    connectionString =
                        "Endpoint=sb://testtrrservicebus.servicebus.windows.net/;SharedAccessKeyName=MessagingAccessKey;SharedAccessKey=1INIAXgbcCvyiBmIpasleUS9Ly+P8GKxj1JprCm7O3k=";
                    break;
                case 'P':
                    connectionString =
                        "Endpoint=sb://trrservicebus.servicebus.windows.net/;SharedAccessKeyName=MessagingAccessKey;SharedAccessKey=pH5EZO+6TlgS+pDpLuHsVjMolniQzOXmJAR3+J7sByA=";
                    break;
            }


            Console.WriteLine("1. Peek one message.");
            Console.WriteLine("2. Receive one message.");
            Console.WriteLine("3. Receive all messages.");
            Console.WriteLine("4. Peek and post one message.");
            Console.WriteLine("5. Resend all dead letters.");
            Console.WriteLine("x. Exit");

            var key = new ConsoleKeyInfo();
            while (key.KeyChar != 'x')
            {
                Console.WriteLine("Select option:");
                key = Console.ReadKey();
                switch (key.KeyChar)
                {
                    case '1':
                        await PeekMessagesAsync(connectionString, topic, subscriptionName);
                        break;
                    case '2':
                        await ReceiveOneMessageAsync(connectionString, topic, subscriptionName);
                        break;
                    case '3':
                        await PickUpAllDeadletters(connectionString, topic, subscriptionName, messagesOlderThen);
                        break;
                    case '4':
                        await PeekAndPostDeadLetter(connectionString, topic, subscriptionName);
                        break;
                    case '5':
                        await ResendAllDeadletters(connectionString, topic, subscriptionName, messagesOlderThen);
                        break;
                    case 'x':
                        return;
                }
            }
        }

        private async Task ReceiveOneMessageAsync(string connectionString, string topic, string subscriptionName)
        {
            var subPath = EntityNameHelper.FormatSubscriptionPath(topic, subscriptionName);
            var receiver = new MessageReceiver(connectionString, EntityNameHelper.FormatDeadLetterPath(subPath),
                ReceiveMode.ReceiveAndDelete);
            // Browse messages from queue
            var message = await receiver.ReceiveAsync();
            // If the returned message value is null, we have reached the bottom of the log
            if (message != null) PrintMessage(message);
        }

        private async Task<Message> PeekMessagesAsync(string connectionString, string topic, string subscriptionName)
        {
            var subPath = EntityNameHelper.FormatSubscriptionPath(topic, subscriptionName);

            var deadLetterPath = EntityNameHelper.FormatDeadLetterPath(subPath);
            var receiver = new MessageReceiver(connectionString, deadLetterPath);

            // Browse messages from queue
            var message = await receiver.PeekAsync();

            // If the returned message value is null, we have reached the bottom of the log
            if (message != null) PrintMessage(message);

            return message;
        }

        private async Task<Message> GetMessagesAsync(string connectionString, string topic, string subscriptionName)
        {
            var subPath = EntityNameHelper.FormatSubscriptionPath(topic, subscriptionName);

            var deadLetterPath = EntityNameHelper.FormatDeadLetterPath(subPath);
            var receiver = new MessageReceiver(connectionString, deadLetterPath, ReceiveMode.ReceiveAndDelete);

            // Browse messages from queue
            var message = await receiver.ReceiveAsync();

            // If the returned message value is null, we have reached the bottom of the log
            if (message != null) PrintMessage(message);

            return message;
        }

        private Task PickUpAllDeadletters(string connectionString, string topic, string subscriptionName,
            DateTime messagesOlderThen)
        {
            var doneReceiving = new TaskCompletionSource<bool>();

            // here, we create a receiver on the Deadletter queue
            var subPath = EntityNameHelper.FormatSubscriptionPath(topic, subscriptionName);
            var dlqReceiver = new MessageReceiver(connectionString, EntityNameHelper.FormatDeadLetterPath(subPath));

            // register the RegisterMessageHandler callback
            dlqReceiver.RegisterMessageHandler(
                async (message, cancellationToken1) =>
                {
                    lock (Console.Out)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(
                            "MessageId = {0}, \t\tSequenceNumber = {1}, \t\tLabel = {2}, \t\tEnqueue time = {3}",
                            message.MessageId,
                            message.SystemProperties.SequenceNumber,
                            message.Label,
                            message.SystemProperties.EnqueuedTimeUtc);
                        Console.ResetColor();
                    }

                    if (message.SystemProperties.EnqueuedTimeUtc < messagesOlderThen)
                    {
                        Console.WriteLine("Completing message");
                        // finally complete the original message and remove it from the DLQ
                        await dlqReceiver.CompleteAsync(message.SystemProperties.LockToken);
                    }
                    else
                    {
                        Console.WriteLine("New message. Keep in DLQ");
                    }
                },
                new MessageHandlerOptions(e => LogMessageHandlerException(e))
                    {AutoComplete = false, MaxConcurrentCalls = 1});

            return doneReceiving.Task;
        }

        private Task ResendAllDeadletters(string connectionString, string topic, string subscriptionName,
            DateTime messagesOlderThen)
        {
            var doneReceiving = new TaskCompletionSource<bool>();

            // here, we create a receiver on the Deadletter queue
            var subPath = EntityNameHelper.FormatSubscriptionPath(topic, subscriptionName);
            var dlqReceiver = new MessageReceiver(connectionString, EntityNameHelper.FormatDeadLetterPath(subPath));
            var sender = new MessageSender(connectionString, topic);


            // register the RegisterMessageHandler callback
            dlqReceiver.RegisterMessageHandler(
                async (message, cancellationToken) =>
                {
                    lock (Console.Out)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(
                            "MessageId = {0}, \t\tSequenceNumber = {1}, \t\tLabel = {2}",
                            message.MessageId,
                            message.SystemProperties.SequenceNumber,
                            message.Label);
                        Console.ResetColor();
                    }

                    if (message.SystemProperties.EnqueuedTimeUtc > messagesOlderThen)
                    {
                        Console.WriteLine("Resending message");
                        var resubmitMessage = message.Clone();
                        await sender.SendAsync(resubmitMessage);

                        // finally complete the original message and remove it from the DLQ
                        await dlqReceiver.CompleteAsync(message.SystemProperties.LockToken);
                    }
                    else
                    {
                        Console.WriteLine("New message. Keep in DLQ");
                    }
                },
                new MessageHandlerOptions(e => LogMessageHandlerException(e))
                    {AutoComplete = false, MaxConcurrentCalls = 1});

            return doneReceiving.Task;
        }

        private async Task PeekAndPostDeadLetter(string connectionString, string topic, string subscriptionName)
        {
            var message = await GetMessagesAsync(connectionString, topic, subscriptionName);
            Console.WriteLine("Press any key to continue.");
            Console.Read();
            var sender = new MessageSender(connectionString, topic);
            var resubmitMessage = message.Clone();
            await sender.SendAsync(resubmitMessage);
        }

        private Task LogMessageHandlerException(ExceptionReceivedEventArgs e)
        {
            Console.WriteLine("Exception: \"{0}\" {0}", e.Exception.Message, e.ExceptionReceivedContext.EntityPath);
            return Task.CompletedTask;
        }

        private static void PrintMessage(Message message)
        {
            // print the message
            var body = Encoding.UTF8.GetString(message.Body);
            lock (Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(
                    "Message peeked: \nMessageId = {0}, \nSequenceNumber = {1}, \nEnqueuedTimeUtc = {2}," +
                    "\nExpiresAtUtc = {5}, \nContentType = \"{3}\", \nSize = {4}, \nState = {6}, " +
                    "  \nContent: \n{7}",
                    message.MessageId,
                    message.SystemProperties.SequenceNumber,
                    message.SystemProperties.EnqueuedTimeUtc,
                    message.ContentType,
                    message.Size,
                    message.ExpiresAtUtc,
                    "", //message.SystemProperties.State,// TODO: Need to restore that property
                    JToken.Parse(body).ToString(Formatting.Indented));
                Console.ResetColor();
            }
        }
    }
}