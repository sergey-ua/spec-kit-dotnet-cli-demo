# Traceability Matrix: Batch Timezone Conversion

**Feature Branch**: `003-batch-timezone-conversion`
**Generated**: 2026-09-21 (refreshed against candidate commit `5291c59`)
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

**Matrix B (Verification: REQ → SYS → STP → STS) is now generated** (was previously "N/A — not yet generated"). Summary:

```
──────────────────────────────────────────────
  MATRIX B COVERAGE (REQ → SYS → STP → STS)
──────────────────────────────────────────────
  Requirements with SYS coverage:      34/34 (100%)  ✅ Pass
  SYS components with STP coverage:    11/11 (100%)  ✅ Pass
  STPs with STS coverage:              23/23 (100%)  ✅ Pass
  Orphaned SYS components:             0  ✅ Pass
  Orphaned STPs / STS:                 0  ✅ Pass
```

## Exception Report

No exceptions found — all traceability links are valid, in both Matrix A and Matrix B.

- GAPS (Forward Traceability Failures): none in Matrix A (REQ→ATP→SCN) or Matrix B (REQ→SYS→STP→STS).
- ORPHANS (Backward Traceability Failures): none in Matrix A (SCN→ATP→REQ) or Matrix B (STS→STP→SYS→REQ).
- DEPRECATION CANDIDATES: none.
- MISSING STP FOR EXISTING SYS: none — all 11 SYS components (SYS-001–SYS-011) have at least one STP test case in `system-test.md`, so there is nothing to send back to the system-test agent.

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

**Status: Generated.** `system-design.md` (11 SYS components) and `system-test.md` (23 STP / 33 STS) are now present for this feature. Every `(REQ, SYS)` pair below is taken directly from the Decomposition View (including the cross-reference addendum) in `system-design.md`; every `(STP, STS)` pair is taken from the `Verifies:` field of each STP in `system-test.md`. Rows are grouped by REQ; a REQ with fan-out to multiple SYS components, or a SYS component reached by multiple STPs that verify that REQ, produces multiple rows.

| Requirement ID | System Component (SYS) | Component Name | Test Case ID (STP) | Technique | Scenario ID (STS) | Status |
|---|---|---|---|---|---|---|
| **REQ-001** | SYS-001 | CSV File Intake | STP-001-A | Interface Contract Testing | STS-001-A1, STS-001-A2 | ⚠️ Partial — see Verification Summary (2026-09-21, `5291c59`) |
| **REQ-002** | SYS-002 | Header/Column Resolver | STP-002-A | Interface Contract Testing | STS-002-A1, STS-002-A2 | ✅ Passed (2026-09-21, `5291c59`) |
| | SYS-002 | Header/Column Resolver | STP-002-B | Equivalence Partitioning | STS-002-B1 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-003** | SYS-003 | Row Validator | STP-003-B | Equivalence Partitioning | STS-003-B1, STS-003-B2 | ✅ Passed (2026-09-21, `5291c59`) |
| | SYS-005 | Timezone Conversion Engine | STP-005-A | Interface Contract Testing | STS-005-A1, STS-005-A2 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-004** | SYS-002 | Header/Column Resolver | STP-002-A | Interface Contract Testing | STS-002-A1, STS-002-A2 | ✅ Passed (2026-09-21, `5291c59`) |
| | SYS-002 | Header/Column Resolver | STP-002-B | Equivalence Partitioning | STS-002-B1 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-005** | SYS-005 | Timezone Conversion Engine | STP-005-A | Interface Contract Testing | STS-005-A1, STS-005-A2 | ✅ Passed (2026-09-21, `5291c59`) |
| | SYS-005 | Timezone Conversion Engine | STP-005-C | Fault Injection | STS-005-C1 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-006** | SYS-006 | Row Processing Controller | STP-006-A | Fault Injection | STS-006-A1 | ✅ Passed (2026-09-21, `5291c59`) |
| | SYS-006 | Row Processing Controller | STP-006-B | Fault Injection | STS-006-B1 | ⚠️ No direct test match found |
| **REQ-007** | SYS-008 | Successful Result Formatter | STP-008-A | Interface Contract Testing | STS-008-A1 | ✅ Passed (2026-09-21, `5291c59`) |
| | SYS-008 | Successful Result Formatter | STP-008-B | Boundary Value Analysis | STS-008-B1 | ⚠️ No direct test match found |
| **REQ-008** | SYS-003 | Row Validator | STP-003-A | Interface Contract Testing | STS-003-A1, STS-003-A2 | ⚠️ Partial — see Verification Summary (2026-09-21, `5291c59`) |
| | SYS-003 | Row Validator | STP-003-B | Equivalence Partitioning | STS-003-B1, STS-003-B2 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-009** | SYS-003 | Row Validator | STP-003-A | Interface Contract Testing | STS-003-A1, STS-003-A2 | ⚠️ Partial — see Verification Summary (2026-09-21, `5291c59`) |
| | SYS-003 | Row Validator | STP-003-B | Equivalence Partitioning | STS-003-B1, STS-003-B2 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-010** | SYS-003 | Row Validator | STP-003-A | Interface Contract Testing | STS-003-A1, STS-003-A2 | ⚠️ Partial — see Verification Summary (2026-09-21, `5291c59`) |
| | SYS-003 | Row Validator | STP-003-B | Equivalence Partitioning | STS-003-B1, STS-003-B2 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-011** | SYS-005 | Timezone Conversion Engine | STP-005-C | Fault Injection | STS-005-C1 | ✅ Passed (2026-09-21, `5291c59`) |
| | SYS-006 | Row Processing Controller | STP-006-A | Fault Injection | STS-006-A1 | ✅ Passed (2026-09-21, `5291c59`) |
| | SYS-006 | Row Processing Controller | STP-006-B | Fault Injection | STS-006-B1 | ⚠️ No direct test match found |
| **REQ-012** | SYS-007 | Invalid Row Reporter | STP-007-A | Interface Contract Testing | STS-007-A1 | ✅ Passed (2026-09-21, `5291c59`) |
| | SYS-007 | Invalid Row Reporter | STP-007-B | Boundary Value Analysis | STS-007-B1 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-013** | SYS-003 | Row Validator | STP-003-A | Interface Contract Testing | STS-003-A1, STS-003-A2 | ⚠️ Partial — see Verification Summary (2026-09-21, `5291c59`) |
| | SYS-007 | Invalid Row Reporter | STP-007-A | Interface Contract Testing | STS-007-A1 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-014** | SYS-003 | Row Validator | STP-003-A | Interface Contract Testing | STS-003-A1, STS-003-A2 | ⚠️ Partial — see Verification Summary (2026-09-21, `5291c59`) |
| | SYS-007 | Invalid Row Reporter | STP-007-A | Interface Contract Testing | STS-007-A1 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-015** | SYS-003 | Row Validator | STP-003-A | Interface Contract Testing | STS-003-A1, STS-003-A2 | ⚠️ Partial — see Verification Summary (2026-09-21, `5291c59`) |
| | SYS-003 | Row Validator | STP-003-B | Equivalence Partitioning | STS-003-B1, STS-003-B2 | ✅ Passed (2026-09-21, `5291c59`) |
| | SYS-007 | Invalid Row Reporter | STP-007-A | Interface Contract Testing | STS-007-A1 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-016** | SYS-001 | CSV File Intake | STP-001-A | Interface Contract Testing | STS-001-A1, STS-001-A2 | ⚠️ Partial — see Verification Summary (2026-09-21, `5291c59`) |
| | SYS-001 | CSV File Intake | STP-001-B | Fault Injection | STS-001-B1 | ⚠️ No direct test match found |
| **REQ-017** | SYS-002 | Header/Column Resolver | STP-002-A | Interface Contract Testing | STS-002-A1, STS-002-A2 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-018** | SYS-001 | CSV File Intake | STP-001-A | Interface Contract Testing | STS-001-A1, STS-001-A2 | ⚠️ Partial — see Verification Summary (2026-09-21, `5291c59`) |
| | SYS-001 | CSV File Intake | STP-001-B | Fault Injection | STS-001-B1 | ⚠️ No direct test match found |
| | SYS-002 | Header/Column Resolver | STP-002-A | Interface Contract Testing | STS-002-A1, STS-002-A2 | ✅ Passed (2026-09-21, `5291c59`) |
| | SYS-011 | Batch Conversion Orchestrator | STP-011-A | Fault Injection | STS-011-A1, STS-011-A2 | ⚠️ Partial — see Verification Summary (2026-09-21, `5291c59`) |
| **REQ-019** | SYS-009 | Output Writer | STP-009-A | Interface Contract Testing | STS-009-A1 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-020** | SYS-009 | Output Writer | STP-009-A | Interface Contract Testing | STS-009-A1 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-021** | SYS-010 | Run Summary Generator | STP-010-A | Interface Contract Testing | STS-010-A1, STS-010-A2 | ✅ Passed (2026-09-21, `5291c59`) |
| | SYS-010 | Run Summary Generator | STP-010-B | Fault Injection | STS-010-B1, STS-010-B2 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-022** | SYS-010 | Run Summary Generator | STP-010-A | Interface Contract Testing | STS-010-A1, STS-010-A2 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-023** | SYS-010 | Run Summary Generator | STP-010-B | Fault Injection | STS-010-B1, STS-010-B2 | ✅ Passed (2026-09-21, `5291c59`) |
| | SYS-011 | Batch Conversion Orchestrator | STP-011-B | Boundary Value Analysis | STS-011-B1, STS-011-B2 | ⚠️ Partial — see Verification Summary (2026-09-21, `5291c59`) |
| **REQ-024** | SYS-004 | Timestamp Interpretation Component | STP-004-A | Interface Contract Testing | STS-004-A1 | ✅ Passed (2026-09-21, `5291c59`) |
| | SYS-004 | Timestamp Interpretation Component | STP-004-B | Boundary Value Analysis | STS-004-B1 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-025** | SYS-005 | Timezone Conversion Engine | STP-005-A | Interface Contract Testing | STS-005-A1, STS-005-A2 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-026** | SYS-005 | Timezone Conversion Engine | STP-005-A | Interface Contract Testing | STS-005-A1, STS-005-A2 | ✅ Passed (2026-09-21, `5291c59`) |
| | SYS-005 | Timezone Conversion Engine | STP-005-B | Boundary Value Analysis | STS-005-B1, STS-005-B2 | ⚠️ No direct test match found |
| **REQ-027** | SYS-005 | Timezone Conversion Engine | STP-005-A | Interface Contract Testing | STS-005-A1, STS-005-A2 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-028** | SYS-003 | Row Validator | STP-003-B | Equivalence Partitioning | STS-003-B1, STS-003-B2 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-029** | SYS-009 | Output Writer | STP-009-B | Fault Injection | STS-009-B1 | ✅ Passed (2026-09-21, `5291c59`) |
| | SYS-011 | Batch Conversion Orchestrator | STP-011-A | Fault Injection | STS-011-A1, STS-011-A2 | ⚠️ Partial — see Verification Summary (2026-09-21, `5291c59`) |
| **REQ-NF-001** | SYS-006 | Row Processing Controller | STP-006-A | Fault Injection | STS-006-A1 | ✅ Passed (2026-09-21, `5291c59`) |
| | SYS-006 | Row Processing Controller | STP-006-B | Fault Injection | STS-006-B1 | ⚠️ No direct test match found |
| | SYS-008 | Successful Result Formatter | STP-008-A | Interface Contract Testing | STS-008-A1 | ✅ Passed (2026-09-21, `5291c59`) |
| | SYS-008 | Successful Result Formatter | STP-008-B | Boundary Value Analysis | STS-008-B1 | ⚠️ No direct test match found |
| **REQ-NF-002** | SYS-007 | Invalid Row Reporter | STP-007-A | Interface Contract Testing | STS-007-A1 | ✅ Passed (2026-09-21, `5291c59`) |
| | SYS-007 | Invalid Row Reporter | STP-007-B | Boundary Value Analysis | STS-007-B1 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-NF-003** | SYS-010 | Run Summary Generator | STP-010-A | Interface Contract Testing | STS-010-A1, STS-010-A2 | ✅ Passed (2026-09-21, `5291c59`) |
| **REQ-NF-004** | SYS-005 | Timezone Conversion Engine | STP-005-B | Boundary Value Analysis | STS-005-B1, STS-005-B2 | ⚠️ No direct test match found |
| **REQ-NF-005** | SYS-011 | Batch Conversion Orchestrator | STP-011-B | Boundary Value Analysis | STS-011-B1, STS-011-B2 | ⚠️ Partial — see Verification Summary (2026-09-21, `5291c59`) |

**Matrix B totals**: 34 REQs / 11 SYS components / 23 STPs / 33 STSs — 63 REQ↔SYS↔STP join rows.
- REQ → SYS coverage: 34/34 (100%) — every requirement is a parent of at least one SYS component (per `system-design.md` Decomposition View, including the cross-reference addendum for REQ-003 and REQ-NF-005).
- SYS → STP coverage: 11/11 (100%) — every SYS component has at least one STP test case.
- STP → STS coverage: 23/23 (100%) — every STP has at least one executable STS scenario.
- Backward check (STS → STP → SYS → REQ): all 33 STS trace to an STP with a non-empty `Verifies:` list, and every REQ named in a `Verifies:` list is a real REQ ID present in `requirements.md`. 0 orphaned STPs, 0 orphaned STS.
- Gaps: none. Orphans: none.

---

## Baseline Information

| Property | Value |
|----------|-------|
| Matrix Generated | 2026-09-21 (refreshed 2026-09-21 15:16 UTC — system testing verification pass, scope=system only) |
| Requirements Source | `specs/003-batch-timezone-conversion/v-model/requirements.md` |
| Requirements Last Modified | 2026-09-21 12:24:53 UTC (commit `06922fa`) |
| Acceptance Plan Source | `specs/003-batch-timezone-conversion/v-model/acceptance-plan.md` |
| Acceptance Plan Last Modified | 2026-09-21 12:31:01 UTC (commit `8266841`) |
| System Design Source | `specs/003-batch-timezone-conversion/v-model/system-design.md` |
| System Design Last Modified | 2026-09-21 12:34:47 UTC (commit `9c5d6f2`) |
| System Test Source | `specs/003-batch-timezone-conversion/v-model/system-test.md` |
| System Test Last Modified | 2026-09-21 12:43:22 UTC (commit `49a3058`) |
| Validation Tool | Manual deterministic ID cross-check (`build-matrix.sh`/`.ps1` not present in this repository checkout; all REQ/ATP/SCN/SYS/STP/STS IDs were re-enumerated and cross-referenced exhaustively by direct parsing of source files at candidate commit `5291c59`, not inferred or hallucinated) |
| Git Commit (if available) | `5291c59` (candidate commit; working tree clean — all source artifacts and this matrix are committed as of this commit) |

---

## Verification Summary (System Scope — Matrix B Only)

**Executed**: 2026-09-21 | **Commit**: `5291c59` | **Scope**: SYSTEM only (Matrix B / STS rows). Matrix A (ATP/SCN) rows were intentionally left untouched at "⬜ Pending Execution" — they are handled by a separate acceptance-scope task.

**Evidence ingested**: `dotnet test` run at commit `5291c59`, converted 1:1 from three `.trx` files (Unit.trx: 45 cases, Contract.trx: 7 cases, Integration.trx: 27 cases = 79 total, all passed) into a single JUnit XML file. No `scripts/bash/ingest-test-results.sh` or `scripts/powershell/Ingest-Test-Results.ps1` exists in this repository checkout, so the mapping and matrix edits below were performed manually/deterministically by cross-referencing STS IDs embedded in test method names (and, where absent from the name, by inspecting class-level doc comments and the full method list of each test class) against `system-test.md`.

**Totals**: 33 STS scenarios total in `system-test.md`. 24 were confidently matched to a specific passing JUnit test case (by exact `STS_NNN_X#` token in the test method name) and are marked `✅ Passed (2026-09-21, `5291c59`)` in Matrix B above. 9 have **no direct test match found**.

Because several Matrix B rows list more than one STS ID under a single Status cell (the matrix format has one Status per REQ/SYS/STP row, not per STS), a row is marked `✅ Passed` only when **every** STS ID listed in that row has a confident match:
- A row is marked `✅ Passed (2026-09-21, `5291c59`)` when all STS IDs in its cell matched a passing test.
- A row is marked `⚠️ No direct test match found` when none of the STS IDs in its cell matched a test.
- A row is marked `⚠️ Partial — see Verification Summary (2026-09-21, `5291c59`)` when the cell mixes a matched STS ID with an unmatched one (e.g. `STS-001-A1, STS-001-A2` where only A1 matched) — see the gap list below for exactly which STS ID within each such row is missing.

### Confidently matched and passed (24 of 33)

STS-002-A1, STS-002-A2, STS-002-B1, STS-003-A2, STS-003-B1, STS-003-B2, STS-004-A1, STS-004-B1, STS-005-A1, STS-005-A2, STS-005-C1, STS-006-A1, STS-007-A1, STS-007-B1, STS-008-A1, STS-009-A1, STS-009-B1, STS-010-A1, STS-010-A2, STS-010-B1, STS-010-B2, STS-001-A1, STS-011-A2, STS-011-B1

Each matched via an exact `STS_NNN_X#_...` token in a JUnit test case name from `Unit.trx`/`Contract.trx`/`Integration.trx` (e.g. `STS_009_B1_WriteDestinationUnwritable_ReportsFailure_AndInMemoryResultsRemainRetained` in `BatchOutputWriterTests.cs`), all reporting `Pass`.

### Gaps — no direct test match found (9 of 33)

| STS ID | Row(s) affected in Matrix B | Note |
|---|---|---|
| STS-001-A2 | REQ-001, REQ-016, REQ-018 rows (SYS-001 / STP-001-A) | `CsvBatchReaderTests.cs` class doc comment claims coverage, but no test method name or body was found asserting the "file exists but unreadable (permission denied)" scenario specifically |
| STS-001-B1 | REQ-016, REQ-018 rows (SYS-001 / STP-001-B) | Same class doc comment claims coverage; no fault-injection test method for SYS-011→SYS-001 found |
| STS-003-A1 | REQ-003, REQ-008 through REQ-015 rows (SYS-003 / STP-003-A) | `BatchRowValidatorTests.cs` covers A2/B1/B2 by name; no test for "unparseable timestamp, recognized source/target timezone" (A1) found |
| STS-005-B1 | REQ-026, REQ-NF-004 rows (SYS-005 / STP-005-B) | No test for the spring-forward DST boundary at the SYS-005 unit level (the ATP-026-A integration test covers the same concept at acceptance scope, but that is a Matrix A artifact, not a Matrix B/STS match) |
| STS-005-B2 | REQ-026, REQ-NF-004 rows (SYS-005 / STP-005-B) | No test for the fall-back DST boundary at the SYS-005 unit level (ATP-026-B is the acceptance-scope analogue, not counted here) |
| STS-006-B1 | REQ-006, REQ-011, REQ-NF-001 rows (SYS-006 / STP-006-B) | `BatchRowProcessorTests.cs` class doc comment claims coverage; no test method for the SYS-011→SYS-006 unhandled-exception fault-injection scenario found |
| STS-008-B1 | REQ-007, REQ-NF-001 rows (SYS-008 / STP-008-B) | `SuccessfulResultFormatterTests.cs` class doc comment claims coverage; no test for the "300/300 rows, 0 missing" completeness boundary found |
| STS-011-A1 | REQ-018, REQ-029 rows (SYS-011 / STP-011-A) | No test found for the SYS-002 column-resolution-error → SYS-011 abort sequencing scenario |
| STS-011-B2 | REQ-023, REQ-NF-005 rows (SYS-011 / STP-011-B) | No test found asserting the 10,000-row single-pass boundary at the SYS-011 orchestrator level (`ATP_NF_005_A_SCN_NF_005_A1_10000RowFile_CompletesInSingleRun` in `ConvertBatchCommandTests.cs` covers the same concept at acceptance scope, not counted here) |

These 9 gaps were deliberately marked `⚠️ No direct test match found` (or, where mixed into a row with a matched STS ID, `⚠️ Partial — see Verification Summary`) rather than `✅ Passed`, since no JUnit test case could be confidently attributed to them — several test classes carry doc comments claiming broader STS coverage than their actual test method set delivers.

### Evidence files

Copied to `specs/003-batch-timezone-conversion/v-model/evidence/system-2026-09-21/`:
- `junit-results.xml` (79 test cases, all passed)
- `Unit.trx`, `Contract.trx`, `Integration.trx`, `full.trx`

**Commit used for all Status entries above**: `5291c59`. **Date used**: 2026-09-21.
