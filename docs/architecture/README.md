# Architecture foundation

This document records the Phase 1 foundation. Original design material is now
available in the [documentation index](../README.md); see the separate
[import review](import-review.md) for status, source comparison, and open issues.
References below to missing ADRs describe the Phase 1 baseline, before that import.

## Target system

Three independently runnable .NET 10 Azure Functions isolated-worker services:

| Service | Intended responsibility | Future data schema |
| --- | --- | --- |
| Workflow | Tenancy events, disputes, acknowledgments, access checks | workflow |
| Evidence & Provenance | Evidence validation, provenance, durable acceptance | evidence |
| Timeline & Reports | Authorized timelines, asynchronous reports, reviewed summaries | timeline |

Target dependencies are Azure SQL Database (including append-only Ledger provenance),
Blob Storage for evidence/reports, Service Bus for events/jobs, and Microsoft Entra ID.
A React/TypeScript client and Bicep infrastructure are planned. None are implemented
or provisioned in this foundation. Functions hosts reference shared contracts only;
they do not reference each other. The integration test project references all hosts
solely to exercise their composition.

## Required invariants

These requirements come from the project brief. They are not a reconstruction of
the missing ADRs, and the foundation does not yet enforce or validate them:

1. Never acknowledge a tenancy event as accepted without durable provenance.
2. Commit provenance and related outbox entries atomically in a single database
   transaction. Resolve service/transaction ownership before implementing acceptance.
3. Internal APIs must authenticate and authorize the calling service. A Functions
   key or anonymous route is not an implementation of Entra service authorization.
4. Every protected tenancy operation must check tenancy-specific access; reviewers
   receive access only to their authorized evidence reports.
5. Async consumers must handle redelivery idempotently, including durable deduplication
   and safe settlement/retry behavior.
6. AI summaries remain optional. Deterministic validation and human approval must
   precede any approved publication; AI failure must not block the core workflow.
7. MVP Scan Gate performs inline file validation only. Do not describe it as
   comprehensive malware scanning.
8. Preserve independently protected Ledger digests and define how to verify them.
   Ledger table creation alone does not establish tamper detection or validate guarantees.
9. Obtain and follow the five existing ADRs before implementing dependent behavior.

## Phase 1 assumptions and boundaries

- Services share one SQL Database in the target architecture, with distinct schemas;
  transaction boundaries and database permissions await approved design details.
- Shared contracts currently contain only a liveness response. No domain event or
  acceptance semantics are invented here.
- DI uses the Functions builder and injects `TimeProvider` into health functions.
- Only anonymous, non-sensitive liveness endpoints are exposed. There are no protected
  operations whose authorization could be bypassed in this skeleton.
- Local emulator configuration supports the Functions runtime; it does not emulate
  SQL Ledger or prove Azure behavior.
- No identity, credentials, infrastructure deployment, or paid resources are created.

## Follow-up work

1. Supply authoritative ADR-001 through ADR-005 content and resolve design gaps.
2. Specify durable acceptance protocol, provenance/outbox transaction ownership,
   failure recovery, idempotency keys, event contracts, and replay/version policies.
3. Define Entra audiences, scopes/app roles, service identities, tenancy membership,
   reviewer authorization, and negative access tests before adding protected routes.
4. Add SQL migrations and integration tests for atomic rollback, concurrency,
   append-only Ledger behavior, independent digest retention and verification.
5. Implement blob upload boundaries, inline size/type/content validation, immutable
   evidence references, and explicit Scan Gate limitations.
6. Add Service Bus publishing/consuming, outbox dispatch, retry/dead-letter behavior,
   idempotency tests, report generation, and authorized report retrieval.
7. Define optional AI validation, reviewer approval state machine and audit trail.
8. Add React/TypeScript frontend, Bicep modules and reviewed deployment CI/CD after
   resource, identity and cost requirements are approved.
9. Expand integration tests to real Functions HTTP hosts and appropriately isolated
   service dependencies. Use synthetic fixtures only.
