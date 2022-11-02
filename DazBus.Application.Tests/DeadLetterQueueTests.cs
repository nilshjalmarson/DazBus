using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace DazBus.Application.Tests;

[TestFixture]
public class DeadLetterQueueTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task PeekMessageTest()
    {
        const string servicebusnamespace = "riksbyggenGraphServiceBusDev.servicebus.windows.net";
        const string queueName = "graph-tokenservice-contact";
        var message = await DeadLetterQueueService.PeekQueueMessageAsync(servicebusnamespace, queueName);
        Assert.IsNotNull(message);
    }

    [Test]
    public async Task GetCountTest()
    {
        const string servicebusnamespace = "riksbyggenGraphServiceBusDev.servicebus.windows.net";
        const string queueName = "graph-tokenservice-contact";
        var messageCount = await DeadLetterQueueService.GetQueueMessageCount(servicebusnamespace, queueName);
        messageCount.Should().BeGreaterThan(1);
    }
}