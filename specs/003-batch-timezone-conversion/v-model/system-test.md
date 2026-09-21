# System Test Plan: Batch Timezone Conversion

**Feature Branch**: `003-batch-timezone-conversion`
**Source Documents**:
- `specs/003-batch-timezone-conversion/v-model/system-design.md` (11 system components, SYS-001–SYS-011, IEEE 1016-2009)
- `specs/003-batch-timezone-conversion/v-model/requirements.md` (34 requirements, REQ-001–REQ-029, REQ-NF-001–REQ-NF-005)
- `specs/003-batch-timezone-conversion/v-model/acceptance-plan.md` (35 ATPs / 37 SCNs — user-journey level; referenced for context only, not duplicated here)
- `specs/003-batch-timezone-conversion/spec.md`
**Generated**: 2026-09-21
**Status**: Draft
**Standard**: ISO/IEC/IEEE 29119-3 (Test Documentation) — System Test Plan and Test Cases, tracing to IEEE 1016-2009 design views

## Overview

This document defines the System Test Plan (STP) and System Test Scenarios (STS) for the 11 system components (`SYS-001`–`SYS-011`) decomposed in `system-design.md`. Unlike the acceptance plan (`acceptance-plan.md`), which exercises end-to-end user journeys (ATP/SCN), this plan tests components and their contracts directly: interfaces (external and internal), data handling boundaries, inter-component dependency/failure propagation, and equivalence classes of input data. Every test targets a named `SYS-NNN` component (or a named source→target dependency edge between components), states the IEEE 1016 design view it is drawn from, names its ISO 29119 test technique, and lists the `REQ-NNN`/`REQ-NF-NNN` id(s) it helps verify. This feeds Matrix B (REQ → SYS → STP → STS) of the project's traceability matrix.

No `v-model-config.yml` was found at the repository root, so no safety-critical domain (ISO 26262 / DO-178C / IEC 62304) is configured for this feature; MC/DC coverage and WCET sections are therefore omitted from this plan.

## ID Schema

- **STP-NNN-X**: System Test Plan (test case) entry. `NNN` matches the `SYS-NNN` component under test; `X` is a letter (`A`, `B`, ...) distinguishing multiple test cases against the same component (e.g., because the component spans multiple IEEE 1016 views, or because more than one technique applies).
- **STS-NNN-X#**: System Test Scenario, an executable Given/When/Then scenario belonging to `STP-NNN-X`. `#` is a numeral (`1`, `2`, ...) distinguishing multiple scenarios under the same test case.
- Every `STP` cites: **Design View** (Interface View / Data Design View / Dependency View, or a named combination), **Technique** (one of the four ISO 29119 techniques below), **Verifies** (one or more `REQ-NNN`/`REQ-NF-NNN` ids), and **Interface Class** (External or Internal, for Interface View-derived STPs only).
- External-interface and internal-interface tests for the same component are always issued as separate `STP` entries, never merged.

## ISO 29119 Test Technique Reference

| Technique | Applied when the SYS component is drawn from... | Typical checks |
|-----------|----------------------------------------------------|-----------------|
| **Interface Contract Testing** | Interface View (External or Internal Interfaces tables) | Request/response shape, protocol conformance, required fields present, error codes on malformed/absent input, contract adherence between caller and callee |
| **Boundary Value Analysis** | Data Design View, or numeric/temporal thresholds called out in the Decomposition View | Edge values of a data domain: zero rows, exactly 10,000 rows, DST transition instants, date-boundary crossings, min/max retention states |
| **Equivalence Partitioning** | Decomposition View module logic (validation/classification behavior) not already covered by a boundary or contract test | Representative valid/invalid/blank input classes are each tested once as a class, rather than exhaustively |
| **Fault Injection** | Dependency View (Source → Target edges and their documented Failure Impact) | Deliberately fail/delay/except a dependency edge and assert the documented failure-isolation or propagation behavior occurs |

---

## SYS-001: CSV File Intake

**Design views**: Interface View (External Interface: Batch CSV Input Interface), Internal Interface (SYS-011→SYS-001 File Open Request), Data Design View (Input CSV File entity).

### STP-001-A — External Interface Contract Testing (Batch CSV Input Interface)

- **Design View**: Interface View — External Interfaces
- **Technique**: Interface Contract Testing
- **Interface Class**: External
- **Verifies**: REQ-001, REQ-016, REQ-018

**STS-001-A1**
- Given a file path pointing to a CSV file that does not exist on the filesystem
- When SYS-001 attempts to open the file via the Batch CSV Input Interface
- Then SYS-001 returns a distinct file-level error object (not a per-row error) with an error code identifying "file not found", and no row stream is produced

**STS-001-A2**
- Given a file path pointing to a file that exists but is unreadable (permission bits deny read access to the executing process)
- When SYS-001 attempts to open the file via the Batch CSV Input Interface
- Then SYS-001 returns a file-level error object with an error code identifying "file unreadable", and no row stream is produced

### STP-001-B — Internal Interface Fault Injection (SYS-011 → SYS-001)

- **Design View**: Dependency View (SYS-011 Batch Conversion Orchestrator → SYS-001 CSV File Intake)
- **Technique**: Fault Injection
- **Interface Class**: Internal
- **Verifies**: REQ-016, REQ-018

**STS-001-B1**
- Given SYS-001 has been made to raise a file-level error when invoked by SYS-011's File Open Request
- When SYS-011 invokes SYS-001
- Then SYS-011 aborts the run immediately, routes the file-level error to SYS-009, and SYS-002 and SYS-006 are never invoked (0 calls recorded to SYS-002 and SYS-006)

---

## SYS-002: Header/Column Resolver

**Design views**: Interface View (Internal: Header Row Read, Column Resolution Request), Decomposition View (column identification by name, order-independent).

### STP-002-A — Internal Interface Contract Testing (Column Resolution Request)

- **Design View**: Interface View — Internal Interfaces (SYS-011 → SYS-002 Column Resolution Request; SYS-002 → SYS-001 Header Row Read)
- **Technique**: Interface Contract Testing
- **Interface Class**: Internal
- **Verifies**: REQ-002, REQ-004, REQ-017, REQ-018

**STS-002-A1**
- Given a header row containing the timestamp, source-timezone, and target-timezone columns in a non-default order, plus one unrecognized extra column
- When SYS-011 issues a Column Resolution Request to SYS-002
- Then SYS-002 returns a column index/name map correctly identifying all three required columns and the unrecognized column is absent from the returned map

**STS-002-A2**
- Given a header row that does not contain an identifiable timestamp column
- When SYS-011 issues a Column Resolution Request to SYS-002
- Then SYS-002 returns a resolver error distinct from a per-row error, and SYS-011 aborts the run without invoking SYS-006 for any row

### STP-002-B — Equivalence Partitioning (Header Content Classes)

- **Design View**: Decomposition View
- **Technique**: Equivalence Partitioning
- **Verifies**: REQ-002, REQ-004

**STS-002-B1**
- Given three equivalence classes of header rows: (1) all required columns present in canonical order, (2) all required columns present in a shuffled order with extra unrecognized columns, (3) at least one required column absent
- When each class representative is submitted to SYS-002
- Then classes (1) and (2) each resolve successfully with the extra column in class (2) excluded from downstream processing, and class (3) returns a resolver error

---

## SYS-003: Row Validator

**Design views**: Interface View (Internal: Row Validation Request), Decomposition View (timestamp/timezone/blank-row validation logic).

### STP-003-A — Internal Interface Contract Testing (Row Validation Request)

- **Design View**: Interface View — Internal Interfaces (SYS-006 → SYS-003 Row Validation Request)
- **Technique**: Interface Contract Testing
- **Interface Class**: Internal
- **Verifies**: REQ-008, REQ-009, REQ-010, REQ-013, REQ-014, REQ-015

**STS-003-A1**
- Given a Row Validation Request for a Conversion Row with an unparseable timestamp string, a recognized source timezone, and a recognized target timezone
- When SYS-006 invokes SYS-003
- Then SYS-003 returns fail=true with a field-specific reason code referencing the timestamp field

**STS-003-A2**
- Given a Row Validation Request for a Conversion Row with a valid timestamp and an unrecognized source-timezone identifier string "Not/AZone"
- When SYS-006 invokes SYS-003
- Then SYS-003 returns fail=true with a reason code referencing the source-timezone field and including the literal unrecognized value "Not/AZone"

### STP-003-B — Equivalence Partitioning (Row Content Classes)

- **Design View**: Decomposition View
- **Technique**: Equivalence Partitioning
- **Verifies**: REQ-008, REQ-009, REQ-010, REQ-015, REQ-028, REQ-003

**STS-003-B1**
- Given five equivalence classes of rows: (1) fully valid, (2) unparseable timestamp, (3) unrecognized source or target timezone, (4) one or more required fields entirely missing, (5) entirely blank row (no data in any column)
- When each class representative is submitted to SYS-003
- Then class (1) returns pass=true; classes (2)-(4) each return fail=true with the reason code matching their respective field; class (5) is classified as skipped (neither pass nor fail) and is excluded from both the succeeded and failed counts

**STS-003-B2**
- Given a row whose source timezone is the IANA identifier "America/New_York" and whose target timezone is the IANA identifier "Asia/Tokyo"
- When the row is submitted to SYS-003
- Then SYS-003 recognizes both identifiers as valid IANA timezone identifiers and returns pass=true for the timezone fields

---

## SYS-004: Timestamp Interpretation Component

**Design views**: Interface View (Internal: Timestamp Interpretation Request), Decomposition View (offset-authoritative interpretation logic).

### STP-004-A — Internal Interface Contract Testing (Timestamp Interpretation Request)

- **Design View**: Interface View — Internal Interfaces (SYS-006 → SYS-004 Timestamp Interpretation Request)
- **Technique**: Interface Contract Testing
- **Interface Class**: Internal
- **Verifies**: REQ-024

**STS-004-A1**
- Given a raw timestamp string "2026-03-10 14:00" with no embedded UTC offset and a source timezone of "America/Chicago"
- When SYS-006 issues a Timestamp Interpretation Request to SYS-004
- Then SYS-004 returns an interpreted timestamp treated as local to "America/Chicago" (UTC-06:00 for that date), with no interpretation error raised

### STP-004-B — Boundary Value Analysis (Offset Presence Boundary)

- **Design View**: Data Design View (Conversion Row in-flight entity — timestamp field variants)
- **Technique**: Boundary Value Analysis
- **Verifies**: REQ-024

**STS-004-B1**
- Given two boundary input variants: (1) a timestamp string with a fully embedded UTC offset "2026-03-10T14:00:00-05:00", and (2) the same clock value with no offset "2026-03-10 14:00:00"
- When each variant is submitted to SYS-004 with source timezone "America/New_York"
- Then variant (1) is interpreted using its embedded offset and variant (2) is interpreted as local to the stated source timezone, producing two distinct interpreted-timestamp results

---

## SYS-005: Timezone Conversion Engine

**Design views**: Interface View (Internal: Conversion Request; external delegation to 001-timezone-utility), Data Design View (Conversion Row, DST accuracy), Dependency View (SYS-005 → 001-timezone-utility).

### STP-005-A — Internal Interface Contract Testing (Conversion Request)

- **Design View**: Interface View — Internal Interfaces (SYS-006 → SYS-005 Conversion Request)
- **Technique**: Interface Contract Testing
- **Interface Class**: Internal
- **Verifies**: REQ-005, REQ-025, REQ-026, REQ-027, REQ-003

**STS-005-A1**
- Given a Conversion Request with interpreted timestamp "2026-01-15T23:30:00", source timezone "America/Los_Angeles", target timezone "Asia/Tokyo"
- When SYS-006 invokes SYS-005
- Then SYS-005 returns a converted date/time/target-timezone-identifier result whose calendar date reflects the date-boundary crossing (i.e., the resulting date is 2026-01-16 in Asia/Tokyo)

**STS-005-A2**
- Given a Conversion Request whose source and target timezone values are both "Europe/Berlin"
- When SYS-006 invokes SYS-005
- Then SYS-005 returns a converted result with the timestamp value unchanged, and does not raise a same-timezone error

### STP-005-B — Boundary Value Analysis (DST Transition Instants)

- **Design View**: Data Design View (Conversion Row — REQ-NF-004 accuracy window; DST rule application)
- **Technique**: Boundary Value Analysis
- **Verifies**: REQ-026, REQ-NF-004

**STS-005-B1**
- Given an interpreted timestamp of "2026-03-08T02:30:00" (falling inside the US "spring forward" nonexistent-hour gap) with source timezone "America/New_York"
- When the timestamp is submitted to SYS-005 for conversion to "UTC"
- Then SYS-005 resolves the ambiguous instant using the standard DST resolution rule and returns a converted result rather than raising a validation failure

**STS-005-B2**
- Given an interpreted timestamp of "2026-11-01T01:30:00" (falling inside the US "fall back" repeated hour) with source timezone "America/New_York"
- When the timestamp is submitted to SYS-005 for conversion to "UTC"
- Then SYS-005 resolves the ambiguous hour using the standard DST resolution rule and returns exactly one converted result, accurate to the minute

### STP-005-C — Fault Injection (SYS-005 → 001-timezone-utility)

- **Design View**: Dependency View (SYS-005 Timezone Conversion Engine → 001-timezone-utility conversion/DST logic, external dependency)
- **Technique**: Fault Injection
- **Verifies**: REQ-011, REQ-005

**STS-005-C1**
- Given the underlying 001-timezone-utility conversion/DST logic is made to throw an exception on invocation
- When SYS-005 delegates a Conversion Request to it
- Then the exception propagates out of SYS-005 to SYS-006, which catches it and converts it into a row-level failure without aborting the batch run

---

## SYS-006: Row Processing Controller

**Design views**: Dependency View (SYS-011→SYS-006; SYS-006→SYS-003/004/005/007/008), Decomposition View (isolation guarantee).

### STP-006-A — Fault Injection (SYS-006 → SYS-005 dependency edge)

- **Design View**: Dependency View (SYS-006 Row Processing Controller → SYS-005 Timezone Conversion Engine: "If SYS-005 fails or throws for a row ... SYS-006 catches it, routes the row to SYS-007, and continues; other rows unaffected")
- **Technique**: Fault Injection
- **Verifies**: REQ-006, REQ-011, REQ-NF-001

**STS-006-A1**
- Given a batch of 5 rows where row 3 is engineered to make SYS-005 throw an unexpected DST-edge-case exception
- When SYS-006 processes all 5 rows sequentially
- Then row 3 is routed to SYS-007 as an invalid-row entry, rows 1, 2, 4, and 5 each produce a successful result via SYS-008, and no row's outcome is altered by row 3's failure

### STP-006-B — Fault Injection (SYS-011 → SYS-006 dependency edge)

- **Design View**: Dependency View (SYS-011 Batch Conversion Orchestrator → SYS-006 Row Processing Controller: "If SYS-006 raises an unexpected (non-isolated) error for a row, SYS-011 must still continue the run for subsequent rows")
- **Technique**: Fault Injection
- **Verifies**: REQ-006, REQ-011, REQ-NF-001

**STS-006-B1**
- Given SYS-006 is engineered to raise an unhandled (non-isolated) exception while processing row 2 of a 4-row batch
- When SYS-011 invokes SYS-006 for each row in sequence
- Then SYS-011 continues invoking SYS-006 for rows 3 and 4 after the row-2 exception, and rows 1, 3, and 4 each reach a terminal (success or reported-invalid) state

---

## SYS-007: Invalid Row Reporter

**Design views**: Interface View (Internal: Invalid Row Submission), Data Design View (Invalid Row Report Entry Set).

### STP-007-A — Internal Interface Contract Testing (Invalid Row Submission)

- **Design View**: Interface View — Internal Interfaces (SYS-006 → SYS-007 Invalid Row Submission)
- **Technique**: Interface Contract Testing
- **Interface Class**: Internal
- **Verifies**: REQ-012, REQ-013, REQ-014, REQ-015, REQ-NF-002

**STS-007-A1**
- Given SYS-006 submits an Invalid Row Submission with row number 7, raw values, and failure reason "unparseable timestamp"
- When SYS-007 receives the submission
- Then SYS-007 returns a confirmation that a distinct report entry was recorded, and the entry is retrievable containing row number 7 and the reason "unparseable timestamp"

### STP-007-B — Boundary Value Analysis (Zero-Omission Guarantee)

- **Design View**: Data Design View (Invalid Row Report Entry Set — REQ-NF-002 zero-omission boundary)
- **Technique**: Boundary Value Analysis
- **Verifies**: REQ-NF-002, REQ-012

**STS-007-B1**
- Given a batch of 200 rows in which exactly 50 rows are engineered to fail validation for distinct reasons
- When all 200 rows are processed
- Then the invalid-row report contains exactly 50 entries, each with a unique row number matching one of the 50 failing rows, and 0 entries are merged or omitted

---

## SYS-008: Successful Result Formatter

**Design views**: Interface View (Internal: Successful Result Submission), Data Design View (Conversion Result Set).

### STP-008-A — Internal Interface Contract Testing (Successful Result Submission)

- **Design View**: Interface View — Internal Interfaces (SYS-006 → SYS-008 Successful Result Submission)
- **Technique**: Interface Contract Testing
- **Interface Class**: Internal
- **Verifies**: REQ-007, REQ-NF-001

**STS-008-A1**
- Given SYS-006 submits a Successful Result Submission with row number 4, original input values, and a converted date/time/target-timezone result
- When SYS-008 receives the submission
- Then SYS-008 returns a confirmation that a distinct output record was recorded, and the record contains both the original input values and the converted result fields

### STP-008-B — Boundary Value Analysis (100% Valid-Row Return Guarantee)

- **Design View**: Data Design View (Conversion Result Set — REQ-NF-001 completeness boundary)
- **Technique**: Boundary Value Analysis
- **Verifies**: REQ-NF-001, REQ-007

**STS-008-B1**
- Given a batch of 300 rows in which exactly 300 rows pass validation and conversion (0 invalid rows)
- When the batch completes processing
- Then SYS-008 holds exactly 300 formatted output records, one for each successfully converted row, with 0 records missing

---

## SYS-009: Output Writer

**Design views**: Interface View (External: Successful Output Interface, Invalid Row Report Interface), Data Design View (write-failure retention).

### STP-009-A — External Interface Contract Testing (Successful/Invalid Output Interfaces)

- **Design View**: Interface View — External Interfaces
- **Technique**: Interface Contract Testing
- **Interface Class**: External
- **Verifies**: REQ-019, REQ-020

**STS-009-A1**
- Given a completed run with 3 successful results and 2 invalid-row entries
- When SYS-009 emits output via the Successful Output Interface and the Invalid Row Report Interface
- Then two separate structured artifacts are produced, the successful-output artifact contains exactly the 3 successful records, and the invalid-row-report artifact contains exactly the 2 invalid entries with no intermixing between the two artifacts

### STP-009-B — Fault Injection (Write-Destination Failure)

- **Design View**: Data Design View (Conversion Result Set / Invalid Row Report Entry Set — "retained in memory on write failure")
- **Technique**: Fault Injection
- **Verifies**: REQ-029

**STS-009-B1**
- Given the output destination is engineered to reject writes (e.g., simulated insufficient disk space) and a completed run holds 10 already-computed successful results
- When SYS-009 attempts to write the successful-output artifact
- Then SYS-009 reports the write failure to the caller with a distinct error state, and the 10 already-computed results remain retained in memory (not discarded) and are inspectable after the failed write attempt

---

## SYS-010: Run Summary Generator

**Design views**: Interface View (External: Run Summary Interface; Internal: Summary Computation Request), Data Design View (Batch Run Summary), Dependency View (SYS-010→SYS-007, SYS-010→SYS-008).

### STP-010-A — External Interface Contract Testing (Run Summary Interface)

- **Design View**: Interface View — External Interfaces
- **Technique**: Interface Contract Testing
- **Interface Class**: External
- **Verifies**: REQ-021, REQ-022, REQ-NF-003

**STS-010-A1**
- Given a completed run with total=10, succeeded=10, failed=0
- When SYS-010 presents the Run Summary Interface output
- Then the presented output explicitly displays the failed count as the numeral "0" (not omitted), and a viewer can classify the run as fully succeeded using only the succeeded and failed values

**STS-010-A2**
- Given a completed run with total=10, succeeded=6, failed=4
- When SYS-010 presents the Run Summary Interface output
- Then the presented output displays both counts such that the run is classifiable as "partially succeeded" (both counts > 0) without cross-referencing the row-level output or invalid-row report

### STP-010-B — Fault Injection (SYS-010 → SYS-007 / SYS-008 dependency edges)

- **Design View**: Dependency View (SYS-010 Run Summary Generator → SYS-007 Invalid Row Reporter: "reads (failed-row count)"; SYS-010 → SYS-008 Successful Result Formatter: "reads (succeeded-row count)")
- **Technique**: Fault Injection
- **Verifies**: REQ-021, REQ-023

**STS-010-B1**
- Given SYS-007's recorded entry count is engineered to be incomplete (2 entries missing from an expected 5)
- When SYS-010 computes the run summary by reading SYS-007's failed-row count
- Then the displayed failed count reflects SYS-007's (incomplete) count of 3, demonstrating the documented dependency-failure-impact that SYS-010's accuracy is bound to SYS-007's completeness

**STS-010-B2**
- Given a header-only input file with zero data rows has completed the SYS-011 pipeline
- When SYS-010 computes the run summary
- Then SYS-010 produces the specific "no rows processed" message rather than a summary showing total=0 succeeded=0 failed=0 without explanation

---

## SYS-011: Batch Conversion Orchestrator

**Design views**: Dependency View (top-level sequencing of SYS-001, SYS-002, SYS-006, SYS-009, SYS-010), Data Design View (empty-file completion path), Decomposition View (REQ-NF-005 streaming design).

### STP-011-A — Fault Injection (Orchestration Sequencing Failures)

- **Design View**: Dependency View (SYS-011 → SYS-001, SYS-011 → SYS-002, SYS-011 → SYS-009, SYS-011 → SYS-010)
- **Technique**: Fault Injection
- **Verifies**: REQ-018, REQ-029

**STS-011-A1**
- Given SYS-002 is engineered to return a column-resolution error (required columns not identifiable)
- When SYS-011 sequences file intake and column resolution for a run
- Then SYS-011 aborts the run, routes a file-level error to SYS-009, and 0 rows are ever submitted to SYS-006

**STS-011-A2**
- Given SYS-009 is engineered to fail on the output-write step after SYS-006 has completed processing all rows
- When SYS-011 sequences the output-writing step
- Then SYS-011 surfaces the write failure while the already-computed in-memory result set remains available (per REQ-029), and SYS-010 is still invoked to produce a run summary

### STP-011-B — Boundary Value Analysis (Empty-File and Large-Batch Boundaries)

- **Design View**: Data Design View (empty-file completion path); Decomposition View (REQ-NF-005 streaming design)
- **Technique**: Boundary Value Analysis
- **Verifies**: REQ-023, REQ-NF-005

**STS-011-B1**
- Given an input CSV file containing only a header row and zero data rows
- When SYS-011 executes the full pipeline against this file
- Then the run completes without raising an error, producing an empty successful-output set, an empty invalid-row report, and SYS-010's "no rows processed" message

**STS-011-B2**
- Given an input CSV file containing exactly 10,000 data rows, all valid
- When SYS-011 executes the full pipeline against this file in a single invocation
- Then the run completes in one pass without requiring the file to be split, producing 10,000 successful output records and a run summary reporting total=10000, succeeded=10000, failed=0

---

## Coverage Summary

| Metric | Count |
|--------|-------|
| System components in scope (SYS-001–SYS-011) | 11 |
| Components with at least one STP | 11 |
| Total STP test cases | 23 |
| Total STS executable scenarios | 33 |
| Component coverage | 11/11 — **100%** |

### STP / STS count per component

| SYS ID | STP count | STS count |
|--------|-----------|-----------|
| SYS-001 | 2 (STP-001-A, STP-001-B) | 3 |
| SYS-002 | 2 (STP-002-A, STP-002-B) | 3 |
| SYS-003 | 2 (STP-003-A, STP-003-B) | 4 |
| SYS-004 | 2 (STP-004-A, STP-004-B) | 2 |
| SYS-005 | 3 (STP-005-A, STP-005-B, STP-005-C) | 5 |
| SYS-006 | 2 (STP-006-A, STP-006-B) | 2 |
| SYS-007 | 2 (STP-007-A, STP-007-B) | 2 |
| SYS-008 | 2 (STP-008-A, STP-008-B) | 2 |
| SYS-009 | 2 (STP-009-A, STP-009-B) | 2 |
| SYS-010 | 2 (STP-010-A, STP-010-B) | 4 |
| SYS-011 | 2 (STP-011-A, STP-011-B) | 4 |
| **Total** | **23** | **33** |

### Technique distribution (authoritative)

| Technique | STPs | Count |
|-----------|------|-------|
| Interface Contract Testing | STP-001-A, STP-002-A, STP-003-A, STP-004-A, STP-005-A, STP-007-A, STP-008-A, STP-009-A, STP-010-A | **9** |
| Boundary Value Analysis | STP-004-B, STP-005-B, STP-007-B, STP-008-B, STP-011-B | **5** |
| Equivalence Partitioning | STP-002-B, STP-003-B | **2** |
| Fault Injection | STP-001-B, STP-005-C, STP-006-A, STP-006-B, STP-009-B, STP-010-B, STP-011-A | **7** |
| **Total** | | **23** |

## Final Totals

- **Total STP test cases**: 23
- **Total STS executable scenarios**: 33
- **Components covered**: 11 / 11 SYS components (SYS-001 through SYS-011) — **100%**
- **Technique distribution**: Interface Contract Testing = 9, Boundary Value Analysis = 5, Equivalence Partitioning = 2, Fault Injection = 7

## Uncovered Components

None — 100% coverage. Every SYS-001 through SYS-011 component has at least one STP test case (with most having 2–3, spanning multiple IEEE 1016 design views and ISO 29119 techniques where applicable), and every STP has at least one executable STS scenario.

## Safety-Critical Sections

Not applicable. No `v-model-config.yml` was found at the repository root, so this feature is not configured for the `iso_26262`, `do_178c`, or `iec_62304` domains. MC/DC coverage analysis and WCET (worst-case execution time) sections are omitted from this plan.
