# Traceability Matrix: Batch Timezone Conversion

**Feature Branch**: `003-batch-timezone-conversion`
**Generated**: 2026-09-21
**Validation Tool**: Manual deterministic ID cross-check (no `build-matrix.sh`/`.ps1` script present in this repository checkout — see Baseline Information for details)

This matrix satisfies DO-178C §6.3.4, ISO 26262 Part 6 Clause 9, IEC 62304 Clause 5.7, FDA 21 CFR Part 820 §820.30(i), and IEC 61508 Part 3 Clause 7.9 traceability requirements.

---

## Coverage Audit

```
══════════════════════════════════════════════
  TRACEABILITY MATRIX — COVERAGE AUDIT
══════════════════════════════════════════════

  Total Requirements:                  34
  Requirements with Test Coverage:     34 (100%)
  Total Test Cases (ATP):              35
  Test Cases with Scenarios:           35 (100%)
  Total Executable Scenarios (SCN):    37

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

Note: the three `[NEEDS CLARIFICATION]` items in `requirements.md` (explicit-offset-vs-source-timezone inconsistency reporting; a maximum file-size ceiling above the 10,000-row floor; exact CSV header column naming) were never assigned REQ IDs and are intentionally excluded from both `requirements.md`'s formal requirement list and this matrix — they are not gaps, since there is no REQ to cover.

---

## Matrix A — Validation (User View)

REQ → ATP → SCN

| REQ ID | Requirement Intent | Test Case ID (ATP) | Validation Condition | Scenario ID (SCN) | Verification Status |
|---|---|---|---|---|---|
| **REQ-001** | Accept CSV input with timestamp/source/target columns | ATP-001-A | Batch conversion accepts a CSV file with the required columns | SCN-001-A1 | ⬜ Pending Execution |
| **REQ-002** | Identify columns via header, not fixed order | ATP-002-A | Columns are identified by header name regardless of order | SCN-002-A1 | ⬜ Pending Execution |
| **REQ-003** | Accept IANA timezone identifiers | ATP-003-A | IANA timezone identifiers are accepted for source and target | SCN-003-A1 | ⬜ Pending Execution |
| **REQ-004** | Ignore unrecognized extra columns | ATP-004-A | Extra unrecognized columns are ignored and the row still converts | SCN-004-A1 | ⬜ Pending Execution |
| **REQ-005** | Convert valid rows applying DST rules | ATP-005-A | Valid row is converted from source to target timezone with DST applied | SCN-005-A1 | ⬜ Pending Execution |
| **REQ-006** | Rows processed independently | ATP-006-A | One row's failure does not affect another row's success | SCN-006-A1 | ⬜ Pending Execution |
| **REQ-007** | Output record contains original values and converted timestamp | ATP-007-A | Successful output record includes original input and converted date/time/timezone | SCN-007-A1 | ⬜ Pending Execution |
| **REQ-008** | Validate timestamp is parseable | ATP-008-A | Unparseable timestamp fails validation before conversion | SCN-008-A1 | ⬜ Pending Execution |
| **REQ-009** | Validate source timezone is recognized | ATP-009-A | Unrecognized source timezone fails validation before conversion | SCN-009-A1 | ⬜ Pending Execution |
| **REQ-010** | Validate target timezone is recognized | ATP-010-A | Unrecognized target timezone fails validation before conversion | SCN-010-A1 | ⬜ Pending Execution |
| **REQ-011** | Continue processing after a row fails | ATP-011-A | Batch continues processing subsequent rows after an invalid row | SCN-011-A1 | ⬜ Pending Execution |
| **REQ-012** | Invalid-row report includes row number | ATP-012-A | Invalid-row report entry references the correct row number | SCN-012-A1 | ⬜ Pending Execution |
| **REQ-013** | Report unparseable-timestamp reason | ATP-013-A | Invalid-row reason references the timestamp field for unparseable timestamps | SCN-013-A1 | ⬜ Pending Execution |
| **REQ-014** | Report unrecognized-timezone reason with the offending value | ATP-014-A | Invalid-row reason references the timezone field and identifies the unrecognized value | SCN-014-A1 | ⬜ Pending Execution |
| **REQ-015** | Report missing-field reason identifying which field | ATP-015-A | Invalid-row reason identifies the specific missing field | SCN-015-A1 | ⬜ Pending Execution |
| **REQ-016** | Distinct file-level error when file cannot be opened/read | ATP-016-A | Unreadable input file produces a distinct file-level error | SCN-016-A1 | ⬜ Pending Execution |
| **REQ-017** | Distinct file-level error when required columns cannot be identified | ATP-017-A | Unidentifiable header columns produce a distinct file-level error | SCN-017-A1 | ⬜ Pending Execution |
| **REQ-018** | No row processing after a file-level error | ATP-018-A | No rows are processed once a file-level error occurs | SCN-018-A1 | ⬜ Pending Execution |
| **REQ-019** | Successful output available in structured machine-readable form | ATP-019-A | Successful rows are available as structured, machine-readable output | SCN-019-A1 | ⬜ Pending Execution |
| **REQ-020** | Invalid-row report kept separate from successful output | ATP-020-A | Invalid-row report is a distinct output from the successful conversion output | SCN-020-A1 | ⬜ Pending Execution |
| **REQ-021** | Run summary reports total/succeeded/failed counts | ATP-021-A | Run summary reports total, succeeded, and failed row counts | SCN-021-A1 | ⬜ Pending Execution |
| **REQ-022** | Zero failed count displayed as zero, not omitted | ATP-022-A | All-succeeded run displays a failed count of zero rather than omitting it | SCN-022-A1 | ⬜ Pending Execution |
| **REQ-023** | Header-only file completes cleanly with empty outputs | ATP-023-A | Header-only CSV completes without error, producing empty outputs and a no-rows message | SCN-023-A1 | ⬜ Pending Execution |
| **REQ-024** | No explicit offset: source timezone is authoritative | ATP-024-A | Timestamp without explicit UTC offset is interpreted as local to the source timezone | SCN-024-A1 | ⬜ Pending Execution |
| **REQ-025** | Converted output includes the correct resulting date across a date boundary | ATP-025-A | Conversion crossing a date boundary reports the correct resulting calendar date | SCN-025-A1 | ⬜ Pending Execution |
| **REQ-026** | DST transitions resolved using standard rules, not treated as invalid | ATP-026-A | Spring-forward gap timestamp is resolved, not rejected | SCN-026-A1 | ⬜ Pending Execution |
| | | ATP-026-B | Fall-back ambiguous-hour timestamp is resolved, not rejected | SCN-026-B1 | ⬜ Pending Execution |
| **REQ-027** | Identical source and target timezone still produces an unchanged result | ATP-027-A | Identical source and target timezone yields a successful, unchanged conversion | SCN-027-A1 | ⬜ Pending Execution |
| **REQ-028** | Entirely blank rows are skipped, not counted | ATP-028-A | Entirely blank row is skipped and counted in neither succeeded nor failed totals | SCN-028-A1 | ⬜ Pending Execution |
| **REQ-029** | Unwritable output destination is reported without discarding computed results | ATP-029-A | Write failure to the output destination is reported without discarding already-computed conversions | SCN-029-A1 | ⬜ Pending Execution |
| **REQ-NF-001** | 100% of valid rows return a converted result regardless of invalid-row proportion | ATP-NF-001-A | All valid rows in a mixed batch produce converted results | SCN-NF-001-A1 | ⬜ Pending Execution |
| **REQ-NF-002** | 100% of invalid rows appear as distinct, uniquely identified report entries | ATP-NF-002-A | Every invalid row in a batch appears as a distinct report entry with a unique row number | SCN-NF-002-A1 | ⬜ Pending Execution |
| **REQ-NF-003** | Summary's two counts alone classify the run as fully succeeded, fully failed, or partial | ATP-NF-003-A | Succeeded and failed counts alone allow classifying the run outcome | SCN-NF-003-A1, SCN-NF-003-A2, SCN-NF-003-A3 | ⬜ Pending Execution |
| **REQ-NF-004** | Minute-accurate conversion within the -10y/+2y window, including DST | ATP-NF-004-A | Converted timestamp within the supported date window is accurate to the minute including DST | SCN-NF-004-A1 | ⬜ Pending Execution |
| **REQ-NF-005** | 10,000-row batch completes in a single run | ATP-NF-005-A | A 10,000-row CSV file is processed in a single run without splitting | SCN-NF-005-A1 | ⬜ Pending Execution |

**Matrix A totals**: 34 REQs / 35 ATPs / 37 SCNs — 100% REQ→ATP coverage, 100% ATP→SCN coverage, 0 gaps, 0 orphans.

---

## Matrix B — Verification (Architectural View)

REQ → SYS → STP → STS

**Status: N/A — not yet generated.** `system-design.md` and `system-test.md` do not exist yet for feature `003-batch-timezone-conversion` at the time this matrix was built. Per the V-Model workflow, Matrix B is populated by a later step (`/speckit.v-model.system-design` followed by `/speckit.v-model.system-test`), after which this matrix must be rebuilt to add the architectural view. No SYS/STP/STS content has been invented to fill this section.

| Requirement ID | System Component (SYS) | Component Name | Test Case ID (STP) | Technique | Scenario ID (STS) | Status |
|---|---|---|---|---|---|---|
| — | — | *(system-design.md not present)* | — | — | — | 🚫 Blocked — awaiting system design |

---

## Baseline Information

| Property | Value |
|----------|-------|
| Matrix Generated | 2026-09-21 |
| Requirements Source | `specs/003-batch-timezone-conversion/v-model/requirements.md` |
| Requirements Last Modified | 2026-09-21 12:24:46 UTC |
| Acceptance Plan Source | `specs/003-batch-timezone-conversion/v-model/acceptance-plan.md` |
| Acceptance Plan Last Modified | 2026-09-21 12:29:00 UTC |
| System Design Source | N/A — not present |
| System Test Source | N/A — not present |
| Validation Tool | Manual deterministic ID cross-check (`build-matrix.sh`/`.ps1` not present in this repository checkout; all REQ/ATP/SCN IDs were enumerated and cross-referenced exhaustively by direct parsing of both source files, not inferred or hallucinated) |
| Git Commit (if available) | 06922fa (base; acceptance-plan.md and this matrix are uncommitted) |
