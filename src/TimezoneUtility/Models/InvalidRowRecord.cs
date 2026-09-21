namespace TimezoneUtility.Models;

/// <summary>The invalid-row outcome for one batch row that failed validation or conversion (REQ-012-REQ-015, SYS-007 output).</summary>
public sealed record InvalidRowRecord
{
    /// <summary>1-based row number this entry corresponds to.</summary>
    public required int RowNumber { get; init; }

    /// <summary>Original raw timestamp value from the input row (may be empty if missing).</summary>
    public required string RawTimestamp { get; init; }

    /// <summary>Original raw source timezone value from the input row (may be empty if missing).</summary>
    public required string RawSourceTimezone { get; init; }

    /// <summary>Original raw target timezone value from the input row (may be empty if missing).</summary>
    public required string RawTargetTimezone { get; init; }

    /// <summary>Specific, human-readable, field-referencing failure reason.</summary>
    public required string Reason { get; init; }
}
