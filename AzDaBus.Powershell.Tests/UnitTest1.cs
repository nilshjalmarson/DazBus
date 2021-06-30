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
                ConnectionString =
                    "Endpoint=sb://devtrrservicebus.servicebus.windows.net/;SharedAccessKeyName=CoreAccessKey;SharedAccessKey=tLdhrENVkPapXmFcUoK18aLQmIVcNdypNiESW/nSwxw=",
                TopicName = "user-profile",
                SubscriptionName = "outgoing_user-updated_client-match-domain"
            };

            // Act.
            var results = cmdlet.Invoke().OfType<DlqInfo>().ToList();

            // Assert.
            Assert.True(results.Count == 1);
        }
    }
}