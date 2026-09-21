namespace TimezoneUtility.Models;

/// <summary>Run-level total/succeeded/failed counts for one batch conversion run (REQ-021, REQ-022, REQ-023, SYS-010 output).</summary>
public sealed record BatchRunSummary
{
    /// <summary>Path to the input CSV file that was processed.</summary>
    public required string InputFile { get; init; }

    /// <summary>Total rows processed (blank rows excluded per REQ-028; header excluded).</summary>
    public required int TotalRows { get; init; }

    /// <summary>Number of rows that were successfully converted.</summary>
    public required int SucceededCount { get; init; }

    /// <summary>Number of rows reported as invalid. Always present and explicit, even when zero (REQ-022).</summary>
    public required int FailedCount { get; init; }

    /// <summary>True when the input file contained a header row and zero data rows (REQ-023).</summary>
    public required bool NoRowsProcessed { get; init; }
}
