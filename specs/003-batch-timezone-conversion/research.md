# Research Summary: Batch Timezone Conversion

**Feature**: 003-batch-timezone-conversion
**Date**: 2026-09-21
**Purpose**: Resolve the planning-phase details spec.md's Assumptions section explicitly defers (exact CSV column names, output file mechanism/naming) and confirm the technical approach for the new batch command.

spec.md's Assumptions section states: "exact column names are a planning-phase detail" and "the exact output file mechanism (path conventions, stdout vs. file) is a planning-phase detail." Both are resolved concretely below. There are no outstanding `[NEEDS CLARIFICATION]` markers in `v-model/requirements.md` that block this plan: the three flagged items there (explicit-offset/source-timezone "inconsistency reporting" semantics, upper file-size bound, and the CSV-column-naming visibility note) are either explicitly out of scope for REQ-NNN formalization (per the requirements doc's own reasoning) or are the exact deferred-to-planning items resolved here.

---

## 1. CSV Input Column Convention

### Decision: Header-driven columns named `timestamp`, `source_timezone`, `target_timezone` (case-insensitive match)

### Rationale

- REQ-002/SYS-002 require identifying columns by header name, independent of order, so the exact names only need to be fixed once and documented — they do not constrain parsing order.
- `timestamp` / `source_timezone` / `target_timezone` are unambiguous, snake_case (common CSV export convention), and match the vocabulary already used in spec.md's Key Entities ("raw timestamp value, source timezone identifier, target timezone identifier").
- Column matching is case-insensitive and tolerant of a leading/trailing BOM or whitespace on header cells, so common CSV export tools (Excel, Google Sheets) don't produce spurious header-resolution failures.
- Extra/unrecognized columns (REQ-004) are preserved positionally in the row's raw-values snapshot (for invalid-row reporting, per REQ-012) but ignored for conversion.

### Alternatives Considered

| Alternative | Why Not Chosen |
|---|---|
| Fixed column order (no header lookup) | Directly contradicts REQ-002 ("identify ... via a header row rather than requiring a fixed column order"). |
| `Timestamp`/`SourceTimeZone`/`TargetTimeZone` (PascalCase) | Equivalent in function; snake_case chosen for consistency with typical CSV export tooling and to visually distinguish CSV headers from C# identifiers in documentation. Case-insensitive matching means users can supply either style regardless. |
| Single combined "timezone pair" column (e.g., `America/New_York->Europe/London`) | Adds unnecessary parsing complexity and contradicts the spec's Key Entities, which lists source and target timezone as separate attributes. |

### Documentation Obligation (per REQ-002 visibility note in requirements.md)

The exact header names (`timestamp`, `source_timezone`, `target_timezone`) are documented in `contracts/cli-batch-convert.md` and `quickstart.md` so users know what to name their CSV columns.

---

## 2. Output File Mechanism and Naming

### Decision: Two sibling CSV files written next to the input file (or to an explicit `--output-dir`), named deterministically from the input file's base name plus a fixed suffix; run summary printed to console (stdout) plus exit code

- Successful output: `<input-basename>.converted.csv`
- Invalid-row report: `<input-basename>.invalid-rows.csv`
- Both default to the same directory as the input file unless `--output-dir <path>` is supplied, in which case both are written there instead.
- The run summary (total/succeeded/failed) is always printed to stdout as human-readable text; `--json` switches the summary to a JSON object on stdout, consistent with the existing CLI's `--json` convention (`ConvertCommand`, `NowCommand`, etc.). The two CSV files are written regardless of `--json`, since REQ-019/REQ-020 require the row-level outputs to exist as structured files, and the summary flag only affects how the summary itself is presented.

### Rationale

- Deterministic naming from the input file avoids requiring the user to specify two separate output paths for the common case, while `--output-dir` covers the case where the input directory isn't writable (ties into REQ-029's write-failure handling: the two output paths are both known before any writing starts, so both writes are attempted and any single failure is reported without discarding the other or the in-memory results).
- Keeping the two outputs as separate files (never merged, never one embedded in the other) directly satisfies SYS-009 / REQ-020's separation requirement in the simplest possible way — no custom multi-part format is needed.
- Printing the summary to stdout (not a third file) matches spec.md's Assumption that "output is written in the same general environment the existing timezone utility runs in (e.g., CLI-invoked)" — the existing commands all print human-facing results to the console, and a summary is exactly that kind of result, not a batch artifact a downstream tool would consume the way it would the two CSVs.

### Alternatives Considered

| Alternative | Why Not Chosen |
|---|---|
| Single combined output file with a `status` column (success/invalid) | Directly contradicts REQ-020 / SYS-009 ("kept separate ... do not merge"), which is a binding constraint from the approved system design. |
| Explicit required `--success-output`/`--invalid-output` path arguments (no default naming) | Adds friction for the common case (spec.md's Assumptions favor a documented, planning-time-decided default); still supported implicitly via `--output-dir` for redirection without needing two separate path arguments. |
| Write the run summary to a third file instead of stdout | Unnecessary for a demo-scale feature; all existing commands report results via console output, and nothing in spec.md's requirements calls for a persisted summary artifact. |

---

## 3. CSV Parsing Approach

### Decision: Lightweight internal CSV line parser (no new NuGet dependency)

### Rationale

- The main project currently has zero CSV-handling dependency (NodaTime, NodaTime.Serialization.SystemTextJson, System.CommandLine, Spectre.Console only). Adding a full library like CsvHelper is unnecessary for this demo-scale feature's requirements: comma-delimited fields with optional double-quote-escaped commas cover every documented input shape (spec.md's edge cases mention extra columns and header variations, not embedded newlines or exotic escaping).
- A small internal parser keeps the "single implementation task" scope tight and avoids a new external dependency surface for a CLI demo.
- Reuses the project's existing pattern of small, purpose-built parsing utilities (see `Services/TimeConversion/TimeParser.cs`, which hand-rolls time/date/duration parsing with `GeneratedRegex` rather than pulling in a parsing library).

### Alternatives Considered

| Alternative | Why Not Chosen |
|---|---|
| CsvHelper (NuGet) | More robust for pathological CSV (embedded newlines, exotic quoting) but unnecessary for this feature's documented scope; adds a dependency for a demo-scale, single-command feature. |
| `System.Text.Json`-based parsing | CSV isn't JSON; irrelevant here — retained only for existing JSON output formatting, unrelated to input parsing. |
| Raw `string.Split(',')` with no quote handling | Would break on any quoted field containing a comma; insufficient given REQ-004 (extra columns may exist, and typical CSV exports quote fields containing commas). |

---

## 4. Timezone Conversion / DST Reuse

### Decision: Reuse `ITimeService.ConvertTime(LocalDateTime, DateTimeZone, DateTimeZone)` and `TimeParser` from `Services/TimeConversion/`, with a batch-local timestamp interpreter (SYS-004) that maps a raw CSV timestamp string + source-timezone column to the `LocalDateTime`/`DateTimeZone` inputs `ITimeService` already expects

### Rationale

- SYS-005 (Timezone Conversion Engine) explicitly must reuse the existing `001-timezone-utility` conversion/DST logic rather than duplicate it — `ITimeService.ConvertTime` already performs exactly this operation (`sourceTime.InZoneLeniently(sourceZone)` → instant → target zone), including NodaTime's documented DST resolution (`InZoneLeniently`) that satisfies REQ-026 (DST ambiguity MUST NOT fail the row).
- Timezone identifier recognition reuses `DateTimeZoneProviders.Tzdb.GetZoneOrNull(id)` (the same IANA lookup pattern documented in `001-timezone-utility/research.md` §1) for REQ-009/REQ-010/REQ-003 validation — no new timezone data source is introduced, per spec.md's Assumptions.
- `TimeParser` (existing) does not currently parse full ISO-8601 date+time-with-optional-offset strings (it parses bare times, bare dates, and durations for the interactive `convert` command). The new `BatchTimestampInterpreter` (SYS-004) is therefore a new, small, batch-specific parser: it first attempts to parse the raw CSV timestamp cell as an offset-aware instant (`OffsetDateTime`/ISO 8601 with explicit UTC offset, e.g. `2026-09-21T14:30:00-04:00`) — if that succeeds, the embedded offset takes precedence (per spec.md Assumptions) and the row's source-timezone column is used only for the target-conversion display, not reinterpretation. If no explicit offset is present, it parses the cell as a `LocalDateTime` (date + time, e.g. `2026-09-21 14:30:00` or `2026-09-21T14:30:00`) and treats the row's source-timezone column as authoritative for interpretation (REQ-024), then hands that `LocalDateTime` + source `DateTimeZone` to the existing `ITimeService.ConvertTime` for the actual conversion (SYS-005 reuse boundary).
- This keeps 100% of the actual DST/offset conversion math inside the existing, already-tested `TimeService`, and confines the new code to CSV-shape parsing and pre/post validation — the smallest possible reuse-respecting design.

### Alternatives Considered

| Alternative | Why Not Chosen |
|---|---|
| Reimplement DST/offset conversion inside the batch feature | Directly violates SYS-005's explicit reuse requirement and duplicates already-correct, already-tested logic. |
| Require CSV timestamps to always include an explicit UTC offset (skip the source-timezone-authoritative path) | Contradicts spec.md's Edge Cases ("no explicit UTC offset ... source timezone column is authoritative") and REQ-024. |
| Extend `TimeParser` itself to add ISO-8601 batch parsing | Considered, but `TimeParser`'s existing methods (`ParseTime`, `ParseDate`, `ParseDuration`) target the interactive single-conversion CLI's natural-language-ish input ("3pm", "today"); batch CSV timestamps are a different, stricter input shape (ISO-8601-like). A separate `BatchTimestampInterpreter` keeps each parser's responsibility single and focused, per the constitution's "single responsibility" code-quality principle, while still calling into the same `ITimeService` for the actual zone conversion. |

---

## Summary: Technology & Design Decisions

| Area | Decision |
|---|---|
| CSV column names | `timestamp`, `source_timezone`, `target_timezone` (header-driven, case-insensitive, order-independent) |
| Output files | `<input-basename>.converted.csv` + `<input-basename>.invalid-rows.csv`, same directory as input unless `--output-dir` given |
| Run summary | Printed to stdout (human text by default, JSON with `--json`); not a third file |
| CSV parsing | New small internal parser in `Services/BatchConversion/CsvBatchReader.cs`; no new NuGet dependency |
| Conversion/DST | Reuses existing `ITimeService`/`TimeService` (`Services/TimeConversion/`); new `BatchTimestampInterpreter` only bridges CSV-shaped timestamp strings into the existing `LocalDateTime`/`DateTimeZone` inputs |
| Timezone identifier validation | Reuses `DateTimeZoneProviders.Tzdb.GetZoneOrNull(id)` (same IANA lookup pattern as 001-timezone-utility) |
| Exit codes | See `contracts/cli-batch-convert.md` (0 = run completed, including runs with 0 succeeded or partial success; 1 = invalid CLI input/arguments; 2 = file-level error, per REQ-016–REQ-018; 3 = internal/output-write error, per REQ-029) |

## Open Questions / Risks

None blocking. The three `[NEEDS CLARIFICATION]` flags in `v-model/requirements.md` are informational (deferred-to-planning column naming, now resolved above; an unbounded-file-size question with a documented floor already covered by REQ-NF-005; and an unspecified embedded-offset-vs-source-timezone "inconsistency" reporting behavior that requirements.md explicitly declined to formalize as a REQ). This plan does not invent behavior for the inconsistency case beyond what §4 above states (explicit offset wins silently; source timezone is not re-validated against it) — if the product owner later wants a distinct warning or invalid-row reason for that mismatch, it should be added as a new REQ-NNN and a corresponding SYS-007 reason code in a follow-up, not invented here.
