# Proposed API contract v1

Design only; no routes are implemented by this phase. Paths below are public
Workflow routes prefixed `/api/v1`. JSON uses camelCase, UUIDs as strings and UTC
ISO-8601 instants. Unknown fields, duplicate JSON keys and client-authored actor,
status, provenance, hash or server timestamp fields are rejected. Synthetic data only.

## Common contract

- Bearer user token with appropriate delegated scope; apply
  [authorization](authorization.md) to every route and each nested resource.
- All POST/DELETE mutations require `Idempotency-Key`. Role, identity and tenant are
  server-resolved, never trusted from a body. Missing/invalid key: 400.
- Existing-resource mutations require `If-Match` with the strong opaque ETag of
  the resource identified below; missing 428, stale 412. A matching successful
  replay returns its original response before current-state precondition checks,
  but AFTER fresh visibility and operation-level authorization. Same key with a
  changed body/ETag is 409 `idempotency_key_reused`.
- Mutation success is synchronous 201/200 only after atomic SQL commit, never 202
  for acceptance deferred to a queue. First resolution consent is a committed
  consent, not a promise that event status changed. Response ETag comes from the
  event for event/consent actions, membership for membership actions, and tenancy
  for tenancy creation. Response representation is a commit snapshot, not current
  read state; replay includes `Idempotency-Replayed: true`.
- GETs return ETag; no pagination cursor can omit tenancy/visibility constraints.
  Errors are `application/problem+json` with stable `type`, `title`, `status`,
  `code`, `traceId`; no hidden-resource identifiers/details. Error statuses follow
  the authorization document, plus 400 validation, 422 invalid semantic input,
  409 domain conflict, 429 throttling, 503 unavailability. Retry-After on transient
  503/429; same-key retry for writes. No stack traces, tokens or SQL text.

## Routes and request shapes

| Route | Request / precondition | Success and policy |
| --- | --- | --- |
| POST `/tenancies` | `{syntheticPropertyLabel, startsOn, endsOn?, creatorRole}`; creatorRole Tenant or Landlord; no If-Match | 201 Tenancy Draft + own active Membership + provenance receipt; Location `/tenancies/{id}`. Creator role is a proposal, not verification of legal status. |
| GET `/tenancies/{id}` | Visible party membership | 200 tenancy and ETag; reviewer sees only grant-permitted minimal view, not participants' private details |
| POST `/tenancies/{id}/memberships` | `{target:{directoryTenantId,objectId}, role, expiresAt, reportIds?}`; If-Match tenancy ETag | 201 invitation membership; Location nested membership URI. Opposite-party invite only while Draft; reviewer invite PendingApproval, requires finite exact report IDs and future expiry. Does NOT immediately grant access. |
| GET `/tenancies/{id}/memberships/{membershipId}` | Authorized party; bound invite target may see only own invitation | 200 membership + membership ETag |
| POST `/tenancies/{id}/memberships/{membershipId}/acceptance` | `{}`; If-Match membership ETag | 200 bound identity accepts; opposite party becomes Active and tenancy becomes Active atomically; reviewer remains PendingApproval until both party approvals and eligibility check |
| POST `/tenancies/{id}/memberships/{membershipId}/approvals` | `{}`; If-Match membership ETag | 200 adds caller's party approval for exact reviewer grant; membership activates only once target accepted, both approvals exist, eligibility is approved, and expiry is in future |
| DELETE `/tenancies/{id}/memberships/{membershipId}` | No body; If-Match membership ETag | 200 revoked reviewer + provenance receipt; either active party or reviewer self relinquishing. Party removal is 409 unsupported_membership_succession, not a generic role change. |
| GET `/tenancies/{id}/events/{eventId}` | Active party; tenancy-filtered event lookup | 200 event + ETag; reviewer denied without any unrestricted event scope |
| POST `/tenancies/{id}/events` | `{eventType, description, occurredAt, supersedesEventId?}`; If-Match tenancy ETag | 201 immutable-content event at Unverified/version 1, genesis transition, provenance and receipt. Active party only. Source is set UserSubmitted. |
| POST `/tenancies/{id}/events/{eventId}/transitions` | `{action, reason?}` where action Acknowledge/Dispute/WithdrawDispute; If-Match event ETag | 201 transition plus event snapshot/receipt; enforce documented edge/actor guards |
| POST `/tenancies/{id}/events/{eventId}/resolution-consents` | `{resolutionText}`; If-Match event ETag | 201 consent plus event snapshot/receipt; first party does not change status, second matching party atomically adds Resolved transition/version. Requires Acknowledged or Disputed. Changed text has different hash and cannot satisfy old consent. |

For create event, use tenancy If-Match to detect known membership/lifecycle changes;
auth is still evaluated from locked SQL state. The tenancy ETag need not change on
every event write; event updates have independent ETags. Membership mutations always
advance Tenancy.AccessVersion and tenancy ETag in addition to their own ETag.
Invitations may target only a configured allowed directory and principal identifier;
do not add an identity-search endpoint that reveals email membership. Delivery of
invitation notices and proof of reviewer eligibility remain implementation blockers.

Initial proposed bounds: description <= 8,000 characters, eventType from a reviewed
allowlist (e.g. MaintenanceReported for fixtures), resolutionText <= 4,000 characters,
syntheticPropertyLabel <= 200 characters, future invite expiry <= 30 days. Reject
invalid/future event occurrence beyond configured clock-skew tolerance; that tolerance,
report grant maximum duration and event-type catalogue require product approval.
These are proposed API limits, not undocumented existing NFRs.

## Synthetic event example

```http
POST /api/v1/tenancies/10000000-0000-4000-8000-000000000001/events
Authorization: Bearer <runtime-user-token>
Idempotency-Key: synthetic-example-command-0001
If-Match: "opaque-tenancy-version"
Content-Type: application/json

{"eventType":"MaintenanceReported","description":"Synthetic tap leak fixture","occurredAt":"2026-11-12T08:00:00Z"}
```

Illustrative 201 response; IDs and ETag are server-generated, not real credentials:

```json
{
  "commandId": "20000000-0000-4000-8000-000000000001",
  "tenancyId": "10000000-0000-4000-8000-000000000001",
  "eventId": "30000000-0000-4000-8000-000000000001",
  "status": "Unverified",
  "version": 1,
  "provenanceEntryId": "40000000-0000-4000-8000-000000000001",
  "recordedAt": "2026-11-12T08:00:01Z"
}
```

Location is the nested event URI; ETag is its committed event rowversion encoding.
Receipt responses use entry IDs, not a fabricated SQL Ledger block ID. Every mutation
returns commandId, provenanceEntryId, affected resource representation and any generated
transition ID; resolution response includes `statusChanged` and nullable transitionId.
No response claims the counterparty agrees merely because the event was stored.

## Unknown outcome and replay

A commit timeout can return 503 with code `outcome_unknown`, `retryWithSameKey: true`
and traceId. Do not return a new event ID that may not exist or state “not accepted.”
The client repeats the EXACT operation/key/body/If-Match; a receipt resolves to the
original 201/200 even if its snapshot ETag is now old. Lost-access callers get 404
rather than the receipt. Different caller or resource never reuses another scope's
receipt. For known rollback, 503 `write_not_committed` is safe, still same-key retry.
No dedicated unauthenticated command-status endpoint is introduced.

## Internal contract boundary

Workflow SQL command execution is not a public REST endpoint. Evidence internal
HTTP still requires allowlisted service identities/application roles, separately
from the originating user's tenancy grant. Never accept a public request to append
arbitrary provenance or caller-supplied actor identity. Evidence's existing proposed
attachment/integrity endpoints are not expanded into a generic event commit endpoint.
API/OpenAPI schema generation and compatibility tests are future implementation work.
