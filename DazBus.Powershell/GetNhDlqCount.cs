using System.Management.Automation;
using DazBus.Application;

namespace DazBus.Powershell;

[Cmdlet(VerbsCommon.Get, "DazBusDlqCount")]
[OutputType(typeof(int))]
public class GetDazBusDlqCount : Cmdlet
{
    [Parameter(Mandatory = false)] 
    public string ConnectionString { get; set; }

    [Parameter(Mandatory = true)] 
    public string SubscriptionName { get; set; }

    [Parameter(Mandatory = true)] 
    public string TopicName { get; set; }
        
    [Parameter] 
    public string Namespace { get; set; }

    protected override void ProcessRecord()
    {
        base.ProcessRecord();

        //var credential = new AzurePowerShellCredential();

        var messageCount = DeadLetterQueueService.GetQueueMessageCount(ConnectionString, TopicName);
        //var messageCount = DeadLetterQueueService.GetMessageCount(credential, Namespace, TopicName, SubscriptionName);
        WriteObject(messageCount);
    }
}