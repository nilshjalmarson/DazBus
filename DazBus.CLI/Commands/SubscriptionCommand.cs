using System.CommandLine;

namespace DazBus.CLI.Commands;

// Commands for managing a subscription
// Peak a message
// Re-send a dead letter message
public static class SubscriptionCommand
{
    public static Command CreateCommand()
    {
        var command = new Command("subscription", "Manage subscriptions");

        var nameOption = new Option<string>(
                name: "--name",
                description: "Subscription name")
        {
            IsRequired = true
        };
        nameOption.AddAlias("-n");

        return command;
    }
}