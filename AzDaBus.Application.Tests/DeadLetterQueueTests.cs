using NUnit.Framework;

namespace AzDaBus.Application.Tests
{
    public class DeadLetterQueueTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void PeekMessageTest()
        {
            string connectionString = "Endpoint=sb://devtrrservicebus.servicebus.windows.net/;SharedAccessKeyName=CoreAccessKey;SharedAccessKey=tLdhrENVkPapXmFcUoK18aLQmIVcNdypNiESW/nSwxw=";
            string topicName = "user-profile";
            string subscriptionName = "outgoing_user-updated_client-match-domain";
            var messageCount = DeadLetterQueueService.PeekMessageAsync(connectionString, topicName, subscriptionName);
        }
    }
}