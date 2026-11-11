# TenancyTrace

TenancyTrace is a tenancy evidence and workflow platform for tenants, landlords,
and approved reviewers. Development uses synthetic tenancy data only.

## Phase 1 scope

This repository provides three .NET 10 Azure Functions v4 isolated-worker hosts,
shared contracts, xUnit test projects, and CI. Only process liveness is implemented.
There are no tenancy APIs, persistence adapters, queues, authentication integration,
AI features, frontend application, deployment pipeline, or provisioned Azure resources.

## Repository structure

```text
TenancyTrace.slnx                  .NET solution
src/
  TenancyTrace.Workflow.Functions/ Workflow host (local port 7071)
  TenancyTrace.Evidence.Functions/ Evidence & Provenance host (7072)
  TenancyTrace.Timeline.Functions/ Timeline & Reports host (7073)
  TenancyTrace.Contracts/          Shared transport contracts
tests/
  TenancyTrace.UnitTests/          Contract tests
  TenancyTrace.IntegrationTests/   In-process DI/HTTP-result composition tests
docs/README.md                   Documentation index
docs/architecture/               Original diagrams, foundation and import review
docs/adr/                        Original ADR transcriptions
docs/process-flows/              Five process sequence diagrams
docs/state-models/               Event, report and AI summary state diagrams
docs/source-images/              Original L1 diagram and ADR photographs
.github/workflows/ci.yml           Restore, Release build, and tests
```

See the [documentation index](docs/README.md) for the original architecture diagrams,
process flows, state models, and ADR transcriptions with source screenshots.
ADR-001 remains **Proposed**; ADR-002 through ADR-005 are **Accepted**.
The [Phase 1 architecture and constraints](docs/architecture/README.md) describe the
implemented foundation. The [import review](docs/architecture/import-review.md)
records transcription differences, unresolved design tensions, and validation limits.

## Prerequisites

- .NET SDK 10.0.404 (pinned in `global.json`, latest patch roll-forward permitted).
- For running HTTP hosts: Azure Functions Core Tools v4 with .NET 10 isolated
  worker support, plus Node.js LTS and Azurite for local storage.
- No Azure subscription or cloud credentials are required for Phase 1.

If the SDK was installed outside PATH, add its directory to PATH in your shell.
Use the existing checkout in cloud tasks; do not create a worktree unless requested.

## Build and test

From the repository root:

```sh
dotnet restore TenancyTrace.slnx
dotnet build TenancyTrace.slnx --configuration Release --no-restore
dotnet test TenancyTrace.slnx --configuration Release --no-build
```

The integration project currently tests in-process composition only. It does not
start the Functions host or validate Azure services, Entra policies, Ledger,
outbox transactions, queue delivery, or storage behavior.

## Run locally

Copy the configuration template for each service, without overwriting existing settings:

```sh
for service in Workflow Evidence Timeline; do
  project="src/TenancyTrace.$service.Functions"
  test -f "$project/local.settings.json" || cp "$project/local.settings.example.json" "$project/local.settings.json"
done
```

The examples contain only emulator settings, not credentials. `local.settings.json`
is ignored and excluded from publishing. Never place secrets in the example files.

Start Azurite in a separate terminal (install its CLI with `npm install -g azurite`
if necessary):

```sh
azurite --location /tmp/tenancytrace-azurite
```

Then start each host in a separate terminal from its project directory:

```sh
cd src/TenancyTrace.Workflow.Functions
func start --port 7071
```

Use `src/TenancyTrace.Evidence.Functions` with port 7072 and
`src/TenancyTrace.Timeline.Functions` with port 7073 for the other hosts.
Check each with `curl --fail http://localhost:7071/api/health` (adjust the port).
Expect HTTP 200 with service, status `healthy`, and a UTC timestamp. Health endpoints
are anonymous and contain no tenancy data; they indicate liveness, not readiness
of Azure dependencies. No other endpoints are implemented.

## CI and next steps

GitHub Actions restores, builds, and runs both test projects on pull requests and
pushes to main, retaining TRX results. It neither deploys nor provisions resources.
See [follow-up work](docs/architecture/README.md#follow-up-work) and
[validation evidence](docs/architecture/validation.md) for current limitations.
