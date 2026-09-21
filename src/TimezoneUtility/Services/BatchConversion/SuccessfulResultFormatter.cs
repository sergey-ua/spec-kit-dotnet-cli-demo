using NodaTime;
using TimezoneUtility.Models;
using TimezoneUtility.Services.TimeConversion;

namespace TimezoneUtility.Services.BatchConversion;

/// <summary>
/// SYS-008 Successful Result Formatter: builds one <see cref="RowConversionResult"/> per successfully
/// validated+interpreted row, delegating the actual conversion to the reused SYS-005 conversion engine
/// (<see cref="ITimeService"/>) — no new conversion logic is introduced here (REQ-005, REQ-007, REQ-025,
/// REQ-026, REQ-027).
/// </summary>
public static class SuccessfulResultFormatter
{
    /// <summary>
    /// Builds the successful result for the given row, its resolved zones, and the already-computed
    /// converted instant (see <see cref="BatchTimestampInterpreter"/>).
    /// </summary>
    public static RowConversionResult Format(BatchConversionRow row, DateTimeZone targetZone, Instant convertedInstant) => new()
    {
        RowNumber = row.RowNumber,
        OriginalTimestamp = row.RawTimestamp,
        OriginalSourceTimezone = row.RawSourceTimezone,
        OriginalTargetTimezone = row.RawTargetTimezone,
        ConvertedInstant = convertedInstant,
        TargetZone = targetZone
    };
}
