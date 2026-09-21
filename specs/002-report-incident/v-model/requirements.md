# V-Model Requirements Specification: Report Incident

**Feature Branch**: `002-report-incident`
**Source Document**: `specs/002-report-incident/spec.md` (primary source of truth)
**Generated**: 2026-09-21
**Status**: Draft

## Overview

This feature adds a "Report Incident" capability to the TimezoneUtility application. Users need a way to submit a free-text description of a problem they encountered (e.g., incorrect time shown, unexpected error, confusing behavior) through a "Report Incident" action, which is recorded by a backend endpoint together with a timestamp and available reporter context. Recorded incidents must be durably persisted, uniquely identifiable, and retrievable in report order so that whoever maintains the application can track and address them. The requirements below are extracted directly from `spec.md`'s User Scenarios, Functional Requirements (FR-001–FR-012), Key Entities, Edge Cases, and Success Criteria (SC-001–SC-005), with compound statements atomized into single-concern requirements per IEEE 29148 / INCOSE guidance.

## Functional Requirements

| ID | Description | Priority | Rationale | Verification Method |
|----|--------------|----------|-----------|----------------------|
| REQ-001 | The application shall provide a "Report Incident" action, labeled as such, that is invocable by any user from the application's interaction surface (e.g., command, menu item, or interactive prompt). | P1 | Traces to FR-001 and User Story 1 (core capability). | Demonstration |
| REQ-002 | The system shall accept a free-text description of the incident as input when the user invokes the "Report Incident" action. | P1 | Traces to FR-002 and User Story 1, Acceptance Scenario 1. | Test |
| REQ-003 | The system shall reject submission of an incident report when the description field is empty or missing. | P1 | Traces to FR-003 and User Story 1, Acceptance Scenario 2. | Test |
| REQ-004 | When a submission is rejected for an empty or missing description, the system shall display a message to the user stating that a description is required. | P1 | Traces to FR-003 and User Story 1, Acceptance Scenario 2 (split from FR-003 per Atomicity — "reject" and "inform" are independently verifiable outcomes). | Test |
| REQ-005 | The system shall record a timestamp of when the report was submitted for every submitted incident report. | P1 | Traces to FR-004 (split) and User Story 1, Acceptance Scenario 1. | Test |
| REQ-006 | The system shall record the available reporter context (e.g., session or user identifier, and the command or view in use) for every submitted incident report. | P1 | Traces to FR-004 (split) and User Story 1, Acceptance Scenario 1. | Test |
| REQ-007 | The system shall assign a unique identifier to each recorded incident at the time it is recorded. | P1 | Traces to FR-005 and User Story 1, Acceptance Scenario 3. | Test |
| REQ-008 | The system shall record the incident timestamp using Coordinated Universal Time (UTC), independent of any timezone the user is currently viewing or converting elsewhere in the application. | P1 | Traces to FR-006 and Edge Cases ("How is the 'current time' ... determined"). | Test |
| REQ-009 | Upon successful submission, the system shall display a confirmation message to the user that includes the unique identifier of the recorded incident. | P1 | Traces to FR-007 and User Story 2, Acceptance Scenario 1. | Test |
| REQ-010 | When an incident report fails to be recorded, the system shall display an error message to the user stating that the report was not saved. | P1 | Traces to FR-008 (split) and User Story 2, Acceptance Scenario 2. | Test |
| REQ-011 | When an incident report fails to be recorded, the system shall allow the user to retry submission without requiring the user to re-enter the description they had already provided. | P1 | Traces to FR-008 (split) and Edge Cases ("the incident MUST NOT be silently dropped"). | Test |
| REQ-012 | When a submitted description exceeds the system's maximum length limit (see REQ-CN-001), the system shall display a message to the user stating that the length limit was exceeded. | P2 | Traces to FR-009 (split) and Edge Cases ("What happens when the description is extremely long?"). | Test |
| REQ-013 | The system shall persist every recorded incident such that it remains retrievable after the current application session has ended. | P1 | Traces to FR-010 and User Story 3. | Test |
| REQ-014 | The system shall provide a way to retrieve previously recorded incidents, each with its description, timestamp, reporter context, and unique identifier, ordered by the timestamp at which it was reported (earliest first). | P2 | Traces to FR-011 and User Story 3, Acceptance Scenario 1. | Test |
| REQ-015 | When reporter context is partially or fully unavailable at submission time, the system shall still record the incident report, marking the unavailable portion(s) of reporter context as unknown rather than blocking submission. | P2 | Traces to FR-012 and Edge Cases ("What happens if reporter context ... is unavailable?"). | Test |

## Non-Functional Requirements

| ID | Description | Priority | Rationale | Verification Method |
|----|--------------|----------|-----------|----------------------|
| REQ-NF-001 | The elapsed time from a user invoking "Report Incident" to the user receiving submission confirmation shall be under 30 seconds under normal operating conditions. | P2 | Traces to SC-001. | Test |
| REQ-NF-002 | When a user attempts to submit an incident report with an empty description, the system shall display the actionable feedback required by REQ-004 within 1 second of the submission attempt, without discarding any other input the user had already provided. | P2 | Traces to SC-003. | Test |
| REQ-NF-003 | 100% of incident reports that are successfully submitted (per REQ-009) shall be retrievable afterward (per REQ-014) with their description, timestamp, and reporter context intact and unmodified. | P1 | Traces to SC-002. | Test |
| REQ-NF-004 | In a failure-injection test suite that induces a defined set of storage-layer failures during incident submission, 100% of the induced failures shall result in the visible error message required by REQ-010 and the retry capability required by REQ-011, with zero observed cases of an incident being dropped without user notification. | P1 | Traces to SC-004. Rephrased from the source's unbounded "no incident report is ever silently lost" (Anti-Pattern Guard 3: Untestable Universal) into a bounded, executable failure-injection test suite, matching the finite verification approach SC-004 itself specifies. | Test |
| REQ-NF-005 | 100% of timestamps recorded across all incidents shall be directly comparable to one another (i.e., expressed in the same UTC reference per REQ-008) regardless of the reporter's local timezone or any timezone(s) the application is concurrently converting. | P1 | Traces to SC-005. | Test |

## Constraint Requirements

| ID | Description | Priority | Rationale | Verification Method |
|----|--------------|----------|-----------|----------------------|
| REQ-CN-001 | The system shall enforce a maximum length on the free-text incident description field, rejecting submissions whose description exceeds this limit (with the exact numeric limit deferred to the planning phase per spec.md Assumptions). | P2 | Traces to FR-009 and Edge Cases ("System MUST enforce a reasonable maximum length"). Anti-Pattern Guard 1 check: this constraint limits the description-submission capability already introduced by REQ-002 (functional requirement exists), so no missing functional counterpart was found. | Test |

## Requirements Flagged for Clarification

- `[NEEDS CLARIFICATION: Edge Cases states the system "SHOULD guard against rapid duplicate submissions from a single accidental double-trigger," but this is phrased as a non-mandatory SHOULD with no concrete detection window, dedup key, or user-visible behavior specified. Per the strict-translation rule and Criterion 4 (Complete), this was not formalized into a REQ-CN or REQ-NF item — doing so would require inventing a threshold not present in the source. Recommend the product owner specify: (a) whether this is mandatory (MUST) or optional (SHOULD) for this release, and (b) a concrete duplicate-detection window/criterion if mandatory.]`

No `[CONFLICT: ...]` items were identified — all 15 functional requirements, 5 non-functional requirements, and 1 constraint requirement extracted from spec.md are mutually consistent.

## Anti-Pattern Guard Results

- **Guard 1 (Constraint Absorption)**: 1 constraint requirement checked (REQ-CN-001). Its suppressed capability (description length beyond the limit) has a matching functional requirement (REQ-002, which introduces the free-text description capability). No missing functional requirement found; no additions needed.
- **Guard 2 (Success Criteria Coverage)**: 5 Success Criteria found in spec.md (SC-001–SC-005). All 5 are covered by a corresponding requirement's Rationale column (REQ-NF-001 through REQ-NF-005, respectively). No dropout found; no additions needed.
- **Guard 3 (Untestable Universal)**: Scanned all "Test"-method requirements for universal quantifiers ("never", "always", "all cases", "under no circumstances"). One instance found — the source phrasing behind SC-004 ("no incident report is ever silently lost"). Action taken: rephrased as REQ-NF-004 using a bounded, finite failure-injection test suite (matching SC-004's own stated verification approach) rather than an unfalsifiable universal claim. No other universals requiring rephrasing were found; REQ-013's "remains retrievable after the current application session has ended" and REQ-015's "still record ... when reporter context is unavailable" are both scoped to a single, directly testable scenario and were left as Test-verifiable.

## Assumptions

- Per spec.md's own Assumptions section: "UI" refers to whatever interaction surface the application currently exposes (command-line/terminal-oriented, per the existing `src/TimezoneUtility` structure), not necessarily a graphical or web interface; "endpoint" refers to any defined entry point (command, API route, or service call), not necessarily HTTP-specific.
- Reporter context capture is best-effort using information the application already has available; no new authentication or identity system is introduced by this feature (carried into REQ-006 and REQ-015).
- This feature covers submission and raw retrieval of incidents only; a user-facing dashboard for browsing incidents is explicitly out of scope (per spec.md Assumptions), and no REQ-NNN was generated for such a dashboard.
- The persistence mechanism (e.g., local file, database) is an implementation detail deferred to the planning phase; REQ-013 is written to be persistence-mechanism-agnostic.
- The exact numeric maximum description length is deferred to the planning phase (per spec.md Assumptions: "a few thousand characters" as an illustrative, non-binding example); REQ-CN-001 intentionally does not hard-code a number.

## Dependencies

- Existing `src/TimezoneUtility` application structure and its current interaction surface (command-line/terminal-oriented), which the "Report Incident" action must integrate into (REQ-001).
- A persistence layer capable of durable storage across sessions, to be selected during planning (REQ-013, REQ-NF-003).
- Whatever session/state information the application already tracks, which supplies "reporter context" on a best-effort basis (REQ-006, REQ-015).

## Glossary

| Term | Definition |
|------|------------|
| Incident Report | A record of a user-reported problem, comprising a unique identifier, free-text description, UTC timestamp, reporter context, and status. |
| Reporter Context | Best-effort information about the circumstances of a report, captured at submission time (e.g., session/user identifier, and the command, view, or operation in use). |
| Reference Identifier | The unique identifier assigned to a recorded incident, returned to the user as part of submission confirmation (REQ-007, REQ-009). |
| UTC | Coordinated Universal Time; the unambiguous, consistent time reference in which all incident timestamps are recorded (REQ-008), independent of any timezone conversions performed elsewhere in the application. |

## Summary Metrics

- **Total requirements**: 21
  - Functional (REQ-NNN): 15
  - Non-Functional (REQ-NF-NNN): 5
  - Interface (REQ-IF-NNN): 0 (category omitted — spec.md defers all interface/protocol details to the planning phase)
  - Constraint (REQ-CN-NNN): 1
- **By priority**: P1 = 12, P2 = 9, P3 = 0
- **By verification method**: Test = 20, Demonstration = 1, Inspection = 0, Analysis = 0
- **Flags raised**: 1 `[NEEDS CLARIFICATION]`, 0 `[CONFLICT]`, 0 `[ADDED BY GUARD]` (all guard checks passed without requiring additions)
