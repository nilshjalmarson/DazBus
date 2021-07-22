using System.Management.Automation;
using DazBus.Application;

namespace Nh.AzServiceBus
{
    [Cmdlet(VerbsCommon.Get, "NhDlqCount")]
    [OutputType(typeof(DlqInfo))]
    public class GetNhDlqCount : Cmdlet
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
            WriteObject(new DlqInfo {Count = messageCount});
        }
    }
}