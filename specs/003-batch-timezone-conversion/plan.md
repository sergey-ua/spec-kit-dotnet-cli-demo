# Implementation Plan: Batch Timezone Conversion

**Branch**: `003-batch-timezone-conversion` | **Date**: 2026-09-21 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/003-batch-timezone-conversion/spec.md`, formalized in `v-model/requirements.md` (REQ-001–REQ-029, REQ-NF-001–REQ-NF-005) and `v-model/system-design.md` (SYS-001–SYS-011).

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Add a `tzutil convert-batch` CLI command that reads a CSV file of (timestamp, source timezone, target timezone) rows, converts every row it can, and writes two separate output artifacts: a successful-conversion CSV and an invalid-row-report CSV, plus a run summary (total/succeeded/failed) printed to the console. Invalid rows (bad timestamp, unrecognized timezone, missing field) never abort the run — each row is processed and reported independently (SYS-006 Row Processing Controller enforces isolation). The command is implemented as a thin System.CommandLine command following the existing `ConvertCommand` pattern, and reuses the existing `ITimeService`/`TimeService`/`TimeParser` conversion and DST-resolution logic from `specs/001-timezone-utility/` rather than reimplementing timezone math (SYS-005 requirement). This is a demo-scale, single-task feature: one new command, its supporting services/models, and tests — not a multi-phase rollout.

## Technical Context

**Language/Version**: .NET 8 (C#), matching the existing `TimezoneUtility` project

**Primary Dependencies**: NodaTime (existing, timezone/DST conversion via reused `ITimeService`), System.CommandLine (existing, CLI parsing) — no new CSV library is added; a small internal CSV reader/writer (`Services/BatchConversion/CsvLineParser.cs`) is implemented directly, matching the project's small-dependency-footprint pattern (RFC 4180 quoting is not required for this demo: fields are comma-delimited with support for double-quoted values containing commas, which covers the documented column set)

**Storage**: None persistent; input is a user-supplied CSV file read once per run, outputs are two CSV files written to disk next to the run (see Output Convention below) — no database or config file involved

**Testing**: xUnit (existing `TimezoneUtility.Unit`, `TimezoneUtility.Contract`, `TimezoneUtility.Integration` projects), FluentAssertions, NodaTime.Testing for deterministic DST-transition tests

**Target Platform**: Same as existing CLI — cross-platform single binary (Windows/macOS/Linux, x64/arm64)

**Project Type**: CLI (single command added to the existing `TimezoneUtility` CLI project)

**Performance Goals**: A 10,000-row batch completes in a single run (REQ-NF-005) without requiring the user to split the file; no specific latency SLA beyond "single run, no manual splitting" — rows are processed via a streaming read (one row in memory at a time plus two accumulating result lists), so memory scales with output size, not a multiple of file size

**Constraints**: MUST reuse existing `ITimeService`/`TimeService`/`TimeParser` conversion/DST logic (SYS-005) rather than duplicating timezone resolution; MUST keep the two outputs (successful CSV, invalid-row report) as separate files (SYS-009, REQ-020); MUST NOT abort the run on a per-row failure (REQ-011); file-level errors (missing file, unresolvable header) MUST short-circuit before any row processing (REQ-018)

**Scale/Scope**: Demo-scale — one CLI command, ~11 small classes/records mapping 1:1 to SYS-001..SYS-011, one implementation task per the assignment's "single implementation task" guidance; batches up to 10,000+ rows (REQ-NF-005)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Research Gate (Phase 0 Entry)

| Principle | Gate Status | Notes |
|-----------|-------------|-------|
| **I. Code Quality** | ✅ PASS | Each SYS component maps to one small, single-responsibility class/record (see SYS↔Class Mapping below); no file is expected to approach the 300-line limit. |
| **II. Testing Standards** | ✅ PASS | Unit tests for Row Validator/Timestamp Interpretation/Row Processing Controller/Run Summary Generator; Contract tests for the CLI argument surface and the two CSV output schemas; Integration tests for full-file runs (mixed valid/invalid, all-invalid, header-only, DST edge cases, 10k-row scale). |
| **III. UX Consistency** | ✅ PASS | Follows the existing `tzutil <command> [options]` pattern (`ConvertCommand`), supports `--json` for the run summary consistent with other commands, produces clear per-row, field-referencing error reasons (REQ-013–REQ-015). |
| **IV. Performance** | ✅ PASS | Streaming row-by-row processing keeps memory proportional to accumulated results, not a multiple of file size; no network calls; startup unaffected (command registered like existing commands, no eager loading). |

### Post-Design Gate (Phase 1 Exit) ✅

| Principle | Gate Status | Verification |
|-----------|-------------|--------------|
| **I. Code Quality** | ✅ PASS | data-model.md defines immutable C# records per entity (`BatchConversionRow`, `RowConversionResult`, `InvalidRowRecord`, `BatchRunSummary`), each with a single clear purpose; services organized under `Services/BatchConversion/` mirroring the existing `Services/TimeConversion/` convention. |
| **II. Testing Standards** | ✅ PASS | contracts/ define the exact CLI option surface and both CSV schemas so contract tests can assert against a stable target; quickstart.md gives a runnable end-to-end validation scenario. |
| **III. UX Consistency** | ✅ PASS | contracts/cli-batch-convert.md defines exit codes and console summary format consistent with `contracts/cli-interface.md` from 001-timezone-utility. |
| **IV. Performance** | ✅ PASS | Design confirms streaming intake (SYS-001) and per-row isolation (SYS-006) avoid loading the whole file into a single blocking data structure beyond the two result lists. |

**Quality Gates Alignment**:
- Linting: `dotnet format --verify-no-changes`
- Type Safety: C# strong typing, nullable reference types enabled (already project-wide)
- Test Coverage: 80% minimum threshold per constitution, applied to new `Services/BatchConversion/*` and `Commands/ConvertBatchCommand.cs` code
- Documentation: XML docs for public APIs (all new public types/members)

## Project Structure

### Documentation (this feature)

```text
specs/003-batch-timezone-conversion/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/            # Phase 1 output (/speckit.plan command)
│   ├── cli-batch-convert.md
│   ├── successful-output-schema.md
│   └── invalid-row-report-schema.md
├── v-model/              # Approved, out of scope for this command (requirements.md, system-design.md, system-test.md, acceptance-plan.md, traceability-matrix.md)
└── tasks.md              # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
└── TimezoneUtility/                          # Existing main CLI application (unchanged root)
    ├── Commands/
    │   └── ConvertBatchCommand.cs             # NEW - SYS-011 Batch Conversion Orchestrator (CLI entry point + top-level sequencing)
    ├── Models/
    │   ├── BatchConversionRow.cs               # NEW - raw Conversion Row (SYS-001/SYS-002 output)
    │   ├── RowConversionResult.cs              # NEW - successful per-row result (SYS-008 output)
    │   ├── InvalidRowRecord.cs                 # NEW - invalid-row report entry (SYS-007 output)
    │   └── BatchRunSummary.cs                  # NEW - run summary counts (SYS-010 output)
    └── Services/
        └── BatchConversion/                    # NEW service folder, mirrors Services/TimeConversion/ convention
            ├── CsvBatchReader.cs                # NEW - SYS-001 CSV File Intake + SYS-002 Header/Column Resolver
            ├── BatchRowValidator.cs             # NEW - SYS-003 Row Validator
            ├── BatchTimestampInterpreter.cs     # NEW - SYS-004 Timestamp Interpretation Component
            │                                    #   (SYS-005 Timezone Conversion Engine = reuse of existing
            │                                    #    Services/TimeConversion/{ITimeService,TimeService,TimeParser}.cs — no new file)
            ├── BatchRowProcessor.cs             # NEW - SYS-006 Row Processing Controller
            ├── InvalidRowReporter.cs            # NEW - SYS-007 Invalid Row Reporter
            ├── SuccessfulResultFormatter.cs     # NEW - SYS-008 Successful Result Formatter
            ├── BatchOutputWriter.cs             # NEW - SYS-009 Output Writer (writes both CSVs)
            └── BatchRunSummaryGenerator.cs      # NEW - SYS-010 Run Summary Generator

tests/
├── TimezoneUtility.Unit/
│   └── Services/BatchConversion/               # NEW - unit tests for SYS-002..SYS-010 logic in isolation
├── TimezoneUtility.Contract/
│   └── BatchConvertContractTests.cs            # NEW - CLI option surface + CSV output schema contract tests
└── TimezoneUtility.Integration/
    └── Commands/ConvertBatchCommandTests.cs    # NEW - full-file run scenarios (US1-US3, edge cases, 10k-row scale)
```

**Structure Decision**: Single .NET solution, single command added to the existing `TimezoneUtility` CLI project — no new project. New service logic lives in a new `Services/BatchConversion/` folder alongside the existing `Services/TimeConversion/`, `Services/LocationResolver/`, `Services/MeetingOptimizer/` domain folders, following the repository's established domain-folder-per-capability convention. `Services/TimeConversion/` is reused, not modified, for SYS-005. Tests land in the three existing test projects (Unit for component logic, Contract for CLI/CSV-schema stability, Integration for end-to-end file-based runs), matching the 001-timezone-utility precedent.

## Complexity Tracking

> **No constitution violations identified.** The design adds one command and one new service folder, following existing conventions; no additional projects, no new architectural patterns, no unjustified complexity.
