namespace TimezoneUtility.Models;

/// <summary>One raw data row read from the input batch CSV file (SYS-001/SYS-002 output; SYS-003 input).</summary>
public sealed record BatchConversionRow
{
    /// <summary>1-based row number, counting data rows only (header excluded, blank rows excluded per REQ-028).</summary>
    public required int RowNumber { get; init; }

    /// <summary>Raw timestamp cell value, exactly as read from the CSV (may be empty).</summary>
    public required string RawTimestamp { get; init; }

    /// <summary>Raw source timezone cell value, exactly as read from the CSV (may be empty).</summary>
    public required string RawSourceTimezone { get; init; }

    /// <summary>Raw target timezone cell value, exactly as read from the CSV (may be empty).</summary>
    public required string RawTargetTimezone { get; init; }
}
