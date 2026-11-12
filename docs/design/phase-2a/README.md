# Phase 2A — Domain model and critical consistency design

Status: **design proposal, not implemented or accepted**. Read with the original
[ADRs](../../README.md#architecture-decisions-and-original-screenshots),
[Flow 1](../../process-flows/01-event-with-evidence.md),
[Flow 5](../../process-flows/05-cross-tenancy-access.md),
[event status model](../../state-models/event.md), and
[import review](../../architecture/import-review.md).

## Review map

- [Domain entities and SQL relationship diagram](domain-model.md)
- [Authorization and cross-tenancy responses](authorization.md)
- [Acceptance protocol, ownership and sequence diagram](consistency-protocol.md)
- [API contract proposal](api-contract.md)
- [Failure scenarios and verification plan](failure-matrix.md)
- [Proposed ADR amendment P-006](proposed-adr-amendment.md)

## Recommendation and approval boundary

Use one connection and one Azure SQL transaction for a command's Workflow state,
Evidence-owned native append-only Ledger entry, Evidence outbox and durable command
receipt. Workflow invokes a narrowly privileged SQL command module; it does not
invoke Evidence HTTP for event acceptance. SQL owns the atomic boundary, not an HTTP
or distributed transaction. This requires approval of [P-006](proposed-adr-amendment.md)
before implementation. Existing ADRs and original flows remain unchanged.

“Accepted write” means the server durably committed a command; it does not mean the
counterparty acknowledged its assertions. `Acknowledged` is a separate event status.
A client receives success only after commit, but a lost response can hide a committed
write. An ambiguous 503 must never promise that nothing was committed.

## Source alignment

| Source | Preserved | Proposed clarification/change |
| --- | --- | --- |
| ADR-002 Accepted | Native Azure SQL append-only Ledger, content hashes, external digests and verification; mutable current state separate | Canonical event payload hashes supplement, not replace, attachment hashes; no application hash chain |
| ADR-003 Accepted | Three Function Apps, three queues, SQL outbox, at-least-once/idempotent processing | SQL command ownership and stable message IDs; no new broker or distributed transaction |
| ADR-005 Accepted | Fail closed when durable provenance cannot commit; same-key retries and operational reconciliation | P-006 explicitly replaces two commits over HTTP for this path, and distinguishes unknown commit from known rollback |
| ADR-004 Accepted | Inline validation only; no malware-scanning claim | Attachment lifecycle is separate from metadata event acceptance and remains follow-up design |
| ADR-001 Proposed | Optional reviewed AI remains Proposed | No AI scope implemented or approval inferred |
| Flows 1 and 5 | Tenancy authorization and non-disclosing access | Prefer 404 for missing/invisible resource; 403 only for visible resource with insufficient action permission |
| Event status model | Existing states and edges | Conservative consent/actor guards proposed; no new event status |

## Assumptions and open decisions

All new entity fields, API routes, role guards, receipt retention and SQL permissions
below are proposals for review, not recovered historical decisions.

1. MVP has at most one active Tenant and one active Landlord per tenancy; co-tenants,
   agents, role succession and historical-party rights require an extension. A
   creator's role is self-declared for the synthetic MVP, not proof of legal tenure.
2. New tenancies are Draft until the opposite party accepts an invitation. No event
   writes while Draft. Dates/property labels are descriptive, not legal validation.
3. Either participant may invite the opposite party. Reviewer access requires both
   parties' approval of a finite, named-report grant; no unilateral reviewer grants.
4. Resolution requires both active parties' consent to identical resolution content,
   even on the unguarded Acknowledged-to-Resolved edge. Review this conservative rule.
5. Identity directory allowlists, reviewer eligibility authority, invitation delivery,
   membership replacement, closed-tenancy retention/access and data deletion obligations
   need product/security approval. No implementation should guess them.
6. Cross-schema command permissions couple schema evolution and database availability.
   A database split would require a new protocol/ADR; this is not independently
   deployable database isolation merely because schemas have owners.
7. Retain dedupe identity and receipts for tenancy lifetime initially. Approve retention,
   privacy, key size limits and archival before enabling deletion or reuse.
8. Independent digest protection/export SLA, freshness alarms, key/identity separation
   and Azure restore/failover behavior require an operational design and real tests.
9. Report completion audit, AI approval/retry ambiguity, file cleanup and rejection
   auditing in the import review remain open; this phase does not solve them by
   claiming the metadata transaction spans Blob or remote services.

## Validation and limits

This change is documentation only. The design guarantee is conditional on all writers
using the reviewed command module, verified privileges/constraints, durable SQL commit
and correct recovery code. It is not an empirical Ledger guarantee. No Azure resources,
SQL schema, business logic, Entra policies or business tests are created here.
See the [verification plan](failure-matrix.md#implementation-verification-plan) before
claiming any implementation satisfies these invariants.

### Checks performed for this proposal

- Both new Mermaid blocks (relationship and acceptance sequence) parsed and rendered
  to SVG/PNG with Mermaid 11.17.2 and Chromium; both rendered diagrams were visually
  inspected. This is local rendering, not verification of GitHub's renderer.
- Relative Markdown link targets and `git diff --check` passed.
- Diff scope was checked: only new Phase 2A design documents and an index link;
  existing ADRs, original diagrams/flows, source, tests, solution and CI are unchanged.
- Application build/tests were not rerun for this documentation-only change. No SQL,
  Azure Ledger, Entra or Service Bus tests were executed; all implementation checks
  in the failure matrix remain future work.
