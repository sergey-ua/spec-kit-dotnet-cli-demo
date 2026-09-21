# V-Model Requirements Specification: Report Incident

**Feature Branch**: `002-report-incident`

**Source**: `specs/002-report-incident/spec.md`

**Generated**: 2026-09-21

**Status**: Draft

## Overview

The TimezoneUtility application needs a way for users to report incidents (e.g., incorrect time shown, unexpected errors, confusing behavior) from within its interface, and for a maintainer to later retrieve those reports for triage. This document translates the user stories, functional requirements (FR-001–FR-012), key entities, and success criteria of `spec.md` into atomic, testable, uniquely-identified requirements (`REQ-NNN`) that will be paired with acceptance test cases in the next V-Model phase.

## Functional Requirements

| ID | Description | Priority | Rationale | Verification Method |
|----|-------------|----------|-----------|---------------------|
| REQ-001 | The application shall provide a "Report Incident" action, labeled as such, invocable by any user from the application's interaction surface (e.g., command, menu option, or interactive prompt). | P1 | Derived from FR-001 and User Story 1; the action is the entry point for the entire feature. | Demonstration |
| REQ-002 | The system shall accept a free-text description of the incident as input when the user invokes the "Report Incident" action. | P1 | Derived from FR-002 and User Story 1 Acceptance Scenario 1. | Test |
| REQ-003 | The system shall reject an incident report submission whose description is empty or missing, and shall not create an incident record for that submission. | P1 | Derived from FR-003 and User Story 1 Acceptance Scenario 2. | Test |
| REQ-004 | When an incident report submission is rejected because its description is empty or missing, the system shall present the user a message stating that a description is required. | P1 | Derived from FR-003 (split from REQ-003 per Atomic criterion: rejection and the informing-the-user message are two separately verifiable behaviors). | Test |
| REQ-005 | Upon a valid incident report submission, the system shall record the incident's description exactly as entered by the user. | P1 | Derived from FR-004 and User Story 1 Acceptance Scenario 1 (split from FR-004's compound "description, timestamp, reporter context" list per Atomic criterion). | Test |
| REQ-006 | Upon a valid incident report submission, the system shall record a timestamp indicating when the report was submitted. | P1 | Derived from FR-004 and FR-006, and User Story 1 Acceptance Scenario 1 (split per Atomic criterion). | Test |
| REQ-007 | The system shall record the incident timestamp (REQ-006) using Coordinated Universal Time (UTC), independent of any timezone the user is currently viewing or converting elsewhere in the application. | P1 | Derived from FR-006 and the "current time" Edge Case; ensures timestamps are unambiguous and comparable regardless of the utility's timezone-conversion features (traces to SC-005). | Test |
| REQ-008 | Upon a valid incident report submission, the system shall record the available reporter context at the time of submission, consisting of a session or user identifier (if available) and the application state (e.g., command or view) in use at that time. | P1 | Derived from FR-004 and the Reporter Context key entity (split per Atomic criterion). | Test |
| REQ-009 | If any element of reporter context (REQ-008) is unavailable at submission time, the system shall record that element as "unknown" or "unavailable" rather than blocking submission. | P1 | Derived from FR-012 and the "reporter context unavailable" Edge Case. | Test |
| REQ-010 | The system shall assign a unique identifier to each incident report it records, distinct from the identifier of every other recorded incident. | P1 | Derived from FR-005 and User Story 1 Acceptance Scenario 3. | Test |
| REQ-011 | The system shall accept and record each incident report submission as a distinct recorded incident, even when the same description is submitted more than once in succession. | P2 | Derived from the "duplicate submission" Edge Case ("System MUST accept each submission as a distinct recorded incident"). | Test |
| REQ-012 | The system shall guard against recording more than one incident from a single accidental double-trigger of the "Report Incident" action within the same user interaction. | P3 | Derived from the "duplicate submission" Edge Case ("SHOULD guard against rapid duplicate submissions from a single accidental double-trigger"). [NEEDS CLARIFICATION: spec.md does not define a concrete time window or trigger-detection mechanism for "rapid duplicate submissions"; a threshold will need to be defined during planning/design.] | Test |
| REQ-013 | Upon successful recording of an incident report, the system shall display to the user a confirmation message that includes the unique identifier (REQ-010) assigned to that report. | P1 | Derived from FR-007 and User Story 2 Acceptance Scenario 1. | Test |
| REQ-014 | When an incident report submission fails to be recorded, the system shall display to the user an error message stating that the report was not saved. | P1 | Derived from FR-008 and User Story 2 Acceptance Scenario 2. | Test |
| REQ-015 | When an incident report submission fails to be recorded, the system shall allow the user to retry the submission without requiring the user to re-enter the description they had already provided. | P1 | Derived from FR-008 and the "backend recording step fails" Edge Case (split from REQ-014 per Atomic criterion: displaying the error and preserving retry input are separately verifiable). | Test |
| REQ-016 | The system shall enforce a maximum length on the incident description, rejecting submissions whose description exceeds that length. | P2 | Derived from FR-009 and the "extremely long description" Edge Case. [NEEDS CLARIFICATION: spec.md's Assumptions section defers the exact maximum length to the planning phase ("a few thousand characters"); the specific numeric threshold must be fixed before a Pass/Fail test can be written.] | Test |
| REQ-017 | When an incident report submission is rejected for exceeding the maximum description length (REQ-016), the system shall inform the user that the length limit was exceeded. | P2 | Derived from FR-009 (split from REQ-016 per Atomic criterion). | Test |
| REQ-018 | The system shall persist every successfully recorded incident report such that it remains retrievable after the current application session has ended. | P1 | Derived from FR-010; underlies User Story 3 and SC-002. | Test |
| REQ-019 | The system shall provide a way to retrieve all previously recorded incident reports, each including its description, timestamp, reporter context, and unique identifier. | P2 | Derived from FR-011 and User Story 3 Acceptance Scenario 1. | Test |
| REQ-020 | When retrieving recorded incident reports, the system shall return them ordered by the timestamp at which they were reported. | P2 | Derived from FR-011 and User Story 3 ("retrievable in the order they occurred"). | Test |
| REQ-021 | When no incident reports have been recorded, a retrieval request shall return an empty result set without raising an error. | P2 | Derived from User Story 3 Acceptance Scenario 2. | Test |

## Non-Functional Requirements

| ID | Description | Priority | Rationale | Verification Method |
|----|-------------|----------|-----------|---------------------|
| REQ-NF-001 | The elapsed time from a user invoking the "Report Incident" action to the user receiving a submission confirmation (REQ-013) shall be less than 30 seconds. | P2 | Derived from SC-001. [ADDED BY GUARD 2: SC Coverage — SC-001 had no corresponding requirement until this entry was added.] | Test |
| REQ-NF-002 | For 100% of successfully recorded incident reports, retrieval (REQ-019) shall return the description, timestamp, and reporter context identical to what was recorded at submission time. | P1 | Derived from SC-002. [ADDED BY GUARD 2: SC Coverage — SC-002 had no corresponding requirement until this entry was added.] | Test |
| REQ-NF-003 | The feedback message informing the user of an incomplete submission (REQ-004) shall be displayed to the user within 1 second of the rejected submission attempt, and shall not discard any other input the user had already provided in the same form/prompt. | P2 | Derived from SC-003. [ADDED BY GUARD 2: SC Coverage — SC-003 had no corresponding requirement until this entry was added.] | Test |
| REQ-NF-004 | Across a series of repeated failure-injection tests simulating backend recording failures, every failed submission shall result in a visible error message (REQ-014) and an available retry option (REQ-015), with zero incidents silently dropped. | P1 | Derived from SC-004. [ADDED BY GUARD 2: SC Coverage — SC-004 had no corresponding requirement until this entry was added.] Verification method is Test rather than pure Inspection because "repeated failure-injection tests" describes an executable, bounded test procedure rather than an unfalsifiable universal claim ("no incident report is ever silently lost" is operationalized as a finite, repeatable test suite per Guard 3). | Test |
| REQ-NF-005 | For 100% of recorded incidents, the recorded timestamp (REQ-007) shall be directly comparable across reports without conversion, regardless of the reporter's local timezone or the timezone(s) the application is currently converting between. | P1 | Derived from SC-005. [ADDED BY GUARD 2: SC Coverage — SC-005 had no corresponding requirement until this entry was added.] | Test |

## Constraint Requirements

| ID | Description | Priority | Rationale | Verification Method |
|----|-------------|----------|-----------|---------------------|
| REQ-CN-001 | This feature shall not introduce a new authentication or identity system; reporter context (REQ-008) shall be limited to identifying/session/state information the application already has available at the time of reporting. | P2 | Derived from the spec.md Assumptions section ("no new authentication or identity system is introduced by this feature"). Guard 1 check: this constrains how reporter context is captured but does not suppress a capability described elsewhere without a corresponding functional requirement — REQ-008 already covers reporter context capture, so no additional REQ is required. | Inspection |
| REQ-CN-002 | This feature shall not include a user-facing dashboard for browsing incidents; retrieval (REQ-019) provides raw recorded incident data only, not a browsing UI. | P3 | Derived from the spec.md Assumptions section ("this feature does not include a user-facing dashboard for browsing incidents ... only the ability to submit and to retrieve raw recorded incidents"). Guard 1 check: the suppressed capability is "a dashboard for browsing incidents," which is explicitly out of scope for this feature (a separate future capability per the source) rather than a capability this spec's functional requirements are meant to build — no missing REQ-NNN is implied. | Inspection |

## Assumptions

- "UI" and "endpoint," per spec.md's own Assumptions section, refer to whatever interaction surface and entry-point mechanism the TimezoneUtility application already exposes (e.g., a command or interactive prompt), not necessarily a graphical or HTTP-specific interface. Requirements above (e.g., REQ-001) are written interface-agnostically for this reason.
- The specific numeric maximum description length (REQ-016) and the specific "rapid duplicate submission" detection window (REQ-012) are left as planning-phase details per spec.md, and are flagged with `[NEEDS CLARIFICATION]` above rather than assumed.
- The persistence mechanism (e.g., local file, database) is a planning-phase implementation detail per spec.md's Assumptions section; REQ-018 only requires that recorded incidents survive beyond the current session, not any specific storage technology.

## Dependencies

- Existing TimezoneUtility application interaction surface (command-line/terminal-oriented, per spec.md Assumptions) into which the "Report Incident" action (REQ-001) is added.
- Whatever session/state information the application already maintains internally, which REQ-008/REQ-009 depend on for reporter context capture.
- A persistence layer (mechanism unspecified at this stage) that REQ-018 depends on for surviving beyond the current session.

## Glossary

- **Incident Report**: A record of a user-reported problem, comprising a unique identifier, description, timestamp (UTC), reporter context, and status.
- **Reporter Context**: Information about the circumstances of an incident report at submission time — the reporter/session identifier (if available) and the relevant application state (e.g., command or view in use).
- **UTC**: Coordinated Universal Time; the unambiguous, consistent time reference used for incident timestamps regardless of any timezone conversion elsewhere in the application.

## Summary Metrics

- **Total requirements**: 28
  - Functional (REQ-NNN): 21
  - Non-Functional (REQ-NF-NNN): 5
  - Interface (REQ-IF-NNN): 0
  - Constraint (REQ-CN-NNN): 2
- **By priority**: P1 = 14, P2 = 11, P3 = 3
- **By verification method**: Test = 25, Inspection = 2, Analysis = 0, Demonstration = 1
- **Flags**: 2 `[NEEDS CLARIFICATION]` (REQ-012, REQ-016); 0 `[CONFLICT]`; 5 `[ADDED BY GUARD 2]` (REQ-NF-001 through REQ-NF-005, covering SC-001–SC-005); 0 `[ADDED BY GUARD 1]`; 0 `[FEASIBILITY CONCERN]`.

## Next Step

Run `/speckit.v-model.acceptance` to generate the paired Acceptance Test Plan (ATP-NNN test cases and BDD scenarios) tracing back to these requirements.
