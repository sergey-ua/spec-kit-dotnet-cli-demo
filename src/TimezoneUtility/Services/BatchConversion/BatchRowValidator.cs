using NodaTime;
using TimezoneUtility.Models;

namespace TimezoneUtility.Services.BatchConversion;

/// <summary>
/// Result of validating a single <see cref="BatchConversionRow"/>: either the row is valid and its
/// resolved source/target zones are returned, or a field-specific failure reason is returned (REQ-013-REQ-015).
/// </summary>
public sealed record BatchRowValidationResult
{
    /// <summary>Non-null field-specific reason when the row is invalid.</summary>
    public string? Reason { get; init; }

    /// <summary>The resolved source timezone, when valid.</summary>
    public DateTimeZone? SourceZone { get; init; }

    /// <summary>The resolved target timezone, when valid.</summary>
    public DateTimeZone? TargetZone { get; init; }

    public bool IsValid => Reason is null;
}

/// <summary>
/// SYS-003 Row Validator: checks a <see cref="BatchConversionRow"/> for missing fields, unparseable
/// timestamps, and unrecognized timezones, producing a field-specific reason on failure
/// (REQ-003, REQ-008, REQ-009, REQ-010, REQ-015).
/// </summary>
public static class BatchRowValidator
{
    /// <summary>Validates the given row, resolving its timezones if all fields are present.</summary>
    public static BatchRowValidationResult Validate(BatchConversionRow row)
    {
        if (string.IsNullOrWhiteSpace(row.RawTimestamp))
        {
            return new BatchRowValidationResult { Reason = "missing field: timestamp" };
        }

        if (string.IsNullOrWhiteSpace(row.RawSourceTimezone))
        {
            return new BatchRowValidationResult { Reason = "missing field: source_timezone" };
        }

        if (string.IsNullOrWhiteSpace(row.RawTargetTimezone))
        {
            return new BatchRowValidationResult { Reason = "missing field: target_timezone" };
        }

        var sourceZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(row.RawSourceTimezone.Trim());
        if (sourceZone is null)
        {
            return new BatchRowValidationResult { Reason = $"unrecognized timezone: {row.RawSourceTimezone}" };
        }

        var targetZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(row.RawTargetTimezone.Trim());
        if (targetZone is null)
        {
            return new BatchRowValidationResult { Reason = $"unrecognized timezone: {row.RawTargetTimezone}" };
        }

        return new BatchRowValidationResult { SourceZone = sourceZone, TargetZone = targetZone };
    }
}
