# Quickstart: Batch Timezone Conversion

**Feature**: 003-batch-timezone-conversion
**Date**: 2026-09-21

This guide validates the `tzutil convert-batch` command end-to-end once implemented. It assumes the general repository setup already covered in `specs/001-timezone-utility/quickstart.md` (`.NET 8 SDK`, `dotnet restore`, `dotnet build`).

---

## Prerequisites

- .NET 8 SDK installed (`dotnet --version` → `8.0.x`)
- Repository built: `dotnet build` from the repository root

---

## 1. Create a Sample Input CSV

Create `batch.csv` in a scratch directory (e.g. `/tmp/tz-batch-demo/batch.csv`) with the following contents, exercising a valid row, a date-boundary-crossing row, a same-timezone row, and three invalid rows (bad timestamp, unrecognized timezone, missing field):

```csv
timestamp,source_timezone,target_timezone,note
2026-09-21 09:00:00,America/New_York,Europe/London,valid
2026-09-21 23:15:00,America/New_York,Asia/Tokyo,date boundary crossing
2026-09-21T09:00:00-04:00,America/New_York,America/New_York,same timezone no-op
not-a-timestamp,America/New_York,Asia/Tokyo,bad timestamp
2026-09-21 10:00:00,Mars/OlympusMons,Asia/Tokyo,bad timezone
2026-09-21 10:00:00,America/New_York,,missing target timezone
```

Note the extra `note` column — it must be ignored by the command (REQ-004) and preserved verbatim in the invalid-row report where applicable.

---

## 2. Run the Batch Conversion

```bash
dotnet run --project src/TimezoneUtility -- convert-batch /tmp/tz-batch-demo/batch.csv
```

### Expected Console Output

```
Batch conversion complete: /tmp/tz-batch-demo/batch.csv
  Total rows processed: 6
  Succeeded:             3
  Failed:                3

Successful output:      /tmp/tz-batch-demo/batch.converted.csv
Invalid-row report:     /tmp/tz-batch-demo/batch.invalid-rows.csv
```

### Expected Exit Code

```bash
echo $?
# 0  (the run completed; partial failure is not a process-level error — see contracts/cli-batch-convert.md)
```

---

## 3. Inspect the Two Output Files

```bash
cat /tmp/tz-batch-demo/batch.converted.csv
```
Expected: a header row plus 3 data rows (row numbers 1, 2, 3), with row 2's `converted_timestamp` showing the date advanced by one day (crossing into `2026-09-22`) and row 3 showing an unchanged same-timezone conversion. Schema: see `contracts/successful-output-schema.md`.

```bash
cat /tmp/tz-batch-demo/batch.invalid-rows.csv
```
Expected: a header row plus 3 data rows (row numbers 4, 5, 6), each with a specific `reason`:
- Row 4: `unparseable timestamp: not-a-timestamp`
- Row 5: `unrecognized timezone: Mars/OlympusMons`
- Row 6: `missing field: target_timezone`

Schema: see `contracts/invalid-row-report-schema.md`.

---

## 4. Validate the Empty-File Path (User Story 1, Scenario 3)

```bash
printf 'timestamp,source_timezone,target_timezone\n' > /tmp/tz-batch-demo/empty.csv
dotnet run --project src/TimezoneUtility -- convert-batch /tmp/tz-batch-demo/empty.csv
echo $?
```
Expected: console output states "No rows were processed (input file contained only a header row)."; both output files are still created but contain only their header row; exit code `0`.

---

## 5. Validate the All-Invalid Path (User Story 2, Scenario 5)

```bash
printf 'timestamp,source_timezone,target_timezone\nbad,bad,bad\n' > /tmp/tz-batch-demo/allbad.csv
dotnet run --project src/TimezoneUtility -- convert-batch /tmp/tz-batch-demo/allbad.csv
echo $?
```
Expected: run completes (does not crash/abort), successful output has 0 data rows, invalid-row report has exactly 1 data row, summary shows `Total: 1, Succeeded: 0, Failed: 1`, exit code `0`.

---

## 6. Validate a File-Level Error (Missing File)

```bash
dotnet run --project src/TimezoneUtility -- convert-batch /tmp/tz-batch-demo/does-not-exist.csv
echo $?
```
Expected: `Error: Could not open input file ...` printed, no output files written, exit code `2`.

---

## 7. Validate `--json` Summary Output

```bash
dotnet run --project src/TimezoneUtility -- convert-batch /tmp/tz-batch-demo/batch.csv --json
```
Expected: a single JSON object on stdout matching the shape in `contracts/cli-batch-convert.md`'s "Run Summary (Console Output)" section; the two CSV files are still written in CSV form.

---

## Running the Automated Test Suite for This Feature

```bash
dotnet test tests/TimezoneUtility.Unit/TimezoneUtility.Unit.csproj
dotnet test tests/TimezoneUtility.Contract/TimezoneUtility.Contract.csproj
dotnet test tests/TimezoneUtility.Integration/TimezoneUtility.Integration.csproj
```

See `plan.md`'s Project Structure section for where new tests for this feature land in each project, and `v-model/system-test.md` / `v-model/acceptance-plan.md` for the full set of scenarios these tests must cover (STP-001-A through the SYS-011 orchestration tests, and the three acceptance-plan user-story test tiers).
