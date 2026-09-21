---
description: "Task list for Batch Timezone Conversion implementation"
---

# Tasks: Batch Timezone Conversion

**Input**: Design documents from `specs/003-batch-timezone-conversion/` (plan.md, data-model.md, contracts/, research.md, quickstart.md) and the APPROVED v-model artifacts under `specs/003-batch-timezone-conversion/v-model/` (requirements.md REQ-001..REQ-029/REQ-NF-001..REQ-NF-005, system-design.md SYS-001..SYS-011, acceptance-plan.md ATP/SCN, system-test.md STP/STS, traceability-matrix.md)

**Prerequisites**: plan.md (read), spec.md (read, not modified), data-model.md (read), contracts/*.md (read), v-model/*.md (read, not modified)

**Tests**: Included — v-model/system-design.md's Post-Design Gate explicitly requires unit/contract/integration test coverage, and acceptance-plan.md / system-test.md are approved test-plan constraints that must be made executable.

**Organization**: Tasks are grouped by user story (US1–US3, from spec.md, matching requirements.md's Rationale traceability) to enable independent implementation and testing. Every relevant task cites the REQ-NNN / SYS-NNN / ATP-NNN / STP-NNN IDs it realizes or verifies. Test tasks are ordered before the implementation tasks they exercise (test-first).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: US1, US2, or US3 (maps to spec.md priorities P1/P1/P2)
- Exact file paths are given in every description

## Path Conventions

Single .NET solution, single project (existing `TimezoneUtility` CLI), per plan.md's Project Structure:
- App code: `src/TimezoneUtility/{Commands,Models,Services/BatchConversion}/`
- Reused (not modified): `src/TimezoneUtility/Services/TimeConversion/{ITimeService,TimeService,TimeParser}.cs`
- Tests: `tests/TimezoneUtility.Unit/`, `tests/TimezoneUtility.Contract/`, `tests/TimezoneUtility.Integration/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare the new service folder and confirm the existing test projects build before any batch-specific code is added.

- [X] T001 Create the `Services/BatchConversion/` folder under `src/TimezoneUtility/Services/` (mirrors the existing `Services/TimeConversion/` convention per plan.md Project Structure); no files yet, folder only
- [X] T002 [P] Confirm the three existing test projects still build and pass with `dotnet test tests/TimezoneUtility.Unit/TimezoneUtility.Unit.csproj`, `dotnet test tests/TimezoneUtility.Contract/TimezoneUtility.Contract.csproj`, `dotnet test tests/TimezoneUtility.Integration/TimezoneUtility.Integration.csproj` before any new code is added (baseline check)
- [X] T003 [P] Create `tests/TimezoneUtility.Unit/Services/BatchConversion/` directory for the new unit tests (per plan.md test layout)

**Checkpoint**: Folders exist, baseline test run is green.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Data models shared by every user story (data-model.md entities) must exist before any row-processing/reporting logic can be written or tested, since every stage's method signatures depend on them.

**⚠️ CRITICAL**: No user story implementation or test task below can compile until this phase is complete.

- [X] T004 [P] Create `BatchConversionRow` record in `src/TimezoneUtility/Models/BatchConversionRow.cs` per data-model.md (RowNumber, RawTimestamp, RawSourceTimezone, RawTargetTimezone) — realizes SYS-001/SYS-002 output type, SYS-003 input type (REQ-001, REQ-002, REQ-028)
- [X] T005 [P] Create `RowConversionResult` record in `src/TimezoneUtility/Models/RowConversionResult.cs` per data-model.md (RowNumber, OriginalTimestamp, OriginalSourceTimezone, OriginalTargetTimezone, ConvertedInstant, TargetZone, computed ConvertedLocalDateTime) using NodaTime types — realizes SYS-008 output type (REQ-007, REQ-025)
- [X] T006 [P] Create `InvalidRowRecord` record in `src/TimezoneUtility/Models/InvalidRowRecord.cs` per data-model.md (RowNumber, RawTimestamp, RawSourceTimezone, RawTargetTimezone, Reason) — realizes SYS-007 output type (REQ-012, REQ-013, REQ-014, REQ-015)
- [X] T007 [P] Create `BatchRunSummary` record in `src/TimezoneUtility/Models/BatchRunSummary.cs` per data-model.md (InputFile, TotalRows, SucceededCount, FailedCount, NoRowsProcessed) — realizes SYS-010 output type (REQ-021, REQ-022, REQ-023)
- [X] T008 [P] [Unit] Write unit tests for the four new model records (invariant checks: `RowConversionResult.ConvertedLocalDateTime` is always derived from `ConvertedInstant`+`TargetZone`; `BatchRunSummary.TotalRows == SucceededCount + FailedCount`) in `tests/TimezoneUtility.Unit/Services/BatchConversion/ModelInvariantTests.cs` — supports REQ-007, REQ-021, REQ-022, REQ-025

**Checkpoint**: All four Models compile; model invariant tests pass. User story work can now begin.

---

## Phase 3: User Story 1 - Convert a Batch of Timestamps from a CSV File (Priority: P1) 🎯 MVP

**Goal**: Read a CSV of (timestamp, source_timezone, target_timezone) rows, convert every valid row, and write the results to a structured successful-output CSV — spec.md US1, requirements.md REQ-001..REQ-005, REQ-007, REQ-019, REQ-023, REQ-024, REQ-025, REQ-027, system-design.md SYS-001, SYS-002, SYS-004, SYS-005 (reused), SYS-008, SYS-009 (successful side), SYS-011.

**Independent Test**: Run `tzutil convert-batch <csv-with-valid-rows>` per quickstart.md and confirm the `<basename>.converted.csv` output contains one correctly converted record per input row, including a header-only file producing an empty (but present) output with a "no rows processed" message.

### Tests for User Story 1 (write first; must fail before implementation)

- [X] T009 [P] [US1] Unit test for `CsvBatchReader` header/column resolution (order-independent, extra columns ignored, unresolvable header raises a file-level error) in `tests/TimezoneUtility.Unit/Services/BatchConversion/CsvBatchReaderTests.cs` — verifies REQ-001, REQ-002, REQ-004, REQ-016, REQ-017, REQ-018; corresponds to STP-001-A, STP-001-B, STP-002-A, STP-002-B (STS-001-A1, STS-001-A2, STS-001-B1, STS-002-A1, STS-002-A2, STS-002-B1)
- [X] T010 [P] [US1] Unit test for `BatchTimestampInterpreter` (no explicit offset → source-timezone-authoritative; explicit offset takes precedence) in `tests/TimezoneUtility.Unit/Services/BatchConversion/BatchTimestampInterpreterTests.cs` — verifies REQ-024; corresponds to STP-004-A, STP-004-B (STS-004-A1, STS-004-B1)
- [X] T011 [P] [US1] Unit test for `SuccessfulResultFormatter` (output record contains original values + converted date/time/target-zone; date-boundary crossing captured; same-timezone no-op) in `tests/TimezoneUtility.Unit/Services/BatchConversion/SuccessfulResultFormatterTests.cs` — verifies REQ-005, REQ-007, REQ-025, REQ-027; corresponds to STP-005-A, STP-008-A, STP-008-B (STS-005-A1, STS-005-A2, STS-008-A1, STS-008-B1)
- [X] T012 [P] [US1] Unit test for `BatchRunSummaryGenerator`'s "no rows processed" / header-only path in `tests/TimezoneUtility.Unit/Services/BatchConversion/BatchRunSummaryGeneratorTests.cs` — verifies REQ-023; corresponds to STP-010-B, STP-011-B (STS-010-B2, STS-011-B1)
- [X] T013 [P] [US1] Contract test for the `convert-batch` CLI argument surface (`input-file` positional, `--output-dir`, `--json`, `--help`) and for the successful-output CSV schema (`row_number,original_timestamp,original_source_timezone,original_target_timezone,converted_timestamp,converted_timezone`) in `tests/TimezoneUtility.Contract/BatchConvertContractTests.cs` per `contracts/cli-batch-convert.md` and `contracts/successful-output-schema.md` — verifies REQ-001, REQ-007, REQ-019; corresponds to STP-009-A (STS-009-A1)
- [X] T014 [P] [US1] Integration test: full-file run with all-valid rows produces exact-count matching successful CSV, and a header-only file produces an empty successful CSV plus "no rows processed" message, per `quickstart.md` scenario, in `tests/TimezoneUtility.Integration/Commands/ConvertBatchCommandTests.cs` — verifies REQ-001, REQ-023, REQ-NF-005 (10k-row case may be added here or in US2, see T030); corresponds to ATP-001-A, ATP-002-A, ATP-004-A, ATP-023-A (SCN-001-A1, SCN-002-A1, SCN-004-A1, SCN-023-A1); STP-011-B (STS-011-B1)

### Implementation for User Story 1

- [X] T015 [US1] Implement `CsvBatchReader` in `src/TimezoneUtility/Services/BatchConversion/CsvBatchReader.cs` — SYS-001 (CSV File Intake: open/read, file-level error on missing/unreadable file) + SYS-002 (Header/Column Resolver: locate timestamp/source_timezone/target_timezone by name, order-independent, ignore extra columns, file-level error if unresolvable); realizes REQ-001, REQ-002, REQ-004, REQ-016, REQ-017, REQ-018, REQ-028 (blank-row skip at the reader level); depends on T004, T009
- [X] T016 [US1] Implement `BatchTimestampInterpreter` in `src/TimezoneUtility/Services/BatchConversion/BatchTimestampInterpreter.cs` — SYS-004 (Timestamp Interpretation Component: explicit-offset-vs-source-timezone-authoritative logic), reusing `Services/TimeConversion/TimeParser.cs`; realizes REQ-024; depends on T004, T010
- [X] T017 [US1] Implement `SuccessfulResultFormatter` in `src/TimezoneUtility/Services/BatchConversion/SuccessfulResultFormatter.cs` — SYS-008 (Successful Result Formatter: build one `RowConversionResult` per successful row), calling the reused `Services/TimeConversion/{ITimeService,TimeService}.cs` for the actual conversion (SYS-005 reuse, no new conversion logic); realizes REQ-005, REQ-007, REQ-025, REQ-026, REQ-027, REQ-NF-004; depends on T005, T016, T011
- [X] T018 [US1] Implement `BatchOutputWriter` (successful-CSV side) in `src/TimezoneUtility/Services/BatchConversion/BatchOutputWriter.cs` — SYS-009 (Output Writer: emit `<basename>.converted.csv` per `contracts/successful-output-schema.md`, always written even if empty); realizes REQ-019; depends on T005, T017
- [X] T019 [US1] Implement `BatchRunSummaryGenerator` in `src/TimezoneUtility/Services/BatchConversion/BatchRunSummaryGenerator.cs` — SYS-010 (Run Summary Generator: total/succeeded/failed counts, explicit zero, "no rows processed" message for header-only files); realizes REQ-021, REQ-022, REQ-023, REQ-NF-003; depends on T007, T012
- [X] T020 [US1] Implement `ConvertBatchCommand` in `src/TimezoneUtility/Commands/ConvertBatchCommand.cs` — SYS-011 (Batch Conversion Orchestrator: sequences file intake → column resolution → per-row processing → output writing → summary; owns the empty-file completion path), following the existing `ConvertCommand.cs` System.CommandLine pattern, wiring in `--output-dir`/`--json` options and exit codes (0/1/2/3) per `contracts/cli-batch-convert.md`; realizes REQ-001, REQ-016, REQ-017, REQ-018, REQ-023, REQ-NF-005; depends on T015, T018, T019, T013, T014
- [X] T021 [US1] Register `convert-batch` as a subcommand in `src/TimezoneUtility/Program.cs`, following the existing registration pattern for `ConvertCommand`/`NowCommand`/etc.; depends on T020

**Checkpoint**: `tzutil convert-batch <valid-csv>` produces a correct successful-output CSV and a "no rows processed" header-only run; T009–T014 all pass.

---

## Phase 4: User Story 2 - Continue Processing Despite Invalid Rows (Priority: P1)

**Goal**: Per-row failure isolation — invalid rows (bad timestamp, unrecognized timezone, missing field, blank row) never abort the run, and each is reported individually with row number and field-specific reason in a separate invalid-row-report CSV — spec.md US2, requirements.md REQ-006, REQ-008..REQ-015, REQ-020, REQ-026, REQ-028, REQ-029, REQ-NF-001, REQ-NF-002, system-design.md SYS-003, SYS-006, SYS-007, SYS-009 (invalid side).

**Independent Test**: Run `tzutil convert-batch <csv-with-mixed-valid-and-invalid-rows>` and confirm every invalid row appears exactly once in `<basename>.invalid-rows.csv` with its row number and a field-specific reason, while every valid row still appears correctly in the successful-output CSV (US1's output unaffected).

### Tests for User Story 2 (write first; must fail before implementation)

- [X] T022 [P] [US2] Unit test for `BatchRowValidator` covering all row-content equivalence classes (fully valid; unparseable timestamp; unrecognized source/target timezone; missing field; entirely blank row skipped) in `tests/TimezoneUtility.Unit/Services/BatchConversion/BatchRowValidatorTests.cs` — verifies REQ-003, REQ-008, REQ-009, REQ-010, REQ-015, REQ-028; corresponds to STP-003-A, STP-003-B (STS-003-A1, STS-003-A2, STS-003-B1, STS-003-B2)
- [X] T023 [P] [US2] Unit test for `InvalidRowReporter` producing exactly one distinct entry per failed row, with correct row number and field-specific reason text (referencing "timestamp", the unrecognized value, or the specific missing field name), and a zero-omission check across a 50-of-200-row batch in `tests/TimezoneUtility.Unit/Services/BatchConversion/InvalidRowReporterTests.cs` — verifies REQ-012, REQ-013, REQ-014, REQ-015, REQ-NF-002; corresponds to STP-007-A, STP-007-B (STS-007-A1, STS-007-B1)
- [X] T024 [P] [US2] Unit test for `BatchRowProcessor` fault-isolation: a row that throws during interpretation/conversion is routed to the invalid-row reporter and processing continues for all other rows, with a 100%-of-valid-rows-still-succeed assertion in a mixed 20-row batch (4 valid / 16 invalid) in `tests/TimezoneUtility.Unit/Services/BatchConversion/BatchRowProcessorTests.cs` — verifies REQ-006, REQ-011, REQ-NF-001; corresponds to STP-005-C, STP-006-A, STP-006-B (STS-005-C1, STS-006-A1, STS-006-B1)
- [X] T025 [P] [US2] Contract test for the invalid-row-report CSV schema (`row_number,raw_timestamp,raw_source_timezone,raw_target_timezone,reason`) and its separateness from the successful-output CSV in `tests/TimezoneUtility.Contract/BatchConvertContractTests.cs` per `contracts/invalid-row-report-schema.md` — verifies REQ-020; corresponds to STP-009-A (STS-009-A1)
- [X] T026 [P] [US2] Contract test for the output-write-failure exit code (3) and error message wording ("could not be saved" / results not discarded) in `tests/TimezoneUtility.Contract/BatchConvertContractTests.cs` per `contracts/cli-batch-convert.md`'s Output Write Failure example — verifies REQ-029; corresponds to STP-009-B (STS-009-B1)
- [X] T027 [P] [US2] Integration test: mixed valid/invalid-row file (unparseable timestamp, unrecognized source timezone, unrecognized target timezone, missing field, and an entirely blank row) produces the correct counts split between the two output files, in `tests/TimezoneUtility.Integration/Commands/ConvertBatchCommandTests.cs` — verifies REQ-006, REQ-008, REQ-009, REQ-010, REQ-011, REQ-012, REQ-013, REQ-014, REQ-015, REQ-020, REQ-028, REQ-NF-001, REQ-NF-002; corresponds to ATP-006-A, ATP-008-A, ATP-009-A, ATP-010-A, ATP-011-A, ATP-012-A, ATP-013-A, ATP-014-A, ATP-015-A, ATP-020-A, ATP-028-A, ATP-NF-001-A, ATP-NF-002-A (SCN-006-A1, SCN-008-A1, SCN-009-A1, SCN-010-A1, SCN-011-A1, SCN-012-A1, SCN-013-A1, SCN-014-A1, SCN-015-A1, SCN-020-A1, SCN-028-A1, SCN-NF-001-A1, SCN-NF-002-A1)
- [X] T028 [P] [US2] Integration test: DST edge cases (spring-forward gap timestamp, fall-back ambiguous-hour timestamp) resolve to successful conversions rather than being reported invalid, in `tests/TimezoneUtility.Integration/Commands/ConvertBatchCommandTests.cs` using NodaTime.Testing for deterministic DST fixtures — verifies REQ-026, REQ-NF-004; corresponds to ATP-026-A, ATP-026-B, ATP-NF-004-A (SCN-026-A1, SCN-026-B1, SCN-NF-004-A1); STP-005-B (STS-005-B1, STS-005-B2)
- [X] T029 [P] [US2] Integration test: file-level error paths (missing input file → exit code 2, unidentifiable header → exit code 2, no rows processed after a file-level error) in `tests/TimezoneUtility.Integration/Commands/ConvertBatchCommandTests.cs` — verifies REQ-016, REQ-017, REQ-018; corresponds to ATP-016-A, ATP-017-A, ATP-018-A (SCN-016-A1, SCN-017-A1, SCN-018-A1); STP-001-B, STP-011-A (STS-001-B1, STS-011-A1)
- [X] T030 [P] [US2] Integration test: a single 10,000-row CSV file completes in one run without splitting, producing 10,000 successful records, in `tests/TimezoneUtility.Integration/Commands/ConvertBatchCommandTests.cs` — verifies REQ-NF-005; corresponds to ATP-NF-005-A (SCN-NF-005-A1); STP-011-B (STS-011-B2)

### Implementation for User Story 2

- [X] T031 [US2] Implement `BatchRowValidator` in `src/TimezoneUtility/Services/BatchConversion/BatchRowValidator.cs` — SYS-003 (Row Validator: timestamp parseability, source/target timezone recognizability via `DateTimeZoneProviders.Tzdb.GetZoneOrNull`, missing-field detection, blank-row skip); realizes REQ-003, REQ-008, REQ-009, REQ-010, REQ-015, REQ-028; depends on T004, T022
- [X] T032 [US2] Implement `InvalidRowReporter` in `src/TimezoneUtility/Services/BatchConversion/InvalidRowReporter.cs` — SYS-007 (Invalid Row Reporter: build one `InvalidRowRecord` per failed row with field-specific reason text per `contracts/cli-batch-convert.md`'s Per-Row Invalid Reasons reference); realizes REQ-012, REQ-013, REQ-014, REQ-015, REQ-NF-002; depends on T006, T023
- [X] T033 [US2] Implement `BatchRowProcessor` in `src/TimezoneUtility/Services/BatchConversion/BatchRowProcessor.cs` — SYS-006 (Row Processing Controller: orchestrates validator → interpreter → conversion engine per row, catches per-row failures, routes to `InvalidRowReporter` or `SuccessfulResultFormatter`, guarantees no cross-row effect); realizes REQ-006, REQ-011, REQ-NF-001; depends on T031, T032, T017 (from US1), T024
- [X] T034 [US2] Extend `BatchOutputWriter` in `src/TimezoneUtility/Services/BatchConversion/BatchOutputWriter.cs` to also emit `<basename>.invalid-rows.csv` per `contracts/invalid-row-report-schema.md`, kept fully separate from the successful-output CSV, and to handle/report write failures without discarding already-computed in-memory results (exit code 3); realizes REQ-020, REQ-029; depends on T018 (from US1), T025, T026
- [X] T035 [US2] Update `ConvertBatchCommand` in `src/TimezoneUtility/Commands/ConvertBatchCommand.cs` to invoke `BatchRowProcessor` per row (replacing the US1 direct-formatter call) and to surface file-level-error and write-failure exit codes/messages per `contracts/cli-batch-convert.md`; realizes REQ-011, REQ-016, REQ-017, REQ-018, REQ-029; depends on T033, T034, T027, T028, T029, T030

**Checkpoint**: US1 + US2 both work independently — mixed-validity batches split correctly across the two output files, DST edge cases and 10k-row batches complete successfully, and file-level/write errors surface with the documented exit codes.

---

## Phase 5: User Story 3 - Review a Summary of the Batch Run (Priority: P2)

**Goal**: Present total/succeeded/failed row counts after a run, always showing an explicit zero failed count, sufficient on its own to classify the run as fully succeeded / fully failed / partially succeeded — spec.md US3, requirements.md REQ-021, REQ-022, REQ-NF-003, system-design.md SYS-010 (console/JSON presentation surface).

**Independent Test**: Run `tzutil convert-batch` against (a) an all-valid file, (b) an all-invalid file, and (c) a mixed file, and confirm the printed summary's succeeded/failed counts alone let a reader classify each run's outcome, with `--json` producing the documented JSON shape.

### Tests for User Story 3 (write first; must fail before implementation)

- [X] T036 [P] [US3] Unit test for `BatchRunSummaryGenerator`'s explicit-zero and classification behavior (all-succeeded, all-failed, partial) in `tests/TimezoneUtility.Unit/Services/BatchConversion/BatchRunSummaryGeneratorTests.cs` — verifies REQ-021, REQ-022, REQ-NF-003; corresponds to STP-010-A, STP-010-B (STS-010-A1, STS-010-A2, STS-010-B1)
- [X] T037 [P] [US3] Contract test for the human-readable and `--json` run-summary output shapes (fields: inputFile, totalRows, succeededCount, failedCount, noRowsProcessed, successfulOutputFile, invalidRowReportFile) in `tests/TimezoneUtility.Contract/BatchConvertContractTests.cs` per `contracts/cli-batch-convert.md`'s Run Summary section — verifies REQ-021, REQ-022, REQ-NF-003
- [X] T038 [P] [US3] Integration test: three separate runs (5 valid/0 invalid; 0 valid/5 invalid; 3 valid/2 invalid) each produce a summary whose succeeded/failed counts alone classify the run as fully succeeded / fully failed / partially succeeded, in `tests/TimezoneUtility.Integration/Commands/ConvertBatchCommandTests.cs` — verifies REQ-021, REQ-022, REQ-NF-003; corresponds to ATP-021-A, ATP-022-A, ATP-NF-003-A (SCN-021-A1, SCN-022-A1, SCN-NF-003-A1, SCN-NF-003-A2, SCN-NF-003-A3)

### Implementation for User Story 3

- [X] T039 [US3] Finalize `BatchRunSummaryGenerator`'s console/JSON rendering in `src/TimezoneUtility/Services/BatchConversion/BatchRunSummaryGenerator.cs` (explicit zero display; classification-sufficient two-count presentation) per `contracts/cli-batch-convert.md`; realizes REQ-021, REQ-022, REQ-NF-003; depends on T019 (from US1), T036
- [X] T040 [US3] Wire the `--json` flag through `ConvertBatchCommand` in `src/TimezoneUtility/Commands/ConvertBatchCommand.cs` to select JSON vs. human-readable summary rendering, consistent with other commands' `--json` handling (see existing `ConvertCommand.cs`); realizes REQ-021; depends on T035 (from US2), T037, T038

**Checkpoint**: All three user stories are independently functional; the full REQ-001..REQ-029/REQ-NF-001..REQ-NF-005 set is exercised by executable tests.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Repository-wide quality gates from plan.md's Constitution Check, applied across all of US1–US3.

- [X] T041 [P] Add XML doc comments to all new public types/members in `src/TimezoneUtility/Models/*.cs` and `src/TimezoneUtility/Services/BatchConversion/*.cs` per plan.md's "Documentation" quality gate
- [X] T042 Run `dotnet format --verify-no-changes` across the solution and fix any violations, per plan.md's "Linting" quality gate
- [X] T043 Verify test coverage for `Services/BatchConversion/*` and `Commands/ConvertBatchCommand.cs` meets the 80% minimum threshold per plan.md's "Test Coverage" quality gate
- [X] T044 Execute the `quickstart.md` end-to-end scenario manually (or via the integration test suite) and confirm its documented expected output matches actual CLI output byte-for-byte on the console summary
- [X] T045 Run all three test projects together — `dotnet test tests/TimezoneUtility.Unit/TimezoneUtility.Unit.csproj && dotnet test tests/TimezoneUtility.Contract/TimezoneUtility.Contract.csproj && dotnet test tests/TimezoneUtility.Integration/TimezoneUtility.Integration.csproj` — and confirm all pass, covering the full REQ→ATP/STP set in traceability-matrix.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories (every stage's signatures depend on the four Models)
- **User Story 1 (Phase 3)**: Depends on Foundational; delivers the MVP (successful-conversion path only)
- **User Story 2 (Phase 4)**: Depends on Foundational; depends on US1's `SuccessfulResultFormatter` (T017) and `BatchOutputWriter` (T018) since `BatchRowProcessor` (T033) calls the successful path on row success and `BatchOutputWriter` (T034) extends the same writer class
- **User Story 3 (Phase 5)**: Depends on Foundational and on US1's `BatchRunSummaryGenerator` scaffold (T019) and US2's `ConvertBatchCommand` wiring (T035)
- **Polish (Phase 6)**: Depends on US1, US2, and US3 all being complete

### Within Each User Story

- Tests (T009–T014, T022–T030, T036–T038) are written and confirmed failing before their corresponding implementation tasks
- Models (Phase 2) before any service
- SYS-001/002/003/004 (intake/validation/interpretation) before SYS-005/006 (conversion/orchestration) before SYS-007/008 (reporting/formatting) before SYS-009/010 (output/summary) before SYS-011 (orchestrator), matching system-design.md's Dependency View
- `Services/TimeConversion/{ITimeService,TimeService,TimeParser}.cs` are reused as-is — no task modifies them

### Parallel Opportunities

- T002, T003 (Setup) in parallel
- T004–T007 (four Models) in parallel; T008 after all four
- T009–T014 (US1 tests, different files) in parallel
- T022–T030 (US2 tests, mostly the same two integration/contract files but independent additions — coordinate before merging) in parallel where files differ; T022–T024 (three distinct unit test files) safely parallel
- T036–T038 (US3 tests) in parallel

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 (Setup) and Phase 2 (Foundational)
2. Complete Phase 3 (User Story 1) — successful-conversion path only, header-only file handled cleanly
3. **STOP and VALIDATE**: `tzutil convert-batch <all-valid.csv>` produces a correct `*.converted.csv` and the header-only case works
4. Demo if ready — this alone satisfies spec.md US1's Acceptance Scenarios 1 and 3

### Incremental Delivery

1. Setup + Foundational → foundation ready
2. Add US1 → validate independently → MVP demo (all-valid batches only)
3. Add US2 → validate independently → demo (mixed-validity batches, DST edge cases, 10k-row scale, file-level/write errors)
4. Add US3 → validate independently → demo (run-summary classification, `--json`)
5. Polish (Phase 6) → full quality-gate pass

## Notes

- [P] tasks touch different files and have no unmet dependency
- [Story] labels map every user-story-phase task to US1/US2/US3 for traceability back to spec.md
- Every task references the REQ-NNN/SYS-NNN/ATP-NNN/STP-NNN IDs it realizes or verifies, per v-model/traceability-matrix.md's Matrix A (REQ→ATP→SCN) and Matrix B (REQ→SYS→STP→STS)
- `Services/TimeConversion/{ITimeService,TimeService,TimeParser}.cs` (SYS-005 reuse) are never modified by any task in this list, per plan.md's Constraints
- Avoid: modifying `spec.md` or anything under `v-model/` (read-only constraints); modifying `plan.md`, `research.md`, `data-model.md`, `quickstart.md`, or `contracts/*.md` (prior-phase inputs, read-only for this task list)
