using System.Management.Automation;
using DazBus.Application;

namespace Nh.AzServiceBus
{
    [Cmdlet(VerbsCommon.Get, "NhTestValue")]
    [OutputType(typeof(string))]
    public class GetNhTestValue : Cmdlet
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
            WriteObject($"{ConnectionString}, {SubscriptionName}, {TopicName}");
        }
    }
}