# Feature Specification: Batch Timezone Conversion

**Feature Branch**: `003-batch-timezone-conversion`

**Created**: September 21, 2026

**Status**: Draft

**Input**: User description: "Add batch timezone conversion: read timestamps and source/target timezones from a CSV file, produce converted timestamps, and report invalid rows individually without stopping the whole batch."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Convert a Batch of Timestamps from a CSV File (Priority: P1)

As a user who needs to convert many timestamps at once (e.g., scheduling data, logs, event exports), I want to provide a CSV file listing timestamps with their source and target timezones and receive a corresponding CSV of converted timestamps, so I don't have to convert each entry manually one at a time.

**Why this priority**: This is the core capability of the feature. Without batch input and output, there is no value over converting timestamps individually using the existing timezone utility.

**Independent Test**: Can be fully tested by providing a CSV file containing several valid rows (timestamp, source timezone, target timezone), running the batch conversion, and verifying the output contains the correctly converted timestamp for each row.

**Acceptance Scenarios**:

1. **Given** a CSV file with a header row and multiple valid data rows (each with a timestamp, source timezone, and target timezone), **When** I run the batch conversion, **Then** I receive output containing, for each row, the original input and the correctly converted timestamp in the target timezone.
2. **Given** a CSV file with a single valid row, **When** I run the batch conversion, **Then** I receive output with exactly one converted result.
3. **Given** a CSV file that is empty (header only, no data rows), **When** I run the batch conversion, **Then** I receive an empty result set and a message indicating no rows were processed, without an error.

---

### User Story 2 - Continue Processing Despite Invalid Rows (Priority: P1)

As a user submitting a batch of timestamps from a potentially messy real-world data source, I want invalid rows (e.g., malformed timestamp, unrecognized timezone, missing field) to be reported individually rather than aborting the entire batch, so that I still get results for all the valid rows in the same run.

**Why this priority**: Batch processing is only useful in practice if a single bad row doesn't discard the value of an entire file. This is as critical as the core conversion itself and is called out explicitly in the feature request.

**Independent Test**: Can be tested by providing a CSV file that mixes valid and invalid rows, running the batch conversion, and verifying that valid rows produce converted results while invalid rows appear in a separate error report with a specific reason, and that the run completes successfully overall.

**Acceptance Scenarios**:

1. **Given** a CSV file containing a mix of valid and invalid rows, **When** I run the batch conversion, **Then** all valid rows are converted and included in the successful output, and all invalid rows are listed individually with a reason for failure.
2. **Given** a row with an unparseable timestamp, **When** the batch is processed, **Then** that row is reported invalid with a reason referencing the timestamp field, and processing continues to subsequent rows.
3. **Given** a row with an unrecognized source or target timezone identifier, **When** the batch is processed, **Then** that row is reported invalid with a reason referencing the timezone field, and processing continues to subsequent rows.
4. **Given** a row missing a required field (timestamp, source timezone, or target timezone), **When** the batch is processed, **Then** that row is reported invalid with a reason indicating which field is missing, and processing continues to subsequent rows.
5. **Given** a CSV file where every row is invalid, **When** I run the batch conversion, **Then** the run completes (it does not abort or crash), the successful output is empty, and every row appears in the invalid-row report with its reason.

---

### User Story 3 - Review a Summary of the Batch Run (Priority: P2)

As a user who just ran a batch conversion, I want a summary of how many rows succeeded and how many failed, so I can quickly judge whether the input file needs to be cleaned up and re-run.

**Why this priority**: This improves usability and trust in the results but is not required for the core conversion or per-row error reporting to deliver value; users could otherwise count rows themselves.

**Independent Test**: Can be tested by running a batch with a known number of valid and invalid rows and verifying the reported counts match.

**Acceptance Scenarios**:

1. **Given** a completed batch run, **When** I view the results, **Then** I see a summary showing the total number of rows processed, the number that succeeded, and the number that failed.
2. **Given** a batch run where all rows succeeded, **When** I view the summary, **Then** the failed count is shown as zero rather than being omitted.

---

### Edge Cases

- What happens when the CSV file is missing entirely or cannot be opened? System MUST report a clear error identifying the file problem and MUST NOT attempt to process rows.
- What happens when the CSV file has an unexpected or missing header (e.g., columns out of order, misspelled column names)? System MUST detect that the expected columns cannot be identified and report a clear file-level error rather than silently misreading data.
- What happens when a row has extra, unexpected columns? System MUST ignore extra columns and process the row using the recognized columns.
- What happens when a timestamp has no explicit time (date only) or no explicit UTC offset embedded in it, and a source timezone is also given? System MUST treat the source timezone column as authoritative for interpreting the timestamp.
- What happens when a row's timestamp, once converted, crosses a date boundary (e.g., 11pm converts to 2am the next day)? System MUST include the correct resulting date, not just the time, in the converted output.
- What happens during a daylight saving time transition (e.g., a timestamp that falls in a "spring forward" gap or a "fall back" ambiguous hour) in either the source or target timezone? System MUST apply standard, documented DST resolution rules consistently and MUST NOT fail the row solely because of DST ambiguity.
- What happens when the source and target timezone for a row are the same? System MUST still produce a converted result (the timestamp unchanged) rather than treating this as an error.
- What happens when the CSV file is very large (e.g., tens of thousands of rows)? System MUST process the full file and report results for all rows without requiring the user to split the file manually.
- What happens when the output destination cannot be written (e.g., no permission, disk full)? System MUST inform the user that results could not be saved and MUST NOT silently discard already-computed conversions.

## Requirements *(mandatory)*

### Functional Requirements

**Input**
- **FR-001**: System MUST accept a CSV file as input containing, at minimum, one row per timestamp to convert with columns for the timestamp value, the source timezone, and the target timezone.
- **FR-002**: System MUST identify the required columns via a header row rather than requiring a fixed column order.
- **FR-003**: System MUST accept source and target timezones expressed as IANA timezone identifiers (e.g., "America/New_York"), consistent with the identifiers supported by the existing timezone utility.
- **FR-004**: System MUST ignore columns in the CSV file that are not required for conversion.

**Conversion**
- **FR-005**: System MUST convert each valid row's timestamp from its specified source timezone to its specified target timezone, correctly accounting for daylight saving time rules in effect for that timestamp.
- **FR-006**: System MUST process each row independently, such that the outcome of one row (success or failure) has no effect on the processing of any other row.
- **FR-007**: System MUST produce, for each successfully converted row, an output record containing the original input values and the resulting converted timestamp (including date, time, and target timezone identifier).

**Error Handling**
- **FR-008**: System MUST validate each row's timestamp format, source timezone identifier, and target timezone identifier before attempting conversion.
- **FR-009**: System MUST NOT stop or abort processing of the remaining batch when an individual row is invalid.
- **FR-010**: System MUST report each invalid row individually, including a reference to which row it was (e.g., row number) and a specific, human-readable reason for the failure (e.g., "unparseable timestamp", "unrecognized timezone: X", "missing field: Y").
- **FR-011**: System MUST report a file-level error (distinct from per-row errors) and MUST NOT attempt row processing when the input file itself cannot be read or its required columns cannot be identified.

**Output**
- **FR-012**: System MUST make the successfully converted rows available as output in a structured, machine-readable form (e.g., CSV) suitable for further use.
- **FR-013**: System MUST make the invalid-row report available as output separate from the successful conversion output, so the two are not intermixed.
- **FR-014**: System MUST provide a summary of the batch run showing the total number of rows processed, the number succeeded, and the number failed.

### Key Entities

- **Batch Conversion Request**: A single invocation of the batch process against one input CSV file. Attributes: input file reference, total row count, success count, failure count.
- **Conversion Row**: One data row from the input CSV. Attributes: row number, raw timestamp value, source timezone identifier, target timezone identifier.
- **Conversion Result**: The outcome of processing one Conversion Row that was valid. Attributes: original row values, converted timestamp (date, time, target timezone).
- **Invalid Row Report Entry**: The outcome of processing one Conversion Row that failed validation or conversion. Attributes: row number, original raw values, failure reason.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can submit a CSV file of timestamps and receive converted results without manually converting any row by hand.
- **SC-002**: A batch containing a mix of valid and invalid rows always completes and returns results for 100% of the valid rows, regardless of how many rows are invalid.
- **SC-003**: Every invalid row is identifiable by row number with a specific reason, with no invalid row silently dropped or merged into another row's result.
- **SC-004**: Users can determine, within a single glance at the run summary, whether their batch fully succeeded, partially succeeded, or fully failed.
- **SC-005**: Converted timestamps are accurate to the minute, including correct handling of daylight saving time, for any date in the past 10 years or future 2 years, consistent with the existing timezone utility's conversion accuracy.
- **SC-006**: A batch of 10,000 rows completes in a single run without requiring the user to manually split the input file.

## Assumptions

- The CSV input format uses a header row identifying at least a timestamp column, a source timezone column, and a target timezone column; exact column names are a planning-phase detail but MUST be documented for users.
- Timestamps in the CSV are naive/local to the stated source timezone (i.e., the source timezone column is authoritative for interpretation) unless the timestamp string itself includes explicit UTC offset information, in which case the explicit offset takes precedence and the source timezone is used only to validate/report inconsistency.
- This feature builds on the timezone resolution and conversion logic already established by the existing timezone utility (see `specs/001-timezone-utility/`); it does not introduce a new timezone data source.
- Output is written in the same general environment the existing timezone utility runs in (e.g., CLI-invoked, file-based); the exact output file mechanism (path conventions, stdout vs. file) is a planning-phase detail.
- A "row" that is entirely blank (e.g., a stray blank line in the CSV) is skipped rather than counted as an invalid row, since it contains no data to evaluate.
- No maximum file size is specified beyond a generous, reasonable bound; very large files should still be processed without requiring users to pre-split them.
