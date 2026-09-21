using NodaTime;

namespace TimezoneUtility.Models;

/// <summary>The successful conversion outcome for one batch row (REQ-007, SYS-008 output).</summary>
public sealed record RowConversionResult
{
    /// <summary>1-based row number this result corresponds to.</summary>
    public required int RowNumber { get; init; }

    /// <summary>Original raw timestamp value from the input row.</summary>
    public required string OriginalTimestamp { get; init; }

    /// <summary>Original raw source timezone value from the input row.</summary>
    public required string OriginalSourceTimezone { get; init; }

    /// <summary>Original raw target timezone value from the input row.</summary>
    public required string OriginalTargetTimezone { get; init; }

    /// <summary>The converted instant, computed via the existing ITimeService/TimeService (SYS-005 reuse).</summary>
    public required Instant ConvertedInstant { get; init; }

    /// <summary>The target timezone the instant is displayed in.</summary>
    public required DateTimeZone TargetZone { get; init; }

    /// <summary>Converted local date/time in the target zone (includes date, so date-boundary crossings are correct per REQ-025).</summary>
    public LocalDateTime ConvertedLocalDateTime => ConvertedInstant.InZone(TargetZone).LocalDateTime;
}
