// See https://aka.ms/new-console-template for more information

using DazBus.Application;

Console.WriteLine("Welcome to DazBus. Select option.");
Console.WriteLine("1. Get Count");
var selectedOption = Console.ReadKey();
Console.WriteLine();
string output = "";

var dlqservice = new DeadLetterQueueService(args[0]);

switch (selectedOption.KeyChar)
{
    case '1':
        output = (await DeadLetterQueueService.GetQueueMessageCount(args[0],args[1])).ToString();
        break;
    case '2':
        output = await DeadLetterQueueService.PeekQueueMessageAsync(args[0],args[1]);
        break;
    case '3':
        output = await DeadLetterQueueService.ReceiveQueueMessageAsync(args[0],args[1]);
        break;
    case '4':
        await DeadLetterQueueService.ReceiveAllQueueForAllQueuesMessagesAsync(args[0]);
        break;
    case '5' :
        await dlqservice.ReceiveAllMessagesForAllTopicSubscriptionsAsync(args[0]);
        break;
}

Console.WriteLine(output);
await dlqservice.DisposeAsync();