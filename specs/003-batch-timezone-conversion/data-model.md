# Data Model: Batch Timezone Conversion

**Feature**: 003-batch-timezone-conversion
**Date**: 2026-09-21
**Source**: Extracted from `spec.md` Key Entities and `v-model/system-design.md` Decomposition/Data Design Views

Each entity below is realized as an immutable C# record under `src/TimezoneUtility/Models/`, following the existing project convention (see `Models/TimeSlot.cs`, `Models/Location.cs`). Column/field names below match the CSV convention fixed in `research.md` §1.

---

## Core Domain Entities

### BatchConversionRow

One raw data row read from the input CSV (spec.md's "Conversion Row"). Produced by `Services/BatchConversion/CsvBatchReader.cs` (SYS-001 + SYS-002); consumed by `Services/BatchConversion/BatchRowProcessor.cs` (SYS-006).

```csharp
namespace TimezoneUtility.Models;

/// <summary>One raw data row read from the input batch CSV file.</summary>
public sealed record BatchConversionRow
{
    /// <summary>1-based row number, counting data rows only (header excluded, blank rows excluded per REQ-028).</summary>
    public required int RowNumber { get; init; }

    /// <summary>Raw timestamp cell value, exactly as read from the CSV (may be empty).</summary>
    public required string RawTimestamp { get; init; }

    /// <summary>Raw source timezone cell value, exactly as read from the CSV (may be empty).</summary>
    public required string RawSourceTimezone { get; init; }

    /// <summary>Raw target timezone cell value, exactly as read from the CSV (may be empty).</summary>
    public required string RawTargetTimezone { get; init; }
}
```

**Invariants**:
- `RowNumber` is assigned only to non-blank rows (a row that is entirely blank across all columns is skipped before a `BatchConversionRow` is constructed — REQ-028).
- Raw string fields are captured verbatim (not trimmed/validated) so `InvalidRowRecord.RawValues` can echo exactly what the user supplied.

**Maps to**: SYS-001 (CSV File Intake), SYS-002 (Header/Column Resolver) as their output type; SYS-003 (Row Validator) as its input type.

---

### RowConversionResult

The outcome of successfully converting one `BatchConversionRow` (spec.md's "Conversion Result"). Produced by `Services/BatchConversion/SuccessfulResultFormatter.cs` (SYS-008); consumed by `Services/BatchConversion/BatchOutputWriter.cs` (SYS-009) and `BatchRunSummaryGenerator.cs` (SYS-010).

```csharp
using NodaTime;

namespace TimezoneUtility.Models;

/// <summary>The successful conversion outcome for one batch row (REQ-007).</summary>
public sealed record RowConversionResult
{
    /// <summary>1-based row number this result corresponds to.</summary>
    public required int RowNumber { get; init; }

    /// <summary>Original raw timestamp value from the input row.</summary>
    public required string OriginalTimestamp { get; init; }

    /// <summary>Original raw source timezone value from the input row.</summary>
    public required string OriginalSourceTimezone { get; init; }

    /// <summary>Original raw target timezone value from the input row.</summary>
    public required string OriginalTargetTimezone { get; init; }

    /// <summary>The converted instant, computed via the existing ITimeService/TimeService (SYS-005 reuse).</summary>
    public required Instant ConvertedInstant { get; init; }

    /// <summary>The target timezone the instant is displayed in.</summary>
    public required DateTimeZone TargetZone { get; init; }

    /// <summary>Converted local date/time in the target zone (includes date, so date-boundary crossings are correct per REQ-025).</summary>
    public LocalDateTime ConvertedLocalDateTime => ConvertedInstant.InZone(TargetZone).LocalDateTime;
}
```

**Invariants**:
- `ConvertedLocalDateTime` is always derived from `ConvertedInstant` + `TargetZone` (never stored redundantly), matching the `TimeSlot` pattern already used elsewhere in the codebase.
- `RowNumber` matches the `BatchConversionRow.RowNumber` it was derived from, so successful and invalid outputs are traceable back to the original input line even though they're in separate files.

**Maps to**: SYS-005 (Timezone Conversion Engine) as the value it produces; SYS-008 (Successful Result Formatter) as its output type; SYS-009 (Output Writer) as input to the successful-output CSV.

---

### InvalidRowRecord

The outcome of a `BatchConversionRow` that failed validation or conversion (spec.md's "Invalid Row Report Entry"). Produced by `Services/BatchConversion/InvalidRowReporter.cs` (SYS-007); consumed by `BatchOutputWriter.cs` (SYS-009) and `BatchRunSummaryGenerator.cs` (SYS-010).

```csharp
namespace TimezoneUtility.Models;

/// <summary>The invalid-row outcome for one batch row that failed validation or conversion (REQ-012–REQ-015).</summary>
public sealed record InvalidRowRecord
{
    /// <summary>1-based row number this entry corresponds to.</summary>
    public required int RowNumber { get; init; }

    /// <summary>Original raw timestamp value from the input row (may be empty if missing).</summary>
    public required string RawTimestamp { get; init; }

    /// <summary>Original raw source timezone value from the input row (may be empty if missing).</summary>
    public required string RawSourceTimezone { get; init; }

    /// <summary>Original raw target timezone value from the input row (may be empty if missing).</summary>
    public required string RawTargetTimezone { get; init; }

    /// <summary>Specific, human-readable, field-referencing failure reason (e.g. "unparseable timestamp", "unrecognized timezone: Foo/Bar", "missing field: target_timezone").</summary>
    public required string Reason { get; init; }
}
```

**Invariants**:
- Exactly one `InvalidRowRecord` per failed row; never merged with another row's entry (REQ-NF-002).
- `Reason` always references a specific field per REQ-013/REQ-014/REQ-015 — never a generic "row invalid" message.

**Maps to**: SYS-003 (Row Validator) as the trigger; SYS-007 (Invalid Row Reporter) as its output type; SYS-009 (Output Writer) as input to the invalid-row-report CSV.

---

### BatchRunSummary

The run-level summary (spec.md's "Batch Conversion Request" attributes: total/success/failure counts). Produced by `Services/BatchConversion/BatchRunSummaryGenerator.cs` (SYS-010); consumed by `Commands/ConvertBatchCommand.cs` (SYS-011) for console/JSON presentation.

```csharp
namespace TimezoneUtility.Models;

/// <summary>Run-level total/succeeded/failed counts for one batch conversion run (REQ-021, REQ-022, REQ-023).</summary>
public sealed record BatchRunSummary
{
    /// <summary>Path to the input CSV file that was processed.</summary>
    public required string InputFile { get; init; }

    /// <summary>Total rows processed (blank rows excluded per REQ-028; header excluded).</summary>
    public required int TotalRows { get; init; }

    /// <summary>Number of rows that were successfully converted.</summary>
    public required int SucceededCount { get; init; }

    /// <summary>Number of rows reported as invalid. Always present and explicit, even when zero (REQ-022).</summary>
    public required int FailedCount { get; init; }

    /// <summary>True when the input file contained a header row and zero data rows (REQ-023); TotalRows/SucceededCount/FailedCount are all 0 in this case.</summary>
    public required bool NoRowsProcessed { get; init; }
}
```

**Invariants**:
- `TotalRows == SucceededCount + FailedCount` always holds (blank rows are excluded from all three counts per REQ-028's Assumption).
- `FailedCount` is always rendered explicitly, never omitted, including when it is `0` (REQ-022).
- `NoRowsProcessed` is `true` only for header-only input (REQ-023); it is `false` (not merely absent) whenever at least one data row existed, even if every row failed (spec.md US2 Acceptance Scenario 5: a run where every row is invalid still completes and is not the "no rows processed" case).

**Maps to**: SYS-010 (Run Summary Generator) as its output type; SYS-011 (Batch Conversion Orchestrator) as what it presents to the user.

---

## Supporting Concept (not a persisted type)

### File-Level Error

Not modeled as a data record — represented as a thrown/returned error result from `CsvBatchReader` (SYS-001/SYS-002) that short-circuits `ConvertBatchCommand` (SYS-011) before any `BatchConversionRow` is constructed (REQ-016–REQ-018). Surfaced to the user as a distinct console error message and a non-zero exit code (see `contracts/cli-batch-convert.md`), never as an `InvalidRowRecord`.

---

## Entity Relationship Summary

```
Input CSV File
   │  (SYS-001 CSV File Intake, SYS-002 Header/Column Resolver)
   ▼
BatchConversionRow (0..N, blank rows excluded)
   │  (SYS-003 Row Validator, SYS-004 Timestamp Interpretation, SYS-005 Timezone Conversion Engine — all orchestrated per-row by SYS-006 Row Processing Controller)
   ├──success──▶ RowConversionResult (SYS-008) ──▶ successful-output CSV (SYS-009)
   └──failure──▶ InvalidRowRecord (SYS-007)     ──▶ invalid-row-report CSV (SYS-009)

RowConversionResult (all) + InvalidRowRecord (all)
   │  (SYS-010 Run Summary Generator)
   ▼
BatchRunSummary ──▶ console/JSON output (SYS-011 Batch Conversion Orchestrator)
```

## Validation Rules

| Entity | Field | Rule | Traces to |
|---|---|---|---|
| BatchConversionRow | RawTimestamp | Must be parseable as an explicit-offset instant or a local date+time before conversion is attempted | REQ-008, REQ-013 |
| BatchConversionRow | RawSourceTimezone | Must resolve via `DateTimeZoneProviders.Tzdb.GetZoneOrNull` | REQ-009, REQ-014 |
| BatchConversionRow | RawTargetTimezone | Must resolve via `DateTimeZoneProviders.Tzdb.GetZoneOrNull` | REQ-010, REQ-014 |
| BatchConversionRow | (any of the three) | Missing (empty/whitespace) value reported with a field-specific reason, checked before format/recognizability validation | REQ-015 |
| BatchConversionRow | (all columns) | Entirely blank row is skipped, not counted, no record produced | REQ-028 |
| InvalidRowRecord | Reason | Must reference the specific offending field | REQ-013, REQ-014, REQ-015 |
| BatchRunSummary | FailedCount | Always displayed, including 0 | REQ-022 |
