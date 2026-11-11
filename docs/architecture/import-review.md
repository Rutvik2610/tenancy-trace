# Architecture documentation import review

## Scope and source preservation

Imported from `TenancyTrace-Architecture-Documentation-Pack.zip` for Phase 1.5.
The package contains 19 logical artifacts: one L1 image, two L2 diagrams, three L3
diagrams, five process flows, three state models, and five ADRs. Four ADR photographs
provide the transcription evidence. There are 13 Mermaid blocks; L1 is an image,
not a reconstructed Mermaid diagram.

All supplied diagram/flow/state/ADR Markdown and all five images are copied byte for
byte. Only the supplied documentation index is adapted for repository navigation.
The existing Phase 1 architecture overview receives an additive historical/navigation
note; its baseline content and `validation.md` are preserved. Source code, projects,
solution, tests, CI, dependencies and local configuration are unchanged.

Statements about sandbox services, provisioning, guarantees and approval in the
package describe the original design context. They are not commands to provision
resources, evidence of implementation, or new approvals. No resources are provisioned
and no business logic is implemented in this import.

## Screenshot comparison

All four ADR photographs were visually compared against the five supplied Markdown
transcriptions, including options, decisions, consequences and trade-offs. The visible
text is readable; no unresolved illegible decision passage was found. This is a human
visual comparison, not a certified verbatim transcription or an approval audit.

| ADR | Source reviewed | Finding |
| --- | --- | --- |
| 001 | Pages 1–2 | Proposed, with written mentor approval required for acceptance. Option D and all seven decision bullets match in substance. The context changes “is about the deterministic report” to “applies to the deterministic report”; risk wording changes “try to manipulate the model” to “attempt prompt manipulation.” These are paraphrases, not exact quotations. |
| 002 | Page 2 | Accepted. Option D, append-only state history, intake hashing, verification cadence, immutable digest export and detection-not-prevention qualification match. Formatting and punctuation are editorial. |
| 003 | Page 2 heading/status and page 3 body | Accepted. Option C, three apps/queues, transactional outbox, idempotent consumers and 270-second deadline match. “within a 5-minute p95” is expanded to “within a five-minute p95 objective.” The transcription cites page 3 only; the index also links page 2 for completeness. |
| 004 | Pages 3–4 | Accepted. Option D, 25 MB cap, type/signature checks, rejection audit, and future managed-scanner adapter match. “Align the scope sheet wording” is shortened to “Align wording”; no scope approval is implied. |
| 005 | Page 4 | Accepted. Option B, three retries with the same key, 503, and orphan reconciliation match. Capitalization/punctuation differ only editorially. |

Primary quality attributes in the transcriptions are supported by the overview table
on page 1. Added source links, headings and transcription disclaimers are editorial
metadata rather than photographed decision text. No substantive decision discrepancy
was identified. Transcriptions and statuses have not been silently corrected.
ADR-001 remains Proposed; this import does not supply written mentor approval.

## Architectural inconsistencies and open questions

These observations require design review; they do not amend the source decisions.

| Area | Sources | Issue to resolve before implementation |
| --- | --- | --- |
| Durable acceptance and recovery | ADR-005; Flow 1 | Ledger/outbox commit precedes Workflow state commit. Define durable idempotency scope, crash recovery and reconciliation; a retry must not create duplicate provenance or accept an uncommitted event. |
| Job enqueue atomicity | ADR-003; Flows 2–3; L3 workers C10/C13 | Flows draw database writes followed by direct queue sends, while ADR-003 requires transactional outbox for event and job messages. Define job outbox ownership and dispatch. |
| Summary validation retry | ADR-001; Flow 3; AI state model | ADR says the validator “rejects rather than repairs.” Flow 3 calls the model once more with stricter instructions after failed validation, then shows Rejected without a second-pass success branch. Confirm whether regeneration is permitted and define both outcomes. |
| Authorization and disclosure | Flows 1–3 vs Flow 5 | Denial responses differ: 403 vs non-disclosing 404. Flow 2 shows tenancy/job-owner checks on download but omits checks on polling; Flow 3 abbreviates authorization on read/discard. Define checks on every protected operation, including internal service roles plus tenancy context. |
| Reviewer access | L1; Flow 2 | L1 permits approved-reviewer report requests; Flow 2 restricts downloads to job owner with tenancy access. Define reviewer grants and sharing semantics, including approved summaries. |
| Completion audit | Flows 2–3 | Report Ready, Summary PendingReview and Approved commits precede separate ledger appends. Define publication gating and recovery if an audit append fails. |
| Blob/SQL boundary | Flow 1; L3 Evidence | Blob storage and SQL are not one ordinary atomic transaction. Best-effort deletion after failed append leaves crash/orphan windows. Define cleanup, access gating and attachment idempotency. |
| Validation rejection audit | ADR-004; Flow 1 | ADR requires rejected-file audit; Workflow's early 400/413 path shows no ledger append. Clarify which rejection paths must be audited, including ledger outage. |
| Ledger digest protection | ADR-002; L2 messaging; Flow 4 | “Tamper-proof” diagram labels are stronger than the ADR's tamper-evident detection claim. Specify independent protection, retention, export cadence, checkpoint freshness and compromised-identity boundaries. No Ledger guarantees are validated here. |
| Runtime/deadline semantics | ADR-003; L3 workers; Flow 2 | Five-minute timeout depends on hosting plan/configuration. Define whether the 270-second budget includes queue time and retries, plus lock renewal, delivery limits, dead-letter handling and p95 measurement. |
| Scan Gate scope | ADR-004; L3 Evidence | Inline checks are not comprehensive malware scanning. The test signature and 25 MB units need explicit definitions; the source scope-sheet alignment remains unresolved. |
| Event status transitions | Event state model | Acknowledged-to-Resolved has no guard, while Disputed-to-Resolved requires both parties. Specify actors, guards and provenance for each transition. |
| Source cross-references | L2 request/data; L3 Evidence | `style LEG` references an undefined legend. `SC2` is referenced without its scope-sheet definition in this package. The Evidence detail omits an explicit non-attachment ledger-write branch. Preserve these source gaps for review. |

## Validation

- All **13 Mermaid blocks parsed and rendered successfully** to SVG and PNG using
  Mermaid CLI 11.12.0's installed Mermaid 11.17.2 library and system Chromium. The CLI's
  `file://` entry page was blocked by Chromium administration policy. A temporary
  local HTTP server and Puppeteer loaded the same Mermaid library and rendered
  the unmodified blocks instead; browser policy was not changed.
- Rendered contact sheets for all five architecture diagrams, five process flows
  and three state models were visually inspected. The undefined `style LEG` in
  L2 request/data renders a stray `LEG` node with this renderer; it is retained
  in source and is not considered fixed by a successful render. Large diagrams
  need zooming; screenshot inspection is not a pixel-perfect layout certification.
- The L1 source image was visually reviewed and retained unchanged. No original
  Mermaid for L1 was supplied or inferred.
- All 24 supplied files other than the adapted index match the extracted package
  byte for byte, including ADRs and original images.
- Local Markdown/image link targets were checked for existence, and whitespace
  errors were checked with `git diff --check`.
- The final diff is limited to `README.md` and `docs/`. Application build/tests
  were not rerun for this documentation-only change. The separate Phase 1 runtime
  validation record remains historical, not a new validation claim.

Rendering validates syntax and layout generation, not the decisions, security,
Ledger guarantees, Azure deployment or the Mermaid version used by GitHub.
No GitHub UI rendering validation or architecture implementation tests were run.
Temporary tooling and rendered outputs are outside the checkout and are not part
of the import.
