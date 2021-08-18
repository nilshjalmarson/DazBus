using System.Linq;
using Xunit;

namespace DazBus.Powershell.Tests
{
    public class GetDazBusDlqCountTests
    {
        [Fact]
        public void TestAgainstDemoServiceBus()
        {
            // Arrange.
            var cmdlet = new GetDazBusDlqCount
            {
                ConnectionString = "Endpoint=sb://nh-sb-sample.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=iYSnMspVj5oQiTxSxFMWO7kvNTRwZL7VDNljM/NDVdI=",
                TopicName = "basictopic",
                SubscriptionName = "Subscription1"
            };

            // Act.
            var results = cmdlet.Invoke().OfType<int>().ToList().First();

            // Assert.
            Assert.True(results == 1);
        }
    }
}