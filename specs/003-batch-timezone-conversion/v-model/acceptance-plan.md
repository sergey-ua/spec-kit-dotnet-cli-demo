# Acceptance Test Plan: Batch Timezone Conversion

**Feature Branch**: `003-batch-timezone-conversion`
**Source Document**: `specs/003-batch-timezone-conversion/v-model/requirements.md`
**Generated**: 2026-09-21
**Status**: Draft

## Overview

This document pairs every requirement (`REQ-NNN` / `REQ-NF-NNN`) in `requirements.md` with Test Cases (`ATP-NNN-X`) and BDD-style User Scenarios (`SCN-NNN-X#`), per the V-Model three-tier acceptance testing approach. Coverage target is 100%: every REQ has at least one ATP, and every ATP has at least one SCN.

Note: Per `requirements.md`'s "Requirements Flagged for Clarification" section, three items were deliberately NOT formalized as requirements (explicit-offset-vs-source-timezone inconsistency reporting; a maximum file-size ceiling above the 10,000-row floor; exact CSV header column naming as a planning-phase detail). No REQ ID exists for these, so no test case is generated for them here, consistent with the instruction not to invent coverage for out-of-scope/clarification-pending capabilities.

---

## Functional Requirements

### Requirement Validation: REQ-001 (Accept CSV input with timestamp/source/target columns)

#### Test Case: ATP-001-A (Batch conversion accepts a CSV file with the required columns)
**Linked Requirement:** REQ-001
**Description:** Validates that the system accepts a CSV file containing at minimum a timestamp column, a source timezone column, and a target timezone column, one row per timestamp to convert.
**Validation Condition:** A CSV file with a header row (`timestamp`, `source_tz`, `target_tz`) and 3 valid data rows is submitted for batch conversion.
**Expected Result:** The batch run completes without a file-level error, and the successful-output set contains exactly 3 records, one per input row.

* **User Scenario: SCN-001-A1**
  * **Given** a CSV file exists with header row `timestamp,source_tz,target_tz` and 3 valid data rows
  * **When** the user runs the batch conversion against that file
  * **Then** the batch run completes with no file-level error and produces exactly 3 successful conversion records

---

### Requirement Validation: REQ-002 (Identify columns via header, not fixed order)

#### Test Case: ATP-002-A (Columns are identified by header name regardless of order)
**Linked Requirement:** REQ-002
**Description:** Validates that the system locates the timestamp, source timezone, and target timezone columns by their header names even when they are not in the canonical order.
**Validation Condition:** A CSV file whose header row lists the required columns in the order `target_tz,timestamp,source_tz` (reordered) with one valid data row is submitted.
**Expected Result:** The single row is processed as a successful conversion, with the correct field values matched to timestamp, source timezone, and target timezone respectively.

* **User Scenario: SCN-002-A1**
  * **Given** a CSV file has header row `target_tz,timestamp,source_tz` (columns reordered from canonical) with one valid data row
  * **When** the user runs the batch conversion against that file
  * **Then** the row is converted successfully using the correct timestamp, source timezone, and target timezone values matched by header name

---

### Requirement Validation: REQ-003 (Accept IANA timezone identifiers)

#### Test Case: ATP-003-A (IANA timezone identifiers are accepted for source and target)
**Linked Requirement:** REQ-003
**Description:** Validates that the system accepts source and target timezone values expressed as IANA identifiers such as "America/New_York".
**Validation Condition:** A CSV row specifies `source_tz=America/New_York` and `target_tz=Asia/Tokyo`.
**Expected Result:** The row is processed as a successful conversion with the output record's target timezone identifier equal to "Asia/Tokyo".

* **User Scenario: SCN-003-A1**
  * **Given** a CSV row has timestamp "2026-01-15 09:00", source timezone "America/New_York", and target timezone "Asia/Tokyo"
  * **When** the user runs the batch conversion against that row
  * **Then** the row succeeds and the output record's target timezone identifier is "Asia/Tokyo"

---

### Requirement Validation: REQ-004 (Ignore unrecognized extra columns)

#### Test Case: ATP-004-A (Extra unrecognized columns are ignored and the row still converts)
**Linked Requirement:** REQ-004
**Description:** Validates that a row containing extra, unexpected columns beyond timestamp/source/target is still processed correctly using only the recognized columns.
**Validation Condition:** A CSV file's header row includes an extra column `notes` alongside `timestamp,source_tz,target_tz`, and one data row populates all four columns.
**Expected Result:** The row is processed as a successful conversion, and the output record contains only the recognized fields (timestamp, source, target, converted result), unaffected by the `notes` column's content.

* **User Scenario: SCN-004-A1**
  * **Given** a CSV file has header row `timestamp,source_tz,target_tz,notes` and one data row with a non-empty value in the `notes` column
  * **When** the user runs the batch conversion against that file
  * **Then** the row is converted successfully using only the timestamp, source timezone, and target timezone columns

---

### Requirement Validation: REQ-005 (Convert valid rows applying DST rules)

#### Test Case: ATP-005-A (Valid row is converted from source to target timezone with DST applied)
**Linked Requirement:** REQ-005
**Description:** Validates that a row passing validation is converted from its source timezone to its target timezone, applying the DST rules in effect for that specific timestamp.
**Validation Condition:** A row with timestamp "2026-07-01 12:00", source "America/New_York" (EDT, UTC-4 in July), target "UTC" is converted.
**Expected Result:** The output record's converted timestamp equals "2026-07-01 16:00" in "UTC", reflecting the -4 hour EDT offset.

* **User Scenario: SCN-005-A1**
  * **Given** a CSV row has timestamp "2026-07-01 12:00", source timezone "America/New_York", and target timezone "UTC"
  * **When** the user runs the batch conversion against that row
  * **Then** the output record's converted timestamp is "2026-07-01 16:00" in timezone "UTC"

---

### Requirement Validation: REQ-006 (Rows processed independently)

#### Test Case: ATP-006-A (One row's failure does not affect another row's success)
**Linked Requirement:** REQ-006
**Description:** Validates that the success or failure outcome of one row has no effect on the processing or outcome of any other row.
**Validation Condition:** A CSV file has row 1 with an unparseable timestamp and row 2 with fully valid data, in that order.
**Expected Result:** Row 2 appears in the successful-output set with its correctly converted timestamp, and row 1 appears in the invalid-row report; row 2's outcome is unaffected by row 1's failure.

* **User Scenario: SCN-006-A1**
  * **Given** a CSV file has row 1 with timestamp value "not-a-date" and row 2 with timestamp "2026-03-10 08:00", source "America/Chicago", target "UTC"
  * **When** the user runs the batch conversion against that file
  * **Then** row 2 appears in the successful output with its correctly converted timestamp while row 1 appears in the invalid-row report

---

### Requirement Validation: REQ-007 (Output record contains original values and converted timestamp)

#### Test Case: ATP-007-A (Successful output record includes original input and converted date/time/timezone)
**Linked Requirement:** REQ-007
**Description:** Validates that each successfully converted row's output record contains the row's original input values plus the resulting converted timestamp (date, time, and target timezone identifier).
**Validation Condition:** A row with timestamp "2026-05-01 10:00", source "Europe/London", target "Asia/Kolkata" is converted.
**Expected Result:** The output record contains the original values "2026-05-01 10:00", "Europe/London", "Asia/Kolkata" and a converted timestamp with a date, a time, and target timezone identifier "Asia/Kolkata".

* **User Scenario: SCN-007-A1**
  * **Given** a CSV row has timestamp "2026-05-01 10:00", source timezone "Europe/London", and target timezone "Asia/Kolkata"
  * **When** the user runs the batch conversion against that row
  * **Then** the output record contains the original timestamp, source timezone, and target timezone values, plus a converted date, time, and target timezone identifier "Asia/Kolkata"

---

### Requirement Validation: REQ-008 (Validate timestamp is parseable)

#### Test Case: ATP-008-A (Unparseable timestamp fails validation before conversion)
**Linked Requirement:** REQ-008
**Description:** Validates that a row's timestamp value is checked for parseability before any conversion attempt.
**Validation Condition:** A row has timestamp value "32/99/2026" (not a valid calendar date), with otherwise valid source and target timezones.
**Expected Result:** The row does not appear in the successful-output set; it appears in the invalid-row report.

* **User Scenario: SCN-008-A1**
  * **Given** a CSV row has timestamp value "32/99/2026", source timezone "America/Denver", and target timezone "UTC"
  * **When** the user runs the batch conversion against that row
  * **Then** the row is absent from the successful output and present in the invalid-row report

---

### Requirement Validation: REQ-009 (Validate source timezone is recognized)

#### Test Case: ATP-009-A (Unrecognized source timezone fails validation before conversion)
**Linked Requirement:** REQ-009
**Description:** Validates that a row's source timezone value is checked against the set of recognized timezone identifiers before conversion.
**Validation Condition:** A row has source timezone value "Mars/Colony_One" (not a recognized IANA identifier), with a parseable timestamp and a valid target timezone.
**Expected Result:** The row does not appear in the successful-output set; it appears in the invalid-row report.

* **User Scenario: SCN-009-A1**
  * **Given** a CSV row has timestamp "2026-02-10 09:00", source timezone "Mars/Colony_One", and target timezone "UTC"
  * **When** the user runs the batch conversion against that row
  * **Then** the row is absent from the successful output and present in the invalid-row report

---

### Requirement Validation: REQ-010 (Validate target timezone is recognized)

#### Test Case: ATP-010-A (Unrecognized target timezone fails validation before conversion)
**Linked Requirement:** REQ-010
**Description:** Validates that a row's target timezone value is checked against the set of recognized timezone identifiers before conversion.
**Validation Condition:** A row has target timezone value "Europe/Atlantis" (not a recognized IANA identifier), with a parseable timestamp and a valid source timezone.
**Expected Result:** The row does not appear in the successful-output set; it appears in the invalid-row report.

* **User Scenario: SCN-010-A1**
  * **Given** a CSV row has timestamp "2026-02-10 09:00", source timezone "UTC", and target timezone "Europe/Atlantis"
  * **When** the user runs the batch conversion against that row
  * **Then** the row is absent from the successful output and present in the invalid-row report

---

### Requirement Validation: REQ-011 (Continue processing after a row fails)

#### Test Case: ATP-011-A (Batch continues processing subsequent rows after an invalid row)
**Linked Requirement:** REQ-011
**Description:** Validates that when an individual row fails validation or conversion, the system continues processing the remaining rows rather than stopping the run.
**Validation Condition:** A CSV file has row 1 invalid (missing timestamp) and rows 2 and 3 valid.
**Expected Result:** The batch run completes (does not abort), and both rows 2 and 3 appear in the successful-output set.

* **User Scenario: SCN-011-A1**
  * **Given** a CSV file has row 1 with a missing timestamp value, row 2 with timestamp "2026-04-01 06:00" (source "UTC", target "UTC"), and row 3 with timestamp "2026-04-02 06:00" (source "UTC", target "UTC")
  * **When** the user runs the batch conversion against that file
  * **Then** the run completes and both row 2 and row 3 appear in the successful-output set

---

### Requirement Validation: REQ-012 (Invalid-row report includes row number)

#### Test Case: ATP-012-A (Invalid-row report entry references the correct row number)
**Linked Requirement:** REQ-012
**Description:** Validates that each invalid row's entry in the invalid-row report includes a reference to that row's row number.
**Validation Condition:** A CSV file has 3 valid rows followed by 1 invalid row (unparseable timestamp) as the 4th data row.
**Expected Result:** The invalid-row report contains exactly one entry, and that entry's row-number reference equals 4.

* **User Scenario: SCN-012-A1**
  * **Given** a CSV file has 3 valid data rows followed by a 4th data row with timestamp value "invalid-ts"
  * **When** the user runs the batch conversion against that file
  * **Then** the invalid-row report contains exactly one entry whose row number is 4

---

### Requirement Validation: REQ-013 (Report unparseable-timestamp reason)

#### Test Case: ATP-013-A (Invalid-row reason references the timestamp field for unparseable timestamps)
**Linked Requirement:** REQ-013
**Description:** Validates that when a row's timestamp cannot be parsed, the invalid-row report's reason for that row references the timestamp field (e.g., "unparseable timestamp").
**Validation Condition:** A row has timestamp value "not-a-real-date" with otherwise valid source and target timezones.
**Expected Result:** The invalid-row report entry for that row has a reason text that references the timestamp field (contains the word "timestamp").

* **User Scenario: SCN-013-A1**
  * **Given** a CSV row has timestamp value "not-a-real-date", source timezone "UTC", and target timezone "UTC"
  * **When** the user runs the batch conversion against that row
  * **Then** the invalid-row report entry for that row has a reason referencing the timestamp field

---

### Requirement Validation: REQ-014 (Report unrecognized-timezone reason with the offending value)

#### Test Case: ATP-014-A (Invalid-row reason references the timezone field and identifies the unrecognized value)
**Linked Requirement:** REQ-014
**Description:** Validates that when a row's source or target timezone is not recognized, the invalid-row report's reason references the timezone field and identifies the specific unrecognized value.
**Validation Condition:** A row has source timezone value "Not/ARealZone" with a parseable timestamp and a valid target timezone.
**Expected Result:** The invalid-row report entry for that row has a reason text that references the timezone field and contains the literal string "Not/ARealZone".

* **User Scenario: SCN-014-A1**
  * **Given** a CSV row has timestamp "2026-06-01 10:00", source timezone "Not/ARealZone", and target timezone "UTC"
  * **When** the user runs the batch conversion against that row
  * **Then** the invalid-row report entry for that row has a reason referencing the timezone field and containing the value "Not/ARealZone"

---

### Requirement Validation: REQ-015 (Report missing-field reason identifying which field)

#### Test Case: ATP-015-A (Invalid-row reason identifies the specific missing field)
**Linked Requirement:** REQ-015
**Description:** Validates that when a row is missing its timestamp, source timezone, or target timezone value, the invalid-row report's reason indicates which specific field is missing.
**Validation Condition:** A row has an empty value for the source timezone column, with a parseable timestamp and a valid target timezone present.
**Expected Result:** The invalid-row report entry for that row has a reason text that identifies the source timezone field as missing (contains a reference to the source timezone field).

* **User Scenario: SCN-015-A1**
  * **Given** a CSV row has timestamp "2026-06-15 08:00", an empty source timezone value, and target timezone "UTC"
  * **When** the user runs the batch conversion against that row
  * **Then** the invalid-row report entry for that row has a reason identifying the source timezone field as missing

---

### Requirement Validation: REQ-016 (Distinct file-level error when file cannot be opened/read)

#### Test Case: ATP-016-A (Unreadable input file produces a distinct file-level error)
**Linked Requirement:** REQ-016
**Description:** Validates that when the input CSV file cannot be opened or read, the system reports a file-level error distinct from per-row invalid-row entries, identifying the file problem.
**Validation Condition:** The batch conversion is invoked against a file path that does not exist on disk.
**Expected Result:** A file-level error is reported identifying that the file could not be opened/read, and the invalid-row report is empty (no per-row entries are produced).

* **User Scenario: SCN-016-A1**
  * **Given** a file path "missing-input.csv" does not exist on the file system
  * **When** the user runs the batch conversion against that file path
  * **Then** the system reports a file-level error identifying that the file could not be opened, and no invalid-row entries are produced

---

### Requirement Validation: REQ-017 (Distinct file-level error when required columns cannot be identified)

#### Test Case: ATP-017-A (Unidentifiable header columns produce a distinct file-level error)
**Linked Requirement:** REQ-017
**Description:** Validates that when the required columns cannot be identified from the CSV's header row, the system reports a file-level error distinct from per-row invalid-row entries.
**Validation Condition:** A CSV file's header row is `col_a,col_b,col_c` (none of the recognized timestamp/source/target header names), with one data row present.
**Expected Result:** A file-level error is reported identifying that the required columns could not be identified, and the invalid-row report is empty.

* **User Scenario: SCN-017-A1**
  * **Given** a CSV file has header row `col_a,col_b,col_c` with no columns matching the recognized timestamp, source timezone, or target timezone names, and one data row
  * **When** the user runs the batch conversion against that file
  * **Then** the system reports a file-level error indicating the required columns could not be identified, and no invalid-row entries are produced

---

### Requirement Validation: REQ-018 (No row processing after a file-level error)

#### Test Case: ATP-018-A (No rows are processed once a file-level error occurs)
**Linked Requirement:** REQ-018
**Description:** Validates that when a file-level error occurs (per REQ-016 or REQ-017), the system does not attempt to process any row of that file.
**Validation Condition:** A CSV file has an unidentifiable header row (per REQ-017) followed by 5 otherwise-valid-looking data rows.
**Expected Result:** Both the successful-output set and the invalid-row report are empty; no rows from the file appear in either.

* **User Scenario: SCN-018-A1**
  * **Given** a CSV file has an unidentifiable header row and 5 data rows that would otherwise be valid under a recognized header
  * **When** the user runs the batch conversion against that file
  * **Then** the successful-output set and the invalid-row report are both empty, confirming no row was processed

---

### Requirement Validation: REQ-019 (Successful output available in structured machine-readable form)

#### Test Case: ATP-019-A (Successful rows are available as structured, machine-readable output)
**Linked Requirement:** REQ-019
**Description:** Validates that the set of successfully converted rows is made available as output in a structured, machine-readable form (e.g., CSV).
**Validation Condition:** A batch run with 2 valid rows completes and its successful output is retrieved.
**Expected Result:** The successful output is a structured, machine-parseable resource (e.g., a CSV with a header row and 2 data rows) containing exactly 2 records, each parseable into discrete fields.

* **User Scenario: SCN-019-A1**
  * **Given** a CSV file has 2 valid data rows
  * **When** the user runs the batch conversion against that file
  * **Then** the successful output is produced as a structured, machine-readable resource containing exactly 2 parseable records

---

### Requirement Validation: REQ-020 (Invalid-row report kept separate from successful output)

#### Test Case: ATP-020-A (Invalid-row report is a distinct output from the successful conversion output)
**Linked Requirement:** REQ-020
**Description:** Validates that the invalid-row report is made available as output separate from the successful conversion output, such that the two are not intermixed in a single output structure.
**Validation Condition:** A CSV file has 2 valid rows and 1 invalid row (missing target timezone).
**Expected Result:** The successful output contains exactly 2 records and none of them is the invalid row; the invalid-row report contains exactly 1 entry and is a structure distinct from the successful output.

* **User Scenario: SCN-020-A1**
  * **Given** a CSV file has 2 valid data rows and 1 data row with a missing target timezone value
  * **When** the user runs the batch conversion against that file
  * **Then** the successful output contains exactly 2 records and the invalid-row report contains exactly 1 entry, held as two separate output structures

---

### Requirement Validation: REQ-021 (Run summary reports total/succeeded/failed counts)

#### Test Case: ATP-021-A (Run summary reports total, succeeded, and failed row counts)
**Linked Requirement:** REQ-021
**Description:** Validates that upon completion of a batch run, the system reports the total number of rows processed, the number succeeded, and the number failed.
**Validation Condition:** A CSV file has 5 data rows: 3 valid and 2 invalid (unrecognized timezones).
**Expected Result:** The run summary reports total = 5, succeeded = 3, failed = 2.

* **User Scenario: SCN-021-A1**
  * **Given** a CSV file has 5 data rows, of which 3 are valid and 2 have unrecognized timezone values
  * **When** the user runs the batch conversion against that file
  * **Then** the run summary reports a total of 5 rows, 3 succeeded, and 2 failed

---

### Requirement Validation: REQ-022 (Zero failed count displayed as zero, not omitted)

#### Test Case: ATP-022-A (All-succeeded run displays a failed count of zero rather than omitting it)
**Linked Requirement:** REQ-022
**Description:** Validates that when a batch run's failed-row count is zero, the system displays that count as the numeral zero rather than omitting it from the summary.
**Validation Condition:** A CSV file has 4 valid data rows and zero invalid rows.
**Expected Result:** The run summary explicitly displays a failed count field with the value 0 (the field is present in the summary output, not absent).

* **User Scenario: SCN-022-A1**
  * **Given** a CSV file has 4 valid data rows and no invalid rows
  * **When** the user runs the batch conversion against that file
  * **Then** the run summary displays a failed count field with the explicit value 0

---

### Requirement Validation: REQ-023 (Header-only file completes cleanly with empty outputs)

#### Test Case: ATP-023-A (Header-only CSV completes without error, producing empty outputs and a no-rows message)
**Linked Requirement:** REQ-023
**Description:** Validates that a CSV file containing a header row and no data rows completes the run producing an empty successful-output set, an empty invalid-row report, and a message indicating no rows were processed, without raising an error.
**Validation Condition:** A CSV file contains only the header row `timestamp,source_tz,target_tz` and zero data rows.
**Expected Result:** The run completes without raising an error; the successful-output set is empty, the invalid-row report is empty, and a message stating that no rows were processed is shown.

* **User Scenario: SCN-023-A1**
  * **Given** a CSV file contains only the header row `timestamp,source_tz,target_tz` and no data rows
  * **When** the user runs the batch conversion against that file
  * **Then** the run completes without error, producing an empty successful-output set, an empty invalid-row report, and a message indicating that no rows were processed

---

### Requirement Validation: REQ-024 (No explicit offset: source timezone is authoritative)

#### Test Case: ATP-024-A (Timestamp without explicit UTC offset is interpreted as local to the source timezone)
**Linked Requirement:** REQ-024
**Description:** Validates that when a row's timestamp string contains no explicit UTC offset, the system interprets that timestamp as local to the row's stated source timezone.
**Validation Condition:** A row has timestamp "2026-01-15 09:00" (no offset), source timezone "America/Los_Angeles" (PST, UTC-8 in January), target timezone "UTC".
**Expected Result:** The converted output timestamp equals "2026-01-15 17:00" in "UTC", confirming the naive timestamp was treated as 09:00 Los Angeles local time.

* **User Scenario: SCN-024-A1**
  * **Given** a CSV row has timestamp "2026-01-15 09:00" with no embedded UTC offset, source timezone "America/Los_Angeles", and target timezone "UTC"
  * **When** the user runs the batch conversion against that row
  * **Then** the converted output timestamp is "2026-01-15 17:00" in "UTC"

---

### Requirement Validation: REQ-025 (Converted output includes the correct resulting date across a date boundary)

#### Test Case: ATP-025-A (Conversion crossing a date boundary reports the correct resulting calendar date)
**Linked Requirement:** REQ-025
**Description:** Validates that for a conversion crossing a calendar date boundary, the resulting calendar date (not only the time) is included in the converted output.
**Validation Condition:** A row has timestamp "2026-03-10 23:00", source timezone "America/New_York", target timezone "Europe/London" (5 hours ahead, no DST offset difference in this window).
**Expected Result:** The converted output's date equals "2026-03-11" (the next calendar day) with time "04:00", confirming the date-boundary crossing is captured.

* **User Scenario: SCN-025-A1**
  * **Given** a CSV row has timestamp "2026-03-10 23:00", source timezone "America/New_York", and target timezone "Europe/London"
  * **When** the user runs the batch conversion against that row
  * **Then** the converted output reports date "2026-03-11" and time "04:00"

---

### Requirement Validation: REQ-026 (DST transitions resolved using standard rules, not treated as invalid)

#### Test Case: ATP-026-A (Spring-forward gap timestamp is resolved, not rejected)
**Linked Requirement:** REQ-026
**Description:** Validates that a row whose timestamp falls within a DST "spring forward" gap in the source timezone is resolved using standard DST resolution rules rather than being reported as invalid.
**Validation Condition:** A row has timestamp "2026-03-08 02:30" (within the US spring-forward gap on that date), source timezone "America/New_York", target timezone "UTC".
**Expected Result:** The row appears in the successful-output set (not the invalid-row report), with a converted timestamp produced using standard DST gap-resolution rules.

* **User Scenario: SCN-026-A1**
  * **Given** a CSV row has timestamp "2026-03-08 02:30" (a nonexistent local time in the US spring-forward transition), source timezone "America/New_York", and target timezone "UTC"
  * **When** the user runs the batch conversion against that row
  * **Then** the row appears in the successful-output set with a resolved converted timestamp, and does not appear in the invalid-row report

#### Test Case: ATP-026-B (Fall-back ambiguous-hour timestamp is resolved, not rejected)
**Linked Requirement:** REQ-026
**Description:** Validates that a row whose timestamp falls within a DST "fall back" ambiguous hour in the source timezone is resolved using standard DST resolution rules rather than being reported as invalid.
**Validation Condition:** A row has timestamp "2026-11-01 01:30" (within the US fall-back ambiguous hour on that date), source timezone "America/New_York", target timezone "UTC".
**Expected Result:** The row appears in the successful-output set (not the invalid-row report), with a converted timestamp produced using standard DST ambiguous-hour resolution rules.

* **User Scenario: SCN-026-B1**
  * **Given** a CSV row has timestamp "2026-11-01 01:30" (an ambiguous local time in the US fall-back transition), source timezone "America/New_York", and target timezone "UTC"
  * **When** the user runs the batch conversion against that row
  * **Then** the row appears in the successful-output set with a resolved converted timestamp, and does not appear in the invalid-row report

---

### Requirement Validation: REQ-027 (Identical source and target timezone still produces an unchanged result)

#### Test Case: ATP-027-A (Identical source and target timezone yields a successful, unchanged conversion)
**Linked Requirement:** REQ-027
**Description:** Validates that when a row's source and target timezone values are identical, the system still produces a converted result with the timestamp unchanged, rather than reporting it as invalid.
**Validation Condition:** A row has timestamp "2026-05-20 14:00", source timezone "Asia/Singapore", target timezone "Asia/Singapore" (identical).
**Expected Result:** The row appears in the successful-output set with a converted timestamp of "2026-05-20 14:00" in "Asia/Singapore", unchanged from the input.

* **User Scenario: SCN-027-A1**
  * **Given** a CSV row has timestamp "2026-05-20 14:00", source timezone "Asia/Singapore", and target timezone "Asia/Singapore"
  * **When** the user runs the batch conversion against that row
  * **Then** the row appears in the successful-output set with converted timestamp "2026-05-20 14:00" in "Asia/Singapore", unchanged from the input

---

### Requirement Validation: REQ-028 (Entirely blank rows are skipped, not counted)

#### Test Case: ATP-028-A (Entirely blank row is skipped and counted in neither succeeded nor failed totals)
**Linked Requirement:** REQ-028
**Description:** Validates that a row containing no data in any column is skipped without being counted as either a successful or an invalid row.
**Validation Condition:** A CSV file has 2 valid data rows and 1 completely blank line (no characters between the row's delimiters) positioned between them.
**Expected Result:** The successful-output set contains exactly 2 records, the invalid-row report contains 0 entries for the blank line, and the run summary's total-processed count equals 2 (the blank line is excluded from the total).

* **User Scenario: SCN-028-A1**
  * **Given** a CSV file has 2 valid data rows with one entirely blank line positioned between them
  * **When** the user runs the batch conversion against that file
  * **Then** the successful-output set contains exactly 2 records, the invalid-row report contains no entry for the blank line, and the run summary's total count is 2

---

### Requirement Validation: REQ-029 (Unwritable output destination is reported without discarding computed results)

#### Test Case: ATP-029-A (Write failure to the output destination is reported without discarding already-computed conversions)
**Linked Requirement:** REQ-029
**Description:** Validates that when the output destination cannot be written to, the system informs the user that results could not be saved and does not discard the conversion results it had already computed for that run.
**Validation Condition:** A batch run with 3 valid rows completes conversion in memory, then the configured output destination is made unwritable (e.g., simulated permission denial) before the write step.
**Expected Result:** The user is shown a message stating that results could not be saved, and the 3 already-computed conversion results remain available/retained (not discarded) for a subsequent retry or alternate save.

* **User Scenario: SCN-029-A1**
  * **Given** a batch run has computed 3 successful conversion results in memory and the configured output destination is unwritable due to simulated permission denial
  * **When** the system attempts to write the results to that output destination
  * **Then** the system displays a message stating that results could not be saved, and the 3 already-computed conversion results remain retained rather than discarded

---

## Non-Functional Requirements

### Requirement Validation: REQ-NF-001 (100% of valid rows return a converted result regardless of invalid-row proportion)

#### Test Case: ATP-NF-001-A (All valid rows in a mixed batch produce converted results)
**Linked Requirement:** REQ-NF-001
**Description:** Validates that for a batch containing a mix of valid and invalid rows, 100% of the valid rows return a converted result, regardless of the proportion of invalid rows.
**Validation Condition:** A CSV file has 20 data rows: 4 valid and 16 invalid (each with an unrecognized timezone).
**Expected Result:** The successful-output set contains exactly 4 records, one for each of the 4 valid rows (`4/4` = 100% of valid rows converted).

* **User Scenario: SCN-NF-001-A1**
  * **Given** a CSV file has 20 data rows, of which 4 are valid and 16 have unrecognized timezone values
  * **When** the user runs the batch conversion against that file
  * **Then** the successful-output set contains exactly 4 records, corresponding to 100% of the valid rows

---

### Requirement Validation: REQ-NF-002 (100% of invalid rows appear as distinct, uniquely identified report entries)

#### Test Case: ATP-NF-002-A (Every invalid row in a batch appears as a distinct report entry with a unique row number)
**Linked Requirement:** REQ-NF-002
**Description:** Validates that 100% of the rows reported as invalid each appear as a distinct entry in the invalid-row report, identifiable by a unique row number and a specific failure reason, with zero entries omitted or merged.
**Validation Condition:** A CSV file has 5 invalid data rows, each with a different failure cause (unparseable timestamp, unrecognized source timezone, unrecognized target timezone, missing timestamp, missing source timezone).
**Expected Result:** The invalid-row report contains exactly 5 entries, each with a distinct row number (matching its position in the file) and a reason text specific to its failure cause; no two entries share a row number.

* **User Scenario: SCN-NF-002-A1**
  * **Given** a CSV file has 5 invalid data rows, each failing for a different specific reason (unparseable timestamp, unrecognized source timezone, unrecognized target timezone, missing timestamp, missing source timezone)
  * **When** the user runs the batch conversion against that file
  * **Then** the invalid-row report contains exactly 5 distinct entries, each with a unique row number and a reason specific to its own failure cause

---

### Requirement Validation: REQ-NF-003 (Summary's two counts alone classify the run as fully succeeded, fully failed, or partial)

#### Test Case: ATP-NF-003-A (Succeeded and failed counts alone allow classifying the run outcome)
**Linked Requirement:** REQ-NF-003
**Description:** Validates that the run summary's succeeded and failed counts, viewed together without cross-referencing row-level output, are sufficient to classify a run as fully succeeded, fully failed, or partially succeeded.
**Validation Condition:** Three separate batch runs are executed: Run A (5 valid, 0 invalid rows), Run B (0 valid, 5 invalid rows), Run C (3 valid, 2 invalid rows).
**Expected Result:** Run A's summary shows succeeded=5, failed=0 (classifiable as fully succeeded); Run B's summary shows succeeded=0, failed=5 (classifiable as fully failed); Run C's summary shows succeeded=3, failed=2, both > 0 (classifiable as partially succeeded) — each classification derivable from the two displayed counts alone.

* **User Scenario: SCN-NF-003-A1**
  * **Given** a CSV file with 5 valid rows and 0 invalid rows is prepared as "Run A"
  * **When** the user runs the batch conversion against "Run A" and views the run summary
  * **Then** the summary shows succeeded=5 and failed=0, which alone classify the run as fully succeeded

* **User Scenario: SCN-NF-003-A2**
  * **Given** a CSV file with 0 valid rows and 5 invalid rows is prepared as "Run B"
  * **When** the user runs the batch conversion against "Run B" and views the run summary
  * **Then** the summary shows succeeded=0 and failed=5, which alone classify the run as fully failed

* **User Scenario: SCN-NF-003-A3**
  * **Given** a CSV file with 3 valid rows and 2 invalid rows is prepared as "Run C"
  * **When** the user runs the batch conversion against "Run C" and views the run summary
  * **Then** the summary shows succeeded=3 and failed=2, both greater than zero, which alone classify the run as partially succeeded

---

### Requirement Validation: REQ-NF-004 (Minute-accurate conversion within the -10y/+2y window, including DST)

#### Test Case: ATP-NF-004-A (Converted timestamp within the supported date window is accurate to the minute including DST)
**Linked Requirement:** REQ-NF-004
**Description:** Validates that for a row whose source timestamp falls between 10 years before and 2 years after a fixed reference date, the converted timestamp is accurate to the minute, including correct DST application.
**Validation Condition:** A row has timestamp "2020-06-15 14:37" (within 10 years before the fixed reference date 2026-09-21), source timezone "America/Chicago" (CDT, UTC-5 in June), target timezone "UTC".
**Expected Result:** The converted output timestamp equals exactly "2020-06-15 19:37" in "UTC" (minute-accurate, reflecting the -5 hour CDT offset).

* **User Scenario: SCN-NF-004-A1**
  * **Given** a CSV row has timestamp "2020-06-15 14:37", source timezone "America/Chicago", and target timezone "UTC"
  * **When** the user runs the batch conversion against that row
  * **Then** the converted output timestamp is exactly "2020-06-15 19:37" in "UTC"

---

### Requirement Validation: REQ-NF-005 (10,000-row batch completes in a single run)

#### Test Case: ATP-NF-005-A (A 10,000-row CSV file is processed in a single run without splitting)
**Linked Requirement:** REQ-NF-005
**Description:** Validates that the system completes a single batch run of 10,000 rows without requiring the user to split the input CSV file into smaller files.
**Validation Condition:** A CSV file containing exactly 10,000 valid data rows is submitted as a single file in one batch-conversion invocation.
**Expected Result:** The run completes and the successful-output set contains exactly 10,000 records, all produced from the single submitted file with no file-splitting step performed by the user.

* **User Scenario: SCN-NF-005-A1**
  * **Given** a single CSV file contains exactly 10,000 valid data rows
  * **When** the user runs the batch conversion against that single file in one invocation
  * **Then** the run completes and the successful-output set contains exactly 10,000 records, with no file-splitting performed

---

## Coverage Summary

| Metric | Count |
|--------|-------|
| Total Requirements | 34 |
| Total Test Cases (ATP) | 35 |
| Total Scenarios (SCN) | 37 |
| REQ → ATP Coverage | 100% |
| ATP → SCN Coverage | 100% |

**Validation Status**: Full Coverage — every REQ-NNN (REQ-001–REQ-029) and REQ-NF-NNN (REQ-NF-001–REQ-NF-005) ID in `requirements.md` has at least one ATP, and every ATP has at least one SCN. Verified by manual cross-check against all 34 requirement IDs, since no automated `validate-requirement-coverage` script/template was found in this repository checkout (searched for `acceptance-plan-template.md` and `validate-requirement-coverage`; neither exists). No coverage gaps identified. The three items flagged `[NEEDS CLARIFICATION]` in requirements.md (explicit-offset inconsistency reporting, upper file-size ceiling, exact header column naming) have no assigned REQ ID and are intentionally excluded from this plan.

**Generated**: 2026-09-21
**Validated by**: manual cross-reference (no `validate-requirement-coverage` script or acceptance-plan template present in this repository checkout)
