# Phase 1 validation

Validated in the cloud development environment using .NET SDK 10.0.404 on Linux:

| Check | Result |
| --- | --- |
| `dotnet restore TenancyTrace.slnx` | All six projects restored |
| `dotnet build TenancyTrace.slnx --configuration Release --no-restore` | Passed, zero warnings/errors |
| `dotnet test TenancyTrace.slnx --configuration Release --no-build` | 4 passed, 0 failed, 0 skipped |
| Workflow `/api/health`, port 7071 | HTTP 200, service Workflow, status healthy, UTC timestamp |
| Evidence `/api/health`, port 7072 | HTTP 200, service Evidence, status healthy, UTC timestamp |
| Timeline `/api/health`, port 7073 | HTTP 200, service Timeline, status healthy, UTC timestamp |

HTTP checks used Azure Functions Core Tools 4.6.0 (Functions runtime
4.1045.200.25556), Azurite 3.35.0, and the local settings templates. All three
workers initialized and executed their health function successfully. Test processes
were stopped after validation. Generated output and local settings are ignored.

The SDK was installed outside the checkout. This cloud sandbox required a writable
`DOTNET_CLI_HOME` and `NUGET_PACKAGES` under `/workspace`, plus running .NET commands
outside the command sandbox to support named mutexes. These are execution-environment
requirements, not additional repository dependencies.

The npm Core Tools download route was denied by the network policy. The official
Core Tools GitHub release was used instead, with TLS verification retained.

The integration test project contains three in-process DI/HTTP-result composition
cases. Real host HTTP checks were separate manual smoke checks. CI runs the four
automated tests; it does not install or run Core Tools or Azurite.

No Azure SQL, Ledger transactions/digests, Entra authorization, Service Bus, Blob
Storage, tenancy behavior, or AI validation was tested. No paid resources were
provisioned. GitHub Actions execution and deployment are not established by local
build success.
