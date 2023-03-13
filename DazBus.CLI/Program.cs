using System.CommandLine;
using DazBus.CLI.Commands;

var rootCommand = new RootCommand("DazBus CLI. Manage Azure ServiceBus messages from the command line.");
var queueCommand = QueueCommand.CreateCommand();
var topicCommand = TopicCommand.CreateCommand();
rootCommand.AddCommand(queueCommand);
rootCommand.AddCommand(topicCommand);

return await rootCommand.InvokeAsync(args);

