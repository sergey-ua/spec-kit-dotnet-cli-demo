# Acceptance Test Plan: Report Incident

**Feature Branch**: `002-report-incident`
**Source Document**: `specs/002-report-incident/v-model/requirements.md`
**Generated**: 2026-09-21
**Status**: Draft

## Overview

This document pairs every requirement (`REQ-NNN`) in `requirements.md` with Test Cases (`ATP-NNN-X`) and BDD-style User Scenarios (`SCN-NNN-X#`), per the V-Model three-tier acceptance testing approach. Coverage target is 100%: every REQ has at least one ATP, and every ATP has at least one SCN.

Note: Per `requirements.md`'s "Requirements Flagged for Clarification" section, duplicate-submission handling was deliberately NOT formalized as a requirement (open `[NEEDS CLARIFICATION]` item) and therefore has no REQ ID. No test case is generated for it here, consistent with the instruction not to invent coverage for out-of-scope/clarification-pending capabilities.

---

## Functional Requirements

### Requirement Validation: REQ-001 (Report Incident action is invocable)

#### Test Case: ATP-001-A (Report Incident action is available and invocable)
**Linked Requirement:** REQ-001
**Description:** Validates that the application exposes a "Report Incident" action, labeled as such, that any user can invoke from the interaction surface.
**Validation Condition:** The interaction surface (e.g., command list, menu) lists an action labeled "Report Incident", and invoking it starts the incident-submission flow.
**Expected Result:** The application enters the incident-submission flow (e.g., prompts for a description) after the action is invoked; the action's label contains the exact text "Report Incident".

* **User Scenario: SCN-001-A1**
  * **Given** the TimezoneUtility application is running and idle at its main interaction surface
  * **When** the user invokes the "Report Incident" action
  * **Then** the application starts the incident-submission flow and prompts the user for a description

* **User Scenario: SCN-001-A2**
  * **Given** the TimezoneUtility application is running and idle at its main interaction surface
  * **When** the user requests the list of available actions/commands
  * **Then** the list includes an entry labeled "Report Incident"

---

### Requirement Validation: REQ-002 (Accept free-text description input)

#### Test Case: ATP-002-A (Free-text description is accepted as input)
**Linked Requirement:** REQ-002
**Description:** Validates that the system accepts a free-text description supplied by the user when the "Report Incident" action is invoked.
**Validation Condition:** A non-empty free-text string entered in response to the description prompt is accepted for submission (i.e., processing continues without an input-format rejection).
**Expected Result:** The submission proceeds to the recording step with the exact description text "Time displayed for Tokyo was off by one hour" carried through unmodified.

* **User Scenario: SCN-002-A1**
  * **Given** the user has invoked the "Report Incident" action and is at the description prompt
  * **When** the user enters the description "Time displayed for Tokyo was off by one hour"
  * **Then** the system accepts the input and proceeds to record the incident with that exact description text

---

### Requirement Validation: REQ-003 (Reject empty/missing description)

#### Test Case: ATP-003-A (Empty description is rejected)
**Linked Requirement:** REQ-003
**Description:** Validates that submission is rejected when the description field is an empty string.
**Validation Condition:** Submitting with description equal to the empty string `""` results in the report not being recorded.
**Expected Result:** No new incident record is created (the incident store's record count is unchanged from before the attempt).

* **User Scenario: SCN-003-A1**
  * **Given** the user has invoked the "Report Incident" action and the current incident store contains a known count of N records
  * **When** the user submits an incident report with description equal to the empty string
  * **Then** the submission is rejected and the incident store still contains exactly N records

#### Test Case: ATP-003-B (Missing description is rejected)
**Linked Requirement:** REQ-003
**Description:** Validates that submission is rejected when the description field is entirely omitted (e.g., user submits with no input provided at the prompt, distinct from an explicit empty string).
**Validation Condition:** Submitting with the description field absent/omitted results in the report not being recorded.
**Expected Result:** No new incident record is created (the incident store's record count is unchanged from before the attempt).

* **User Scenario: SCN-003-B1**
  * **Given** the user has invoked the "Report Incident" action and the current incident store contains a known count of N records
  * **When** the user attempts to submit the incident report without providing any value for the description field
  * **Then** the submission is rejected and the incident store still contains exactly N records

---

### Requirement Validation: REQ-004 (Inform user description is required)

#### Test Case: ATP-004-A (Required-description message is displayed on empty/missing description)
**Linked Requirement:** REQ-004
**Description:** Validates that when a submission is rejected per REQ-003, the user is shown a message stating a description is required.
**Validation Condition:** After a rejected submission due to empty/missing description, a message containing the text "description is required" (or an equivalent explicit statement that a description must be provided) is displayed to the user.
**Expected Result:** The displayed message text explicitly states that a description is required for the report to be submitted.

* **User Scenario: SCN-004-A1**
  * **Given** the user has invoked the "Report Incident" action
  * **When** the user submits an incident report with an empty description
  * **Then** the system displays a message to the user stating that a description is required

---

### Requirement Validation: REQ-005 (Record submission timestamp)

#### Test Case: ATP-005-A (Timestamp is recorded for every submitted report)
**Linked Requirement:** REQ-005
**Description:** Validates that a timestamp of submission time is stored with every successfully submitted incident report.
**Validation Condition:** After a successful submission, the recorded incident has a non-null timestamp field populated with a valid timestamp value.
**Expected Result:** The retrieved incident record includes a timestamp field that is present and parseable as a valid date-time value.

* **User Scenario: SCN-005-A1**
  * **Given** the user has invoked the "Report Incident" action and provided the description "Clock offset in dashboard"
  * **When** the user submits the incident report
  * **Then** the recorded incident includes a non-null, parseable timestamp field representing the moment of submission

---

### Requirement Validation: REQ-006 (Record reporter context)

#### Test Case: ATP-006-A (Reporter context is recorded for every submitted report)
**Linked Requirement:** REQ-006
**Description:** Validates that available reporter context (e.g., session/user identifier and the command or view in use) is stored with every successfully submitted incident report.
**Validation Condition:** After a successful submission from a session with known identifier "session-42" while the "convert-time" command is active, the recorded incident's reporter-context field contains "session-42" and "convert-time".
**Expected Result:** The retrieved incident record's reporter-context field contains both the session identifier "session-42" and the command name "convert-time".

* **User Scenario: SCN-006-A1**
  * **Given** the user is operating under session identifier "session-42" while using the "convert-time" command, and invokes the "Report Incident" action
  * **When** the user submits an incident report with description "Conversion result looked wrong"
  * **Then** the recorded incident's reporter context includes the session identifier "session-42" and the command name "convert-time"

---

### Requirement Validation: REQ-007 (Assign unique identifier)

#### Test Case: ATP-007-A (Unique identifier is assigned at recording time)
**Linked Requirement:** REQ-007
**Description:** Validates that each recorded incident receives a unique identifier at the time it is recorded.
**Validation Condition:** Two separately submitted incident reports each receive a non-null identifier, and the two identifiers are not equal to each other.
**Expected Result:** Both recorded incidents have non-null identifier values, and `identifier(incident_1) != identifier(incident_2)`.

* **User Scenario: SCN-007-A1**
  * **Given** the incident store is available and the user submits an incident report with description "First report"
  * **When** the user then submits a second incident report with description "Second report"
  * **Then** both recorded incidents have non-null unique identifiers and the two identifiers are not equal to each other

---

### Requirement Validation: REQ-008 (Record timestamp in UTC)

#### Test Case: ATP-008-A (Timestamp is recorded in UTC regardless of viewed timezone)
**Linked Requirement:** REQ-008
**Description:** Validates that the incident timestamp is recorded in UTC even when the user is viewing/converting a different timezone elsewhere in the application.
**Validation Condition:** While the application's active display/conversion context is set to a non-UTC timezone (e.g., "Asia/Tokyo"), the recorded incident's timestamp field is expressed in UTC (i.e., its UTC offset is `+00:00` or equivalent, and it is not adjusted for "Asia/Tokyo").
**Expected Result:** The recorded incident's timestamp field carries a UTC offset of `+00:00` (or "Z" designator), independent of the "Asia/Tokyo" display context active at submission time.

* **User Scenario: SCN-008-A1**
  * **Given** the user is currently viewing/converting times in the "Asia/Tokyo" timezone within the application, and invokes the "Report Incident" action
  * **When** the user submits an incident report with description "Timezone display looked off"
  * **Then** the recorded incident's timestamp field is expressed in UTC (offset `+00:00`), not in "Asia/Tokyo" local time

---

### Requirement Validation: REQ-009 (Confirmation message includes identifier)

#### Test Case: ATP-009-A (Successful submission displays confirmation with unique identifier)
**Linked Requirement:** REQ-009
**Description:** Validates that upon successful submission, the user is shown a confirmation message containing the incident's unique identifier.
**Validation Condition:** After a successful submission that was assigned identifier "INC-1001", the displayed confirmation message text contains the literal string "INC-1001".
**Expected Result:** The confirmation message displayed to the user contains the exact unique identifier assigned to the just-recorded incident.

* **User Scenario: SCN-009-A1**
  * **Given** the user has invoked the "Report Incident" action and entered a valid description
  * **When** the submission succeeds and is assigned unique identifier "INC-1001"
  * **Then** the system displays a confirmation message to the user that contains the identifier "INC-1001"

---

### Requirement Validation: REQ-010 (Error message on failed recording)

#### Test Case: ATP-010-A (Failed recording displays a not-saved error message)
**Linked Requirement:** REQ-010
**Description:** Validates that when recording fails, the user is shown an error message stating the report was not saved.
**Validation Condition:** When the persistence layer raises a write failure during submission, the displayed message text states that the report was not saved.
**Expected Result:** The displayed error message explicitly states that the incident report was not saved.

* **User Scenario: SCN-010-A1**
  * **Given** the user has invoked the "Report Incident" action, entered the description "Storage test case", and the persistence layer is configured to fail the next write
  * **When** the user submits the incident report
  * **Then** the system displays an error message to the user stating that the report was not saved

---

### Requirement Validation: REQ-011 (Retry without re-entering description)

#### Test Case: ATP-011-A (Retry after failure preserves the previously entered description)
**Linked Requirement:** REQ-011
**Description:** Validates that after a failed recording, the user can retry submission without having to re-enter the description already provided.
**Validation Condition:** After a failed submission of description "Preserve me on retry", the retry action re-submits using that same description text without prompting the user to re-type it, and the retry succeeds once the persistence layer is no longer failing.
**Expected Result:** The retried submission succeeds and the resulting recorded incident's description field equals "Preserve me on retry" exactly, with no re-entry prompt shown to the user between the failure and the retry.

* **User Scenario: SCN-011-A1**
  * **Given** the user submitted description "Preserve me on retry" and the submission failed because the persistence layer was configured to fail exactly one write
  * **When** the user invokes the retry action
  * **Then** the system resubmits the description "Preserve me on retry" without re-prompting for it, and the incident is successfully recorded with that exact description

---

### Requirement Validation: REQ-012 (Length-limit-exceeded message)

#### Test Case: ATP-012-A (Description exceeding maximum length is rejected with a length-exceeded message)
**Linked Requirement:** REQ-012
**Description:** Validates that when a submitted description exceeds the maximum length enforced per REQ-CN-001, the user is shown a message stating the length limit was exceeded, and the report is not recorded.
**Validation Condition:** Submitting a description whose length is exactly `max_length + 1` characters (where `max_length` is the enforced constant from REQ-CN-001) results in rejection and a length-exceeded message; the incident store's record count is unchanged.
**Expected Result:** The displayed message explicitly states that the length limit was exceeded, and no new incident record is created.

* **User Scenario: SCN-012-A1**
  * **Given** the system enforces a maximum description length of `max_length` characters per REQ-CN-001, and the incident store contains a known count of N records
  * **When** the user submits an incident report with a description exactly `max_length + 1` characters long
  * **Then** the system displays a message stating that the length limit was exceeded, and the incident store still contains exactly N records

---

### Requirement Validation: REQ-013 (Persistence across sessions)

#### Test Case: ATP-013-A (Recorded incident is retrievable after the recording session has ended)
**Linked Requirement:** REQ-013
**Description:** Validates that a recorded incident remains retrievable after the application session in which it was recorded has ended.
**Validation Condition:** An incident recorded in session 1 (assigned identifier "INC-2001") is retrievable by that identifier after session 1 has terminated and a new session 2 has started.
**Expected Result:** In session 2, a retrieval by identifier "INC-2001" returns a record with that identifier and the original description text intact.

* **User Scenario: SCN-013-A1**
  * **Given** the user recorded an incident with description "Persist me" in an application session, receiving identifier "INC-2001", and that session has since ended
  * **When** a new application session retrieves the incident with identifier "INC-2001"
  * **Then** the retrieval returns a record with identifier "INC-2001" and description "Persist me"

---

### Requirement Validation: REQ-014 (Retrieve incidents ordered by timestamp)

#### Test Case: ATP-014-A (Retrieval returns full incident fields ordered earliest-first)
**Linked Requirement:** REQ-014
**Description:** Validates that the system provides a retrieval mechanism returning previously recorded incidents with description, timestamp, reporter context, and unique identifier, ordered earliest-submitted first.
**Validation Condition:** Given three incidents recorded in strict succession with identifiers "INC-A", "INC-B", "INC-C" (in that submission order), a retrieval-all call returns them in the exact order INC-A, INC-B, INC-C, and each returned record includes non-null description, timestamp, reporter-context, and identifier fields.
**Expected Result:** The retrieval result is the ordered list `[INC-A, INC-B, INC-C]`, and each element has all four fields (description, timestamp, reporter context, identifier) populated.

* **User Scenario: SCN-014-A1**
  * **Given** three incidents have been recorded in strict succession, in order, with identifiers "INC-A", "INC-B", "INC-C"
  * **When** the user retrieves the list of all previously recorded incidents
  * **Then** the returned list is ordered exactly as `[INC-A, INC-B, INC-C]`, with each entry containing a non-null description, timestamp, reporter context, and identifier

---

### Requirement Validation: REQ-015 (Unavailable reporter context marked unknown)

#### Test Case: ATP-015-A (Submission succeeds with reporter context marked unknown when unavailable)
**Linked Requirement:** REQ-015
**Description:** Validates that when reporter context is partially or fully unavailable at submission time, the incident is still recorded, with the unavailable portion(s) marked as unknown rather than blocking submission.
**Validation Condition:** When the session identifier is unavailable at submission time (e.g., no active session context can be determined) but the command-in-use is available, the incident is still successfully recorded, with the session-identifier portion of reporter context set to a recognizable "unknown" marker and the command-in-use portion populated normally.
**Expected Result:** The recorded incident exists (submission is not blocked), its reporter-context session-identifier field equals the "unknown" marker, and its reporter-context command field is populated with the actual command in use.

* **User Scenario: SCN-015-A1**
  * **Given** the application has no determinable session identifier at submission time, but the command "convert-time" is active, and the user invokes the "Report Incident" action
  * **When** the user submits an incident report with description "Context partially missing"
  * **Then** the incident is successfully recorded with its reporter-context session-identifier field marked as unknown and its reporter-context command field set to "convert-time"

---

## Non-Functional Requirements

### Requirement Validation: REQ-NF-001 (Submission-to-confirmation latency under 30 seconds)

#### Test Case: ATP-NF-001-A (End-to-end submission completes within the 30-second bound)
**Linked Requirement:** REQ-NF-001
**Description:** Validates that the elapsed time from invoking "Report Incident" to the user receiving submission confirmation is under 30 seconds under normal operating conditions (no injected failures, no artificial delay).
**Validation Condition:** Measured elapsed time between the "Report Incident" action invocation timestamp and the confirmation-message-displayed timestamp is strictly less than 30.000 seconds.
**Expected Result:** The measured elapsed duration is `< 30s`, recorded as a numeric value in the test report (e.g., "elapsed_seconds: 2.4").

* **User Scenario: SCN-NF-001-A1**
  * **Given** the application is running under normal operating conditions with no injected storage delays or failures
  * **When** the user invokes "Report Incident", enters a valid description, and submits
  * **Then** the elapsed time from invocation to displayed confirmation is measured and is strictly less than 30 seconds

---

### Requirement Validation: REQ-NF-002 (Empty-description feedback within 1 second, no input discarded)

#### Test Case: ATP-NF-002-A (Empty-description feedback is shown within 1 second without discarding other input)
**Linked Requirement:** REQ-NF-002
**Description:** Validates that when a user attempts to submit with an empty description while other input fields already hold data, the REQ-004 feedback message is displayed within 1 second and that other input is preserved.
**Validation Condition:** Given a submission attempt where the description is empty but an optional context note field already contains the value "context-note-value", the required-description message (per REQ-004) appears within 1.000 second of the submission attempt, and after the message is shown the context-note field still contains "context-note-value".
**Expected Result:** Measured feedback latency is `<= 1s`, and the context-note field's value remains exactly "context-note-value" after the feedback is displayed.

* **User Scenario: SCN-NF-002-A1**
  * **Given** the user has entered "context-note-value" into an available optional field and left the description field empty
  * **When** the user attempts to submit the incident report
  * **Then** the required-description message is displayed within 1 second of the attempt, and the "context-note-value" entry remains unchanged and undiscarded

---

### Requirement Validation: REQ-NF-003 (100% of successful submissions retrievable intact)

#### Test Case: ATP-NF-003-A (All successfully submitted incidents in a batch are retrievable with fields intact)
**Linked Requirement:** REQ-NF-003
**Description:** Validates that every incident successfully submitted (confirmed per REQ-009) is subsequently retrievable (per REQ-014) with its description, timestamp, and reporter context unmodified.
**Validation Condition:** A fixed batch of 10 distinct incident reports is submitted and each receives a confirmation (per REQ-009); a subsequent retrieval-all call returns exactly 10 records whose description, timestamp, and reporter-context fields each match byte-for-byte the values submitted/recorded at confirmation time.
**Expected Result:** `10 out of 10` (100%) of the confirmed submissions are found in the retrieval result with description, timestamp, and reporter-context fields identical to their recorded values.

* **User Scenario: SCN-NF-003-A1**
  * **Given** a fixed batch of 10 distinct incident reports, each with a unique description, has been submitted and each received a REQ-009 confirmation
  * **When** the user retrieves the full list of previously recorded incidents
  * **Then** all 10 confirmed incidents are present in the retrieval result with description, timestamp, and reporter-context fields identical to their values at confirmation time

---

### Requirement Validation: REQ-NF-004 (100% of injected storage failures produce visible error + retry, zero silent drops)

#### Test Case: ATP-NF-004-A (Failure-injection suite: every induced storage failure yields visible error and retry, none silently dropped)
**Linked Requirement:** REQ-NF-004
**Description:** Validates, via a bounded failure-injection test suite, that every induced storage-layer failure during submission results in the REQ-010 error message and the REQ-011 retry capability, with zero cases of an incident being dropped without notification.
**Validation Condition:** A defined suite of 5 distinct induced storage-layer failure modes (e.g., write timeout, disk-full, connection-reset, write-then-crash, permission-denied) is run, one submission attempt per mode; for each of the 5 attempts, the REQ-010 error message is displayed and the REQ-011 retry action is available and functional.
**Expected Result:** `5 out of 5` (100%) of the induced-failure attempts produce the REQ-010 error message and a working REQ-011 retry path; `0 out of 5` result in the user receiving no notification of the failure.

* **User Scenario: SCN-NF-004-A1**
  * **Given** a failure-injection suite of 5 defined storage-layer failure modes is configured, and the incident store's record count is a known value N
  * **When** an incident submission is attempted once under each of the 5 failure modes in turn
  * **Then** all 5 attempts display the REQ-010 error message and offer a working REQ-011 retry, and the incident store's record count remains N until a retry succeeds

---

### Requirement Validation: REQ-NF-005 (All recorded timestamps directly comparable in UTC)

#### Test Case: ATP-NF-005-A (Timestamps recorded under different concurrently-viewed timezones are directly comparable in UTC)
**Linked Requirement:** REQ-NF-005
**Description:** Validates that timestamps recorded across multiple incidents remain directly comparable (all expressed in the same UTC reference per REQ-008), regardless of the timezone the reporter was viewing/converting at submission time.
**Validation Condition:** Two incidents are recorded seconds apart: incident "INC-X" submitted while the application's active display context is "America/New_York", and incident "INC-Y" submitted immediately after while the active display context is "Asia/Tokyo". Both recorded timestamps carry a UTC offset of `+00:00`, and `timestamp(INC-Y) > timestamp(INC-X)` holds true when compared directly as UTC values without any timezone conversion step.
**Expected Result:** Both timestamps are stored with UTC offset `+00:00`, and the direct chronological comparison `timestamp(INC-Y) > timestamp(INC-X)` evaluates to true without requiring any additional timezone-conversion step by the comparer.

* **User Scenario: SCN-NF-005-A1**
  * **Given** incident "INC-X" was recorded while the application's active display context was "America/New_York"
  * **When** incident "INC-Y" is recorded immediately afterward while the active display context is "Asia/Tokyo"
  * **Then** both incidents' timestamps carry a UTC offset of `+00:00`, and comparing them directly shows `timestamp(INC-Y)` is later than `timestamp(INC-X)` with no further timezone conversion needed

---

## Constraint Requirements

### Requirement Validation: REQ-CN-001 (Maximum description length enforced)

#### Test Case: ATP-CN-001-A (Description at the maximum length is accepted)
**Linked Requirement:** REQ-CN-001
**Description:** Validates that a description exactly at the enforced maximum length boundary is accepted (boundary-inclusive behavior of the constraint).
**Validation Condition:** Submitting a description whose length is exactly `max_length` characters (the enforced constant defined during planning) is accepted and recorded.
**Expected Result:** The submission succeeds; the recorded incident's description field has a length of exactly `max_length` characters and matches the submitted text exactly.

* **User Scenario: SCN-CN-001-A1**
  * **Given** the system enforces a maximum description length of `max_length` characters
  * **When** the user submits an incident report with a description exactly `max_length` characters long
  * **Then** the submission succeeds and the recorded incident's description field matches the submitted text exactly, at length `max_length`

#### Test Case: ATP-CN-001-B (Description exceeding the maximum length is rejected)
**Linked Requirement:** REQ-CN-001
**Description:** Validates that a description exceeding the enforced maximum length boundary by one character is rejected (this is the constraint side of the same boundary validated jointly with the user-facing message in ATP-012-A).
**Validation Condition:** Submitting a description whose length is exactly `max_length + 1` characters is rejected and not recorded.
**Expected Result:** No new incident record is created; the incident store's record count is unchanged from before the attempt.

* **User Scenario: SCN-CN-001-B1**
  * **Given** the system enforces a maximum description length of `max_length` characters, and the incident store contains a known count of N records
  * **When** the user submits an incident report with a description exactly `max_length + 1` characters long
  * **Then** the submission is rejected and the incident store still contains exactly N records

---

## Coverage Summary

| Metric | Count |
|--------|-------|
| Total Requirements | 21 |
| Total Test Cases (ATP) | 22 |
| Total Scenarios (SCN) | 24 |
| REQ → ATP Coverage | 100% |
| ATP → SCN Coverage | 100% |

**Validation Status**: Full Coverage — every REQ-NNN, REQ-NF-NNN, and REQ-CN-NNN ID in `requirements.md` has at least one ATP, and every ATP has at least one SCN. Verified by manual cross-check against all 21 requirement IDs (REQ-001–REQ-015, REQ-NF-001–REQ-NF-005, REQ-CN-001), since no automated `validate-requirement-coverage` script/template is present in this repository checkout. No coverage gaps identified. The out-of-scope duplicate-submission item (flagged `[NEEDS CLARIFICATION]` in requirements.md, no REQ ID assigned) is intentionally excluded from this plan.

**Generated**: 2026-09-21
**Validated by**: Manual ID cross-check (deterministic script unavailable in this environment; see note above)
