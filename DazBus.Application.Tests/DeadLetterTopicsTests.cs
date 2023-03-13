using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace DazBus.Application.Tests;

[TestFixture]
public class DeadLetterTopicsTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task GetCountTest()
    {
        const string servicebusnamespace = "riksbyggenGraphServiceBusDev.servicebus.windows.net";
        await TopicService.GetDeadLetterCountForAllTopicsAsync(servicebusnamespace);
    }
}