# P-006 — Proposed amendment to critical event acceptance ownership

**Status: Proposed — NOT ACCEPTED.** Candidate ADR number 006 must be confirmed
against the project ADR register before adoption. This proposal does not edit or
supersede existing ADRs merely by being merged as design documentation.

## Context

[ADR-005](../../adr/ADR-005.md) and [Flow 1](../../process-flows/01-event-with-evidence.md)
put the ledger commit behind Evidence HTTP, then commit Workflow state. This protects
against provenance-free acknowledgment but leaves ledger-only orphans and ambiguous
outcomes across two commits. The [import review](../../architecture/import-review.md)
identifies that gap. One physical Azure SQL Database already contains workflow,
evidence and timeline schemas. The current task prefers a single SQL transaction
without distributed transactions over HTTP.

## Options

| Option | Benefit | Cost |
| --- | --- | --- |
| Keep original Evidence HTTP ledger-first protocol with reconciliation | Strong service-owned HTTP boundary; least historical change | Two commits, orphan recovery complexity; cannot atomically commit Workflow state and provenance |
| Evidence owns the whole acceptance HTTP command and writes Workflow schema | One SQL transaction remains possible | Moves tenancy command ownership/trust into Evidence; Evidence gains Workflow mutation responsibility |
| Workflow invokes reviewed cross-schema SQL command modules (recommended) | Atomic state/history/outbox/receipt; no network transaction; Workflow remains policy owner | Deliberate shared-database/module coupling, signing/permission and migration discipline |
| Distributed transaction or acknowledge then async-provenance | None needed for MVP | Distributed transaction explicitly excluded; asynchronous provenance violates fail-closed acknowledgment |

## Proposed decision

Approve the [single-transaction protocol](consistency-protocol.md) for tenancy/membership
and event commands. Workflow authenticates/authorizes the user and invokes a narrow
SQL command module. Evidence retains schema and native Ledger/outbox ownership;
reviewed append modules participate in that SAME transaction. No generic direct DML
or cross-service HTTP transaction. Evidence internal endpoints retain independent
service authorization; user tenancy authorization remains Workflow's responsibility.

Amend ADR-005's critical-path dependency and recovery language only if approved:

- Event acceptance depends on durable SQL Ledger commit, not Evidence HTTP runtime
  availability. SQL failure rolls back state, provenance, outbox and receipt together.
- Retries are same-key whole-command retries (proposed interpretation: initial
  attempt plus up to three retries, bounded). No blind resubmission with a new key.
- Proven rollback can report not committed. Unknown commit reports outcome_unknown
  and resolves against the durable receipt; a 503 alone is not proof of rollback.
- Original ledger-only orphan reconciliation remains relevant for legacy two-commit
  writes and separate attachment workflows, but is not the normal new event protocol.

ADR-002's native Ledger/digest decision and ADR-003's SQL outbox/idempotent consumer
rules are preserved. Do not replace native Ledger with a custom hash chain, promote
ADR-001 to Accepted, or describe inline validation as malware scanning. Existing
Flow 1 and L2/L3 ownership drawings remain historical source; create updated derived
views only after approval rather than editing the originals silently.

## Consequences

A successful durable commit enforces the four-record acceptance invariant under
reviewed writer permissions; an Evidence HTTP outage alone no longer blocks metadata
writes. Shared schema deployment and a single database become intentional coupling.
A compromised trusted Workflow principal remains powerful through allowed commands;
module input validation, audit, grant testing and denial of direct DML are mandatory.
Independent service/database scaling and future database separation become harder.
This proposal makes no guarantees about untested Ledger/restore behavior.

## Approval and implementation gates

Architecture owner must explicitly approve this boundary change and record the final
ADR number/status. Product/security review must approve membership/reviewer rules,
resolution guards, response disclosure and receipt retention. Then test native Ledger
DDL/index constraints, signed-module privileges, atomic rollback/fault injection,
concurrent revocation/idempotency, broker replay and independent digest verification
with synthetic data. A merged design PR is not evidence those gates passed.
