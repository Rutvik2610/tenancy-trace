using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using TenancyTrace.Contracts;

namespace TenancyTrace.Timeline.Functions;

public sealed class HealthFunction(TimeProvider clock)
{
    // Anonymous by design: exposes no tenancy or dependency information.
    [Function("TimelineHealth")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequest request)
        => new OkObjectResult(new HealthResponse("Timeline", "healthy", clock.GetUtcNow()));
}
