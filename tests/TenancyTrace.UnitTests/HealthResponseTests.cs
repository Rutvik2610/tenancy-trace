using System.Text.Json;
using TenancyTrace.Contracts;
using Xunit;

namespace TenancyTrace.UnitTests;

public class HealthResponseTests
{
    [Fact]
    public void ContractPreservesServiceStatusAndTimestampThroughJson()
    {
        var expected = new HealthResponse("Workflow", "healthy", DateTimeOffset.Parse("2026-01-01T00:00:00Z"));
        var json = JsonSerializer.Serialize(expected);
        Assert.Equal(expected, JsonSerializer.Deserialize<HealthResponse>(json));
    }
}
