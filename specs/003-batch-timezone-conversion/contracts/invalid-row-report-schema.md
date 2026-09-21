# Output Contract: Invalid Row Report CSV

**Version**: 1.0.0
**Date**: 2026-09-21
**Feature**: 003-batch-timezone-conversion
**File**: `<input-basename>.invalid-rows.csv` (see `cli-batch-convert.md`)

One row per input row that failed validation or conversion (REQ-012–REQ-015). Kept entirely separate from the successful output file (REQ-020) — never intermixed.

## Columns

| Column | Type | Description |
|---|---|---|
| `row_number` | integer | 1-based data-row number from the input file (header and blank rows excluded from numbering) |
| `raw_timestamp` | string | The raw `timestamp` cell value exactly as supplied (empty string if the field was missing) |
| `raw_source_timezone` | string | The raw `source_timezone` cell value exactly as supplied (empty string if the field was missing) |
| `raw_target_timezone` | string | The raw `target_timezone` cell value exactly as supplied (empty string if the field was missing) |
| `reason` | string | Specific, human-readable, field-referencing failure reason (see the Per-Row Invalid Reasons reference in `cli-batch-convert.md`) |

## Example

```csv
row_number,raw_timestamp,raw_source_timezone,raw_target_timezone,reason
3,not-a-timestamp,America/New_York,Asia/Tokyo,unparseable timestamp: not-a-timestamp
4,2026-09-21 10:00:00,Mars/OlympusMons,Asia/Tokyo,unrecognized timezone: Mars/OlympusMons
5,2026-09-21 10:00:00,America/New_York,,missing field: target_timezone
```

## Empty Case

If every row succeeded, this file is still written, containing only the header row and zero data rows — a failed count of zero is never represented by omitting this file (REQ-022 requires the summary to display zero explicitly; the file itself follows the same "always present, possibly empty" rule for consistency).
