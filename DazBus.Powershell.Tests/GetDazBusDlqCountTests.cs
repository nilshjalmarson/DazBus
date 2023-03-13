using System.Linq;
using Xunit;

namespace DazBus.Powershell.Tests;

public class GetDazBusDlqCountTests
{
    [Fact]
    public void TestAgainstDemoServiceBus()
    {
        // Arrange.
        var cmdlet = new GetDazBusDlqCount
        {
            Namespace = "riksbyggengraphservicebusdev.servicebus.windows.net",
            QueueName = "graph-customer-contact"
        };

        // Act.
        var results = cmdlet.Invoke().OfType<int>().ToList().First();

        // Assert.
        Assert.True(results > 1);
    }
}