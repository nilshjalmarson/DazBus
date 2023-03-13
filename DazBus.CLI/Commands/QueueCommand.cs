using System.CommandLine;
using DazBus.Application;

namespace DazBus.CLI.Commands;

public static class QueueCommand
{
    public static Command CreateCommand()
    {
        var command = new Command("queue", "Manage queues");

        var sbNameSpaceOption = new Option<string>(
            name: "--namespace",
            description: "ServiceBus namespace");

        var sbQueueNameOption = new Option<string>(
            name: "--name",
            description: "Queue name");


        var listQueuesCommand = new Command("list", "List all queues and get their message count.")
        {
            sbNameSpaceOption
        };

        listQueuesCommand.SetHandler(async (sbNameSpace) => { await QueueService.GetDeadLetterCountForAllQueuesAsync(sbNameSpace); }, sbNameSpaceOption);

        var clearQueueCommand = new Command("clear", "Silently receive and delete all messages in the queue")
        {
            sbNameSpaceOption,
            sbQueueNameOption
        };

        clearQueueCommand.SetHandler(async (sbNameSpace, queueName) => { await QueueService.ReceiveAllQueueMessagesAsync(sbNameSpace, queueName); }, sbNameSpaceOption,
            sbQueueNameOption);

        command.AddCommand(listQueuesCommand);
        command.AddCommand(clearQueueCommand);
        return command;
    }
}