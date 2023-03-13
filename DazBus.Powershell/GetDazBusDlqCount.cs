using System.Management.Automation;
using DazBus.Application;

namespace DazBus.Powershell;

[Cmdlet(VerbsCommon.Get, "DazBusDlqCount")]
[OutputType(typeof(int))]
public class GetDazBusDlqCount : Cmdlet
{
    [Parameter(Mandatory = true)]
    public string Namespace { get; set; }

    [Parameter(Mandatory = true)]
    public string QueueName { get; set; }

    protected override void ProcessRecord()
    {
        base.ProcessRecord();
        var messageCount = (QueueService.GetQueueMessageCount(Namespace, QueueName)).GetAwaiter().GetResult();
        WriteObject(messageCount);
    }
}