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
            ConnectionString = "Endpoint=sb://riksbyggengraphservicebusat.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=r9WdxThNTGWz3VzCSdA5OIGgOLb0kH3MbNrzc5X5hco=",
            TopicName = "boardeaser-callback-received",
            SubscriptionName = "callbacks",
            Namespace = "riksbyggengraphservicebusat.servicebus.windows.net"
        };

        // Act.
        var results = cmdlet.Invoke().OfType<int>().ToList().First();

        // Assert.
        Assert.True(results > 1);
    }
}