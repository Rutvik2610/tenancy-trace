# Failure scenarios and recovery proposal

`Success` below means a command commit, not an HTTP packet arriving at the client or
an event becoming Acknowledged. Every scenario assumes P-006 is approved and correctly
implemented. No row in this matrix is a completed integration test.

| Failure/scenario | Durable result / client response | Recovery and invariant |
| --- | --- | --- |
| Missing token | No mutation; 401 | No tenancy lookup/existence disclosure |
| Wrong tenancy/hidden event/revoked or expired membership | No mutation; 404 | Same generic body for missing/invisible; scoped joins and composite keys |
| Visible resource, forbidden action | No mutation; 403 | No reviewer writes or self-acknowledgment |
| Access store unavailable | No mutation; 503 | No stale cached permit |
| Validation fails/stale ETag | No mutation; 400/422 or 412 | No success receipt; fresh valid command may use a new key |
| Evidence HTTP down, SQL command module healthy | New command may succeed under P-006 | Explicit ADR-005 amendment, not an accidental fail-open implementation |
| SQL/Ledger unavailable or denied insert | All command writes rolled back; 503 if exhausted | No state-only success; retain client draft |
| Crash after event insert, before Ledger insert | Transaction rolls back on session termination/recovery | No observable committed event without provenance |
| Ledger succeeds but outbox/receipt/projection insert fails | Same SQL transaction rolls back all effects | No ledger-only orphan from new acceptance path |
| Process dies after statements but before commit | SQL aborts or caller sees uncertain outcome during recovery | Reconnect; resolve key; never infer from process memory |
| SQL commit returns timeout/disconnect | Could be committed; 503 outcome_unknown if receipt cannot be resolved | Same-key retry against primary; no claim that 503 means not accepted |
| Commit succeeded, response lost | All four effects remain | Fresh authorization + same fingerprint replays original result |
| Restart after commit | Receipt/outbox remain, no local cache needed | Replay response; reclaim expired relay lease |
| Duplicate same key/body in parallel | One commit; other waits then replays | Unique range/key protects absent record, not only process-local lock |
| Same key, different semantic body or If-Match | Existing result unchanged; 409 | Never overwrite receipt or create second event |
| Same key on different principal/resource scope | Independent namespaces, no receipt leak | Cannot dedupe unrelated identities; auth still constrains effects |
| Repeated equivalent intent with DIFFERENT keys | Distinct commands unless domain guard blocks | No exactly-once intent claim; client must retain original key |
| Retry old successful status command after later status change | Original receipt replay if still authorized | Check matching receipt before current ETag/edge guards; no new transition |
| Access revoked after original success, then retry | No replay data; 404 | Current visibility overrides historical receipt entitlement |
| Membership revocation races command | Tenancy lock serializes ordering | If revocation wins, deny; if write wins, it had access at transaction authorization point |
| Concurrent opposing transitions | One version update wins; other 412 | Lock + expected version; no lost updates |
| Second resolution consent duplicates/races | One consent per party/version/hash; one final transition | Serialize event; stale version/changed text cannot resolve old proposal |
| Deadlock/lock timeout | Rollback, bounded same-key retry; eventually 503 | Stable lock order; do not retry only Ledger insert |
| Relay crashes before send | Committed outbox Pending/InFlight | Reclaim expired lease and send same MessageId |
| Broker accepts, relay loses acknowledgment or dies before Sent | May deliver again | Resend same MessageId; durable consumer inbox dedupes |
| Broker outage / repeated relay errors | Accepted event remains; projection delayed | Persist retries, alarm, retain outbox, never silently discard |
| Consumer crashes before inbox/projection commit | Neither effect durable | Broker redelivery repeats transaction |
| Consumer commits but broker completion lost | Both effects durable | Duplicate receipt prevents second projection effect |
| Messages reordered / missing event version | Projection not falsely advanced past a gap | Persist/defer gap or rebuild from authoritative history; monitor |
| Poison message / delivery limit reached | DLQ, visible operational failure | Investigate then replay same MessageId; no duplicate or silent loss |
| Evidence blob stored but reference append fails | Outside metadata SQL protocol | Never expose as accepted evidence; separate staging/cleanup design required |
| Privileged ledger/blob tampering / digest stale | Atomic commit alone is insufficient | Native verification, independently protected digests, hash recomputation and alerts; do not claim validated detection |
| Restore/failover loses recent transactions | Depends on actual Azure recovery configuration | Validate RPO/consistency including dedupe and digests; no universal zero-data-loss promise |

## Implementation verification plan

Use synthetic tenancy A/B principals and a real supported Azure SQL Ledger test
environment only when separately authorized; local SQL/emulators do not establish
Azure guarantees. Required future tests:

1. Attempt direct DML with each service identity; only reviewed command/append/relay
   module permissions must work. Verify no Ledger update/delete permission and native
   append-only enforcement. Check actual index/FK support and deployment signatures.
2. Inject exceptions after each write before commit; assert aggregate, transition,
   Ledger, outbox and receipt all roll back. Verify successful versions have exactly
   one matching canonical provenance and initial message; reject tampered links.
3. Kill process/connection before and around commit, lose HTTP responses, restart;
   same-key replay yields exactly the committed command effects, or no effects before
   a new attempt. Verify 503 unknown handling rather than asserting rollback.
4. Race same/different payload keys, ETags, status actions, consents, revocation and
   expiry. Prove one commit per key and ordered authorization boundaries.
5. Test every role against every route with missing/wrong-tenant nested IDs, expired
   grants, wrong invitation identity, old receipts, reviewer report subsets and lists.
   Assert generic 404 vs visible 403 policy and absence of direct blob bypass.
6. Crash relay and consumer at send/commit/ack boundaries; test redelivery, event
   version gaps, poison messages, lease fencing and replay with retained MessageId.
7. Export digests to independently protected storage, then use supported native
   verification and synthetic corruption tests. Test stale checkpoints, digest access
   denial, evidence byte mismatch and restore scenarios separately from append tests.
8. Publish canonical serialization fixtures and test hashes across runtimes. Never
   use real tenancy data, credentials or externally obtained evidence in fixtures.

## Design-document validation

The diagrams and relative links should be checked before review. Source, tests,
solution, CI and existing ADRs must remain unchanged. A documentation check or passing
Mermaid render is not a SQL consistency, security or Ledger verification result.
