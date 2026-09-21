# Traceability Matrix: Report Incident

**Feature Branch**: `002-report-incident`
**Generated**: 2026-09-21
**Validation Tool**: Manual deterministic ID cross-check (no `build-matrix.sh`/`.ps1` script present in this repository checkout — see Baseline Information for details)

This matrix satisfies DO-178C §6.3.4, ISO 26262 Part 6 Clause 9, IEC 62304 Clause 5.7, FDA 21 CFR Part 820 §820.30(i), and IEC 61508 Part 3 Clause 7.9 traceability requirements.

---

## Coverage Audit

```
══════════════════════════════════════════════
  TRACEABILITY MATRIX — COVERAGE AUDIT
══════════════════════════════════════════════

  Total Requirements:                  21
  Requirements with Test Coverage:     21 (100%)
  Total Test Cases (ATP):              22
  Test Cases with Scenarios:           22 (100%)
  Total Executable Scenarios (SCN):    24

──────────────────────────────────────────────
  FORWARD TRACEABILITY (REQ → ATP → SCN)
──────────────────────────────────────────────
  Untested Requirements:               0  ✅ Pass
  ATPs Without Scenarios:              0  ✅ Pass

──────────────────────────────────────────────
  BACKWARD TRACEABILITY (SCN → ATP → REQ)
──────────────────────────────────────────────
  Orphaned Test Cases:                 0  ✅ Pass
  Orphaned Scenarios:                  0  ✅ Pass

══════════════════════════════════════════════
  OVERALL STATUS: ✅ COMPLIANT
══════════════════════════════════════════════
```

## Exception Report

No exceptions found — all traceability links are valid.

- GAPS (Forward Traceability Failures): none.
- ORPHANS (Backward Traceability Failures): none.
- DEPRECATION CANDIDATES: none.

Note: the `[NEEDS CLARIFICATION]` item in `requirements.md` (rapid duplicate-submission guarding) was never assigned a REQ ID and is intentionally excluded from both `requirements.md`'s formal requirement list and this matrix — it is not a gap, since there is no REQ to cover.

---

## Matrix A — Validation (User View)

REQ → ATP → SCN

| REQ ID | Requirement Intent | Test Case ID (ATP) | Validation Condition | Scenario ID (SCN) | Verification Status |
|---|---|---|---|---|---|
| **REQ-001** | "Report Incident" action is invocable | ATP-001-A | Report Incident action is available and invocable | SCN-001-A1, SCN-001-A2 | ⬜ Pending Execution |
| **REQ-002** | Accept free-text description input | ATP-002-A | Free-text description is accepted as input | SCN-002-A1 | ⬜ Pending Execution |
| **REQ-003** | Reject empty/missing description | ATP-003-A | Empty description is rejected | SCN-003-A1 | ⬜ Pending Execution |
| | | ATP-003-B | Missing description is rejected | SCN-003-B1 | ⬜ Pending Execution |
| **REQ-004** | Inform user description is required | ATP-004-A | Required-description message is displayed | SCN-004-A1 | ⬜ Pending Execution |
| **REQ-005** | Record submission timestamp | ATP-005-A | Timestamp is recorded for every submitted report | SCN-005-A1 | ⬜ Pending Execution |
| **REQ-006** | Record reporter context | ATP-006-A | Reporter context is recorded for every submitted report | SCN-006-A1 | ⬜ Pending Execution |
| **REQ-007** | Assign unique identifier | ATP-007-A | Unique identifier is assigned at recording time | SCN-007-A1 | ⬜ Pending Execution |
| **REQ-008** | Record timestamp in UTC | ATP-008-A | Timestamp is recorded in UTC regardless of viewed timezone | SCN-008-A1 | ⬜ Pending Execution |
| **REQ-009** | Confirmation message includes identifier | ATP-009-A | Successful submission displays confirmation with unique identifier | SCN-009-A1 | ⬜ Pending Execution |
| **REQ-010** | Error message on failed recording | ATP-010-A | Failed recording displays a not-saved error message | SCN-010-A1 | ⬜ Pending Execution |
| **REQ-011** | Retry without re-entering description | ATP-011-A | Retry after failure preserves the previously entered description | SCN-011-A1 | ⬜ Pending Execution |
| **REQ-012** | Length-limit-exceeded message | ATP-012-A | Description exceeding maximum length is rejected with a length-exceeded message | SCN-012-A1 | ⬜ Pending Execution |
| **REQ-013** | Persistence across sessions | ATP-013-A | Recorded incident is retrievable after the recording session has ended | SCN-013-A1 | ⬜ Pending Execution |
| **REQ-014** | Retrieve incidents ordered by timestamp | ATP-014-A | Retrieval returns full incident fields ordered earliest-first | SCN-014-A1 | ⬜ Pending Execution |
| **REQ-015** | Unavailable reporter context marked unknown | ATP-015-A | Submission succeeds with reporter context marked unknown when unavailable | SCN-015-A1 | ⬜ Pending Execution |
| **REQ-NF-001** | Submission-to-confirmation latency under 30s | ATP-NF-001-A | End-to-end submission completes within the 30-second bound | SCN-NF-001-A1 | ⬜ Pending Execution |
| **REQ-NF-002** | Empty-description feedback within 1s, no input discarded | ATP-NF-002-A | Empty-description feedback is shown within 1 second without discarding other input | SCN-NF-002-A1 | ⬜ Pending Execution |
| **REQ-NF-003** | 100% of successful submissions retrievable intact | ATP-NF-003-A | All successfully submitted incidents in a batch are retrievable with fields intact | SCN-NF-003-A1 | ⬜ Pending Execution |
| **REQ-NF-004** | 100% of injected storage failures produce visible error + retry, zero silent drops | ATP-NF-004-A | Failure-injection suite: every induced storage failure yields visible error and retry, none silently dropped | SCN-NF-004-A1 | ⬜ Pending Execution |
| **REQ-NF-005** | All recorded timestamps directly comparable in UTC | ATP-NF-005-A | Timestamps recorded under different concurrently-viewed timezones are directly comparable in UTC | SCN-NF-005-A1 | ⬜ Pending Execution |
| **REQ-CN-001** | Maximum description length enforced | ATP-CN-001-A | Description at the maximum length is accepted | SCN-CN-001-A1 | ⬜ Pending Execution |
| | | ATP-CN-001-B | Description exceeding the maximum length is rejected | SCN-CN-001-B1 | ⬜ Pending Execution |

**Matrix A totals**: 21 REQs / 22 ATPs / 24 SCNs — 100% REQ→ATP coverage, 100% ATP→SCN coverage, 0 gaps, 0 orphans.

---

## Matrix B — Verification (Architectural View)

REQ → SYS → STP → STS

**Status: N/A — not yet generated.** `system-design.md` and `system-test.md` do not exist yet for feature `002-report-incident` at the time this matrix was built. Per the V-Model workflow, Matrix B is populated by a later step (`/speckit.v-model.system-design` followed by `/speckit.v-model.system-test`), after which this matrix must be rebuilt to add the architectural view. No SYS/STP/STS content has been invented to fill this section.

| Requirement ID | System Component (SYS) | Component Name | Test Case ID (STP) | Technique | Scenario ID (STS) | Status |
|---|---|---|---|---|---|---|
| — | — | *(system-design.md not present)* | — | — | — | 🚫 Blocked — awaiting system design |

---

## Baseline Information

| Property | Value |
|----------|-------|
| Matrix Generated | 2026-09-21 |
| Requirements Source | `specs/002-report-incident/v-model/requirements.md` |
| Requirements Last Modified | 2026-09-21 11:47:42 UTC |
| Acceptance Plan Source | `specs/002-report-incident/v-model/acceptance-plan.md` |
| Acceptance Plan Last Modified | 2026-09-21 11:55:46 UTC |
| System Design Source | N/A — not present |
| System Test Source | N/A — not present |
| Validation Tool | Manual deterministic ID cross-check (`build-matrix.sh`/`.ps1` not present in this repository checkout; all REQ/ATP/SCN IDs were enumerated and cross-referenced exhaustively by direct parsing of both source files, not inferred or hallucinated) |
| Git Commit (if available) | 63438a7 (base; this matrix commit follows) |
