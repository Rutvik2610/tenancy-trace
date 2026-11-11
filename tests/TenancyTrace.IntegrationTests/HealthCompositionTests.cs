using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TenancyTrace.Contracts;
using Xunit;
using WorkflowHealth = TenancyTrace.Workflow.Functions.HealthFunction;
using EvidenceHealth = TenancyTrace.Evidence.Functions.HealthFunction;
using TimelineHealth = TenancyTrace.Timeline.Functions.HealthFunction;

namespace TenancyTrace.IntegrationTests;

// In-process DI/HTTP-result composition checks, not Functions host or Azure integration.
public class HealthCompositionTests
{
    [Theory]
    [InlineData("Workflow")]
    [InlineData("Evidence")]
    [InlineData("Timeline")]
    public void HealthResolvesThroughDiAndReturnsExpectedContract(string service)
    {
        var clock = new FixedClock();
        using var provider = new ServiceCollection().AddSingleton<TimeProvider>(clock).BuildServiceProvider();
        var request = new DefaultHttpContext().Request;
        var result = service switch
        {
            "Workflow" => ActivatorUtilities.CreateInstance<WorkflowHealth>(provider).Run(request),
            "Evidence" => ActivatorUtilities.CreateInstance<EvidenceHealth>(provider).Run(request),
            _ => ActivatorUtilities.CreateInstance<TimelineHealth>(provider).Run(request)
        };
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        Assert.Equal(new HealthResponse(service, "healthy", clock.GetUtcNow()), Assert.IsType<HealthResponse>(ok.Value));
    }

    private sealed class FixedClock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    }
}
