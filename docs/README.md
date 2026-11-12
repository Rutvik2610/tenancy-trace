# TenancyTrace architecture documentation pack

## Provenance and scope

This pack was assembled from the user's material shared in the ChatGPT conversation on November 11–12, 2026. The original five process flows, three state models, and five L2/L3 Mermaid diagrams are transcribed from the text shared in chat. The L1 was supplied as an image and is preserved as such. The five ADRs are transcribed from four source photographs, which are included for review. **Do not reinterpret this package as approval of unresolved architecture decisions.**

## Inventory (19 source artifacts)

| Category | Original count | Where to find it |
|---|---:|---|
| L1 conceptual diagram | 1 | `architecture/l1-context.md`, `source-images/l1-original.png` |
| L2 diagrams | 2 | `architecture/l2-*.md` |
| L3 diagrams | 3 | `architecture/l3-*.md` |
| Process sequence flows | 5 | `process-flows/*.md` |
| State diagrams | 3 | `state-models/*.md` |
| ADRs | 5 | `adr/ADR-*.md` and `source-images/adr-page-*.jpg` |

## Source-first policy

1. Treat the diagram text and ADR decisions as the current design baseline, not as implemented functionality.
2. Keep ADR-001 **Proposed** until the original approval requirement has been met. ADR-002 through ADR-005 were marked **Accepted** in the originals.
3. Preserve original source files when proposing corrections. Record refinements in a separate design-review note or new ADR; do not silently rewrite source decisions.
4. The L1 image has no original Mermaid text in this pack. Do not describe a reconstructed diagram as original source.
5. ADR text was transcribed from photographs. Compare with adjacent `source-images/adr-page-*.jpg` before treating the transcriptions as authoritative.
6. The first L2 diagram contains an unresolved `style LEG` reference from the original Mermaid. This is preserved intentionally.

## Known architecture tensions — for review, not silently resolved

- Event acceptance crosses Workflow and Evidence Function Apps; a ledger commit followed by workflow state failure needs idempotent recovery and an explicit ownership protocol.
- Report and AI job database writes plus queue sends require the ADR-003 transactional-outbox mechanism, whereas some process flows show direct queue sends.
- Cross-tenancy denial codes differ (`403` in earlier flows vs `404` in Flow 5). Agree on a non-disclosing API policy.
- Summary `Approved` state and `SummaryApproved` ledger append are separate commits; durable reconciliation is required.
- Blob files and SQL ledger cannot be committed as one ordinary atomic transaction; address orphan cleanup and access gating.
- Ledger digest export cadence, checkpoint freshness and nightly verification need operational contracts.
- Distinguish inline file validation from malware scanning per ADR-004.
- Report job `Ready` vs `ReportGenerated` audit append needs a committed-completion protocol.

## Repository navigation

- [Phase 2A domain model and critical consistency design — Proposed](design/phase-2a/README.md)

- [Phase 1 architecture and implementation boundaries](architecture/README.md)
- [Phase 1 runtime validation](architecture/validation.md)
- [Import review: transcription comparison, open issues, and Mermaid validation](architecture/import-review.md)

### Architecture diagrams

- [L1 conceptual architecture (original image)](architecture/l1-context.md)
- [L2 request path and data](architecture/l2-request-data.md)
- [L2 messaging, integrity and AI](architecture/l2-messaging-ai.md)
- [L3 request path and data access](architecture/l3-request-data.md)
- [L3 messaging, workers, integrity and AI](architecture/l3-workers-integrity.md)
- [L3 Evidence and Provenance Service detail](architecture/l3-evidence-service.md)

### Process flows

- [1. Record an event with evidence](process-flows/01-event-with-evidence.md)
- [2. Generate an evidence report](process-flows/02-evidence-report.md)
- [3. Generate and review an AI summary](process-flows/03-ai-summary.md)
- [4. Detect a simulated unauthorised change](process-flows/04-tamper-detection.md)
- [5. Block cross-tenancy access](process-flows/05-cross-tenancy-access.md)

### State models

- [Event](state-models/event.md)
- [Report job](state-models/report-job.md)
- [AI summary](state-models/ai-summary.md)

### Architecture decisions and original screenshots

| ADR | Status in original | Transcription | Source photographs |
| --- | --- | --- | --- |
| ADR-001 | Proposed | [Bounded, human-reviewed AI summarization](adr/ADR-001.md) | [Page 1](source-images/adr-page-1.jpg), [page 2](source-images/adr-page-2.jpg) |
| ADR-002 | Accepted | [SQL Ledger and content hashing](adr/ADR-002.md) | [Page 2](source-images/adr-page-2.jpg) |
| ADR-003 | Accepted | [Functions, queues, transactional outbox](adr/ADR-003.md) | [Page 2: heading/status](source-images/adr-page-2.jpg), [page 3: body](source-images/adr-page-3.jpg) |
| ADR-004 | Accepted | [Pluggable Scan Gate](adr/ADR-004.md) | [Page 3](source-images/adr-page-3.jpg), [page 4](source-images/adr-page-4.jpg) |
| ADR-005 | Accepted | [Fail closed on unavailable ledger](adr/ADR-005.md) | [Page 4](source-images/adr-page-4.jpg) |

The supplied source documents and images are retained unchanged, except for this
index. The five ADR transcriptions replace the Phase 1 placeholders. The repository
README links to this index, and the existing architecture overview has an additive
navigation note. Source code, solution, tests, and CI are unchanged.
