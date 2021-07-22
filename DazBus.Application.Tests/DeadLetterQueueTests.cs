using DazBus.Application;
using NUnit.Framework;

namespace AzDaBus.Application.Tests
{
    [TestFixture]
    public class DeadLetterQueueTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void PeekMessageTest()
        {
            var connectionString = "Endpoint=sb://devtrrservicebus.servicebus.windows.net/;SharedAccessKeyName=CoreAccessKey;SharedAccessKey=tLdhrENVkPapXmFcUoK18aLQmIVcNdypNiESW/nSwxw=";
            var topicName = "user-profile";
            var subscriptionName = "outgoing_user-updated_client-match-domain";
            //var messageCount = DeadLetterQueueService.PeekMessageAsync(connectionString, topicName, subscriptionName);
        }

        [Test]
        public void GetCountTest()
        {
            var connectionString = "Endpoint=sb://nh-sb-sample.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=iYSnMspVj5oQiTxSxFMWO7kvNTRwZL7VDNljM/NDVdI=";
            var topicName = "basictopic";
            var subscriptionName = "Subscription1";
            var messageCount = DeadLetterQueueService.GetMessageCount(connectionString, topicName, subscriptionName);
            Assert.AreEqual(1,messageCount);
        }
    }
}