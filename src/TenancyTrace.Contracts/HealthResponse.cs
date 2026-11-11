namespace TenancyTrace.Contracts;

/// <summary>Process liveness only; does not assert dependency readiness.</summary>
public sealed record HealthResponse(string Service, string Status, DateTimeOffset Timestamp);
