# CLI Interface Contract: tzutil convert-batch

**Version**: 1.0.0
**Date**: 2026-09-21
**Status**: Draft
**Feature**: 003-batch-timezone-conversion

This document defines the command-line interface contract for the new batch timezone conversion command, following the same contract style as `specs/001-timezone-utility/contracts/cli-interface.md`. It is additive to that document — existing commands (`now`, `convert`, `meeting`, `dashboard`, `profile`, `config`) are unchanged.

---

## `tzutil convert-batch`

Convert a batch of timestamps listed in a CSV file, isolating and reporting invalid rows individually.

```
tzutil convert-batch <input-file> [options]
```

**Arguments**:

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `input-file` | string (path) | Yes | Path to the input CSV file (see `successful-output-schema.md`'s companion, the CSV Input Schema section below, for required columns) |

**Options**:

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `--output-dir` | string (path) | same directory as `input-file` | Directory to write both output files into |
| `--json` | flag | false | Print the run summary as JSON instead of human-readable text (does not affect the two CSV output files, which are always written in CSV form) |

**Global options** `--help`/`-h` and `--version` behave the same as for all other `tzutil` commands.

### CSV Input Schema

The input CSV file MUST contain a header row identifying, by name (case-insensitive, order-independent), these three columns:

| Column name | Required | Description |
|---|---|---|
| `timestamp` | Yes | The timestamp to convert. Either an ISO-8601 string with an explicit UTC offset (e.g. `2026-09-21T14:30:00-04:00`), in which case the offset takes precedence, or a local date+time with no offset (e.g. `2026-09-21 14:30:00`), in which case `source_timezone` is authoritative (REQ-024). |
| `source_timezone` | Yes | IANA timezone identifier the timestamp is local to when no explicit offset is embedded (e.g. `America/New_York`). |
| `target_timezone` | Yes | IANA timezone identifier to convert the timestamp into (e.g. `Europe/London`). |

Any additional columns present in the file are ignored (REQ-004). A row that is entirely blank across all columns is skipped and not counted in the run summary (REQ-028).

### Output Files

Two files are always written on a successful (non-file-level-error) run, even if one of the two result sets is empty:

| File | Naming | Contents |
|---|---|---|
| Successful output | `<input-basename>.converted.csv` | One row per successfully converted input row — see `successful-output-schema.md` |
| Invalid-row report | `<input-basename>.invalid-rows.csv` | One row per invalid input row — see `invalid-row-report-schema.md` |

`<input-basename>` is `input-file`'s file name without its extension (e.g. `batch.csv` → `batch.converted.csv` and `batch.invalid-rows.csv`). Both files are written to `--output-dir` if given, else to the same directory as `input-file`.

### Run Summary (Console Output)

**Human-readable (default)**:
```
$ tzutil convert-batch batch.csv

Batch conversion complete: batch.csv
  Total rows processed: 8
  Succeeded:             6
  Failed:                2

Successful output:      batch.converted.csv
Invalid-row report:     batch.invalid-rows.csv
```

When the input file has a header row and zero data rows (REQ-023):
```
$ tzutil convert-batch empty.csv

Batch conversion complete: empty.csv
  No rows were processed (input file contained only a header row).

Successful output:      empty.converted.csv
Invalid-row report:     empty.invalid-rows.csv
```

**JSON (`--json`)**:
```json
{
  "inputFile": "batch.csv",
  "totalRows": 8,
  "succeededCount": 6,
  "failedCount": 2,
  "noRowsProcessed": false,
  "successfulOutputFile": "batch.converted.csv",
  "invalidRowReportFile": "batch.invalid-rows.csv"
}
```

### Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Run completed (includes runs with `failedCount > 0`, and the header-only "no rows processed" case — a completed run with some or all rows failing is still success at the process level, per REQ-009/SC-002: the batch mechanism itself did not abort) |
| 1 | Invalid CLI input (e.g. missing required argument, malformed `--output-dir`) |
| 2 | File-level error: input file missing/unreadable, or required columns could not be identified from the header (REQ-016–REQ-018) |
| 3 | Output write failure: one or both output files could not be written (REQ-029); already-computed in-memory results are not discarded, and the error message states which file failed |

**File-Level Error Human Output Example**:
```
$ tzutil convert-batch missing.csv
Error: Could not open input file "missing.csv": file not found.
```
```
$ tzutil convert-batch badheader.csv
Error: Could not identify required columns in "badheader.csv". Expected a header row containing "timestamp", "source_timezone", and "target_timezone" (case-insensitive, any order). Found columns: "time", "tz".
```

**Output Write Failure Human Output Example**:
```
$ tzutil convert-batch batch.csv --output-dir /readonly
Error: Batch conversion finished (6 succeeded, 2 failed) but could not write output to "/readonly": permission denied. No results were discarded; re-run with a writable --output-dir to retry writing.
```

### Per-Row Invalid Reasons (Reference)

Reasons written into the invalid-row report's `reason` column (REQ-013–REQ-015), used verbatim or with the specific field/value substituted:

| Condition | Reason text pattern |
|---|---|
| Timestamp cell unparseable | `unparseable timestamp: <raw value>` |
| Source or target timezone unrecognized | `unrecognized timezone: <raw value>` |
| A required field is empty/missing | `missing field: <timestamp\|source_timezone\|target_timezone>` |
