# Proposed authorization policy

## Independent identity and access decisions

1. User HTTP APIs validate Entra signature, issuer, audience, lifetime and required
   delegated scopes before resource work. Use validated directory/object identity;
   a valid JWT or global Tenant role grants no tenancy access by itself.
2. Workflow authorizes the operation against current Membership, lifecycle, expiry,
   grant scope and event actor rules. Every lookup is constrained by TenancyId AND
   resource ID. Recheck access inside the SQL command transaction, not only in HTTP
   middleware. Scope query joins and lists too; opaque identifiers do not prevent IDOR.
3. Evidence internal HTTP APIs independently validate service audience, issuer,
   lifetime, allowed application identity and operation-specific application role
   (e.g. Evidence.Read, Evidence.Write, Integrity.Verify). Reject user tokens and
   unapproved services. No function key or forwarded user-supplied header is proof.
4. Service role authorization is necessary but insufficient to act for a user.
   Workflow establishes the user/tenancy decision; Evidence receives server-built
   actor, tenancy, operation and resource context and verifies it through a trusted,
   authenticated Workflow access check. A caller cannot assert its own tenancy grant.
   Read/stream operations recheck expiry/revocation; no long-lived authorization
   cache is sufficient. Signed delegated capabilities may be a future alternative,
   but TTL, binding and revocation semantics require explicit approval.
5. The critical SQL module authenticates the Workflow database principal separately.
   It accepts actor identity from the trusted Workflow host and performs tenancy
   checks against Workflow tables under locks. SQL cannot verify a forwarded JWT
   merely from actor parameters. A compromised trusted host remains a threat and
   must be constrained/audited; do not describe this as end-to-end cryptographic
   proof of the user. Evidence HTTP is not called in this transaction proposal.

## Capability matrix

| Action | Active Tenant | Active Landlord | Approved Reviewer |
| --- | --- | --- | --- |
| Create synthetic tenancy | Yes, self as Tenant | Yes, self as Landlord | No reviewer-only creation privilege |
| Read visible tenancy metadata/timeline | Own tenancy | Own tenancy | Only minimum metadata/timeline included in named authorized reports; no unrestricted event browse |
| Submit event/evidence | Yes | Yes | No |
| Acknowledge/dispute/withdraw | Domain actor guards apply | Domain actor guards apply | No |
| Consent to resolution | Yes, once per current version/hash | Yes, once per current version/hash | No |
| Invite opposite party | Yes during Draft, cannot grant same role | Same | No |
| Accept invitation | Bound target principal only | Bound target principal only | Bound target plus separate reviewer eligibility |
| Invite reviewer | Yes, requires other party approval | Same | No |
| Approve reviewer grant | Yes; cannot count as Landlord approval | Yes; cannot count as Tenant approval | No self-approval |
| Revoke reviewer | Either party immediately | Either party immediately | May relinquish own grant |
| Request/read/download report | Own tenancy, subject to report sharing policy | Same | Only explicitly granted ReportIds, unexpired and approved; new report generation is denied until separately scoped policy is approved |
| Read AI draft/approve AI | Requester-only if later approved AI design permits | Same | No broad draft access; original ADR-001 remains Proposed |

Party revocation/replacement is not a generic membership patch: deny it in this
proposal until succession/consent policy exists. It must not become a landlord's
ability to erase the tenant's access/history. Reviewer grant approval binds exact
identity, report set and expiry; changing any of these resets both approvals and
requires new target acceptance. Validate every granted ReportId belongs to the
same tenancy through an authenticated authoritative report lookup before grant
approval; report IDs cannot widen the tenancy scope. Reviewers have no direct blob URLs or Evidence API
access. Eligibility is a separate approved-reviewer directory check; tenancy
membership alone cannot manufacture eligibility. If eligibility authority is not
defined, reviewer activation remains disabled.

## Consistent resource responses

| Situation | Response |
| --- | --- |
| Missing/invalid authentication | 401, standard challenge, no existence lookup |
| Valid identity, nonexistent tenancy/resource, wrong tenancy, revoked/expired membership, or resource outside reviewer scope | 404 with identical generic Problem Details |
| Resource visible under current access but action not allowed (e.g. party acknowledging own event) | 403 generic insufficient_permission |
| Invalid transition on an accessible event | 409 invalid_transition |
| Missing If-Match / stale If-Match on accessible resource | 428 / 412 |
| Access database/check unavailable | 503, never a cached allow or a fabricated 404 |

Apply authorization before returning state/ETag/idempotency/body-validation details
that reveal resource existence. Global malformed request size/JSON checks and
authentication may happen first. Resource visibility precedes key replay lookup;
a previously authorized user who lost access receives 404, not their old response.
List endpoints return only authorized rows (possibly empty); never disclose global
counts. Responses carry Cache-Control: no-store. Use uniform error shapes and avoid
obvious existence timing differences; this does not claim timing-proof behavior.
Denials log correlation ID, principal, requested tenancy, route and code with
restricted retention; no evidence payloads/tokens. Log failure must not turn denial
into authorization success.

Invitation target has a narrow exception: can view/accept only an invitation bound
to that authenticated identity, not tenancy history or other memberships. Expired,
wrong-target and missing invitations all return the same 404. Permission checks for
revocation and mutation are serialized with writes via the tenancy lock protocol.
This policy chooses Flow 5's non-disclosure behavior and clarifies the older 403
paths; originals are not rewritten.
