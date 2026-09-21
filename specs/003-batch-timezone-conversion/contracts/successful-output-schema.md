# Output Contract: Successful Conversion CSV

**Version**: 1.0.0
**Date**: 2026-09-21
**Feature**: 003-batch-timezone-conversion
**File**: `<input-basename>.converted.csv` (see `cli-batch-convert.md`)

One row per successfully converted input row (REQ-007, REQ-019). Never contains invalid-row entries — those go to `invalid-row-report-schema.md`'s file (REQ-020).

## Columns

| Column | Type | Description |
|---|---|---|
| `row_number` | integer | 1-based data-row number from the input file (header and blank rows excluded from numbering) |
| `original_timestamp` | string | The raw `timestamp` cell value exactly as supplied in the input |
| `original_source_timezone` | string | The raw `source_timezone` cell value exactly as supplied in the input |
| `original_target_timezone` | string | The raw `target_timezone` cell value exactly as supplied in the input |
| `converted_timestamp` | string | The converted date+time in the target timezone, ISO-8601 with offset (e.g. `2026-09-22T02:30:00+09:00`) — includes the resulting calendar date, correct across date-boundary crossings (REQ-025) |
| `converted_timezone` | string | The target IANA timezone identifier the converted timestamp is expressed in (same value as `original_target_timezone`, included for machine-readability without needing to re-join to the input) |

## Example

```csv
row_number,original_timestamp,original_source_timezone,original_target_timezone,converted_timestamp,converted_timezone
1,2026-09-21 23:15:00,America/New_York,Asia/Tokyo,2026-09-22T12:15:00+09:00,Asia/Tokyo
2,2026-09-21T09:00:00-04:00,America/New_York,America/New_York,2026-09-21T09:00:00-04:00,America/New_York
```

Row 1 demonstrates a date-boundary crossing (REQ-025). Row 2 demonstrates an explicit-offset timestamp and a same-timezone (no-op) conversion (REQ-027).

## Empty Case

If no rows succeeded (including the header-only "no rows processed" case, REQ-023), this file is still written, containing only the header row and zero data rows.
