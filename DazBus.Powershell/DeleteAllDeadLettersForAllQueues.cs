using System.Management.Automation;
using DazBus.Application;

namespace DazBus.Powershell;

[Cmdlet(VerbsCommon.Remove, "DazBusAllDeadLettersForAllQueues")]
[OutputType(typeof(int))]
public class DeleteAllDeadLettersForAllQueues : Cmdlet
{
    [Parameter(Mandatory = true)]
    public string Namespace { get; set; }

    protected override void ProcessRecord()
    {
        base.ProcessRecord();
        (QueueService.DeleteAllDeadLettersForAllQueuesAsync(Namespace)).GetAwaiter().GetResult();
    }
}