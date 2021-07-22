using System.Linq;
using Nh.AzServiceBus;
using Xunit;

namespace Nh.AzureServiceBus.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            // Arrange.
            var cmdlet = new GetNhDlqCount
            {
                ConnectionString = "Endpoint=sb://nh-sb-sample.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=iYSnMspVj5oQiTxSxFMWO7kvNTRwZL7VDNljM/NDVdI=",
                TopicName = "basictopic",
                SubscriptionName = "Subscription1"
            };

            // Act.
            var results = cmdlet.Invoke().OfType<DlqInfo>().ToList();

            // Assert.
            Assert.True(results.Count == 1);
        }
    }
}