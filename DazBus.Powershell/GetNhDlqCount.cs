using System.Management.Automation;
using DazBus.Application;

namespace DazBus.Powershell
{
    [Cmdlet(VerbsCommon.Get, "DazBusDlqCount")]
    [OutputType(typeof(int))]
    public class GetDazBusDlqCount : Cmdlet
    {
        [Parameter(Mandatory = true)] 
        public string ConnectionString { get; set; }

        [Parameter(Mandatory = true)] 
        public string SubscriptionName { get; set; }

        [Parameter(Mandatory = true)] 
        public string TopicName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var messageCount = DeadLetterQueueService.GetMessageCount(ConnectionString, TopicName, SubscriptionName);
            WriteObject(messageCount);
        }
    }
}