using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using TenancyTrace.Contracts;

namespace TenancyTrace.Workflow.Functions;

public sealed class HealthFunction(TimeProvider clock)
{
    // Anonymous by design: exposes no tenancy or dependency information.
    [Function("WorkflowHealth")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequest request)
        => new OkObjectResult(new HealthResponse("Workflow", "healthy", clock.GetUtcNow()));
}
