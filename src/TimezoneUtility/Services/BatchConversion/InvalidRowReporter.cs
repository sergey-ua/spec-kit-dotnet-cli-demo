using TimezoneUtility.Models;

namespace TimezoneUtility.Services.BatchConversion;

/// <summary>
/// SYS-007 Invalid Row Reporter: builds exactly one <see cref="InvalidRowRecord"/> per failed row
/// (REQ-012-REQ-015, REQ-NF-002), never omitting or merging entries.
/// </summary>
public static class InvalidRowReporter
{
    /// <summary>Builds the invalid-row record for a failed row, given its field-specific reason.</summary>
    public static InvalidRowRecord Report(BatchConversionRow row, string reason) => new()
    {
        RowNumber = row.RowNumber,
        RawTimestamp = row.RawTimestamp,
        RawSourceTimezone = row.RawSourceTimezone,
        RawTargetTimezone = row.RawTargetTimezone,
        Reason = reason
    };
}
