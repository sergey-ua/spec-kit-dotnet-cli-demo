using NodaTime;
using NodaTime.Text;

namespace TimezoneUtility.Services.BatchConversion;

/// <summary>
/// SYS-004 Timestamp Interpretation Component: decides whether a raw timestamp cell carries an explicit
/// UTC offset (offset-authoritative, REQ-024) or is a local date+time that must be resolved against the
/// row's source timezone (source-timezone-authoritative, REQ-024). Delegates the actual instant resolution
/// to the reused SYS-005 conversion engine (<see cref="TimeConversion.ITimeService"/>).
/// </summary>
public static class BatchTimestampInterpreter
{
    private static readonly OffsetDateTimePattern[] OffsetPatterns =
    [
        OffsetDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd'T'HH':'mm':'sso<G>"),
        OffsetDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd' 'HH':'mm':'sso<G>"),
    ];

    private static readonly LocalDateTimePattern[] LocalPatterns =
    [
        LocalDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd'T'HH':'mm':'ss"),
        LocalDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd' 'HH':'mm':'ss"),
        LocalDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd'T'HH':'mm"),
        LocalDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd' 'HH':'mm"),
    ];

    /// <summary>
    /// Attempts to interpret the raw timestamp into an <see cref="Instant"/>, given the row's resolved
    /// source timezone. Returns false with no exception when the timestamp cannot be parsed at all
    /// (REQ-008, REQ-013).
    /// </summary>
    public static bool TryInterpret(string rawTimestamp, DateTimeZone sourceZone, TimeConversion.ITimeService timeService, out Instant instant)
    {
        instant = default;

        if (string.IsNullOrWhiteSpace(rawTimestamp))
        {
            return false;
        }

        var trimmed = rawTimestamp.Trim();

        // Explicit-offset form takes precedence (REQ-024).
        foreach (var pattern in OffsetPatterns)
        {
            var offsetResult = pattern.Parse(trimmed);
            if (offsetResult.Success)
            {
                instant = offsetResult.Value.ToInstant();
                return true;
            }
        }

        // Otherwise, local date+time with the source timezone authoritative (REQ-024), via the
        // reused conversion engine (SYS-005).
        foreach (var pattern in LocalPatterns)
        {
            var localResult = pattern.Parse(trimmed);
            if (localResult.Success)
            {
                var slot = timeService.ConvertTime(localResult.Value, sourceZone, sourceZone);
                instant = slot.Instant;
                return true;
            }
        }

        return false;
    }
}
