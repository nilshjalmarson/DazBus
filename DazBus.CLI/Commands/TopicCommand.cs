using System.CommandLine;
using DazBus.Application;

namespace DazBus.CLI.Commands;

public static class TopicCommand
{
    public static Command CreateCommand()
    {
        var command = new Command("topic", "Manage topics");

        var sbNameSpaceOption = new Option<string>(
            name: "--namespace",
            description: "ServiceBus namespace");

        var sbTopicNameOption = new Option<string>(
            name: "--topic",
            description: "Topic name");
        
        var sbSubscriptionNameOption = new Option<string>(
            name: "--subscription",
            description: "Subscription name");

        var listTopicsCommand = new Command("list", "List all topics and get their message count.")
        {
            sbNameSpaceOption
        };

        listTopicsCommand.SetHandler(async (sbNameSpace) => { await TopicService.GetDeadLetterCountForAllTopicsAsync(sbNameSpace); },
            sbNameSpaceOption);

        var clearSubscriptionCommand = new Command("clear", "Silently receive and delete all messages in the subscription")
        {
            sbNameSpaceOption,
            sbTopicNameOption,
            sbSubscriptionNameOption
        };

        clearSubscriptionCommand.SetHandler(async (sbNameSpace, topicName, subscriptionName) =>
            {
                await TopicService.ReceiveAllSubscriptionMessagesAsync(sbNameSpace,
                    topicName,
                    subscriptionName);
            },
            sbNameSpaceOption,
            sbTopicNameOption,
            sbSubscriptionNameOption);

        command.AddCommand(listTopicsCommand);
        command.AddCommand(clearSubscriptionCommand);
        return command;
    }
}