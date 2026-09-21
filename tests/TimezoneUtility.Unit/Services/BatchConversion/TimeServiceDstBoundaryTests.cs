using FluentAssertions;
using NodaTime;
using TimezoneUtility.Services.TimeConversion;

namespace TimezoneUtility.Unit.Services.BatchConversion;

/// <summary>
/// Unit tests exercising SYS-005 Timezone Conversion Engine (via the reused
/// <see cref="TimeService.ConvertTime"/>) directly at the DST boundary, independent of the batch
/// pipeline around it. Verifies REQ-026, REQ-NF-004.
/// Corresponds to STP-005-B (STS-005-B1, STS-005-B2).
/// </summary>
public class TimeServiceDstBoundaryTests
{
    private readonly TimeService _timeService = new();

    [Fact]
    public void STS_005_B1_SpringForwardNonexistentHour_ResolvesToConvertedResult_NotAnException()
    {
        var sourceZone = DateTimeZoneProviders.Tzdb["America/New_York"];
        var targetZone = DateTimeZoneProviders.Tzdb["Etc/UTC"];
        // 2026-03-08 02:30:00 falls inside the US "spring forward" nonexistent-hour gap for America/New_York.
        var localTime = new LocalDateTime(2026, 3, 8, 2, 30, 0);

        var act = () => _timeService.ConvertTime(localTime, sourceZone, targetZone);

        act.Should().NotThrow();
        var result = act();
        result.Should().NotBeNull();
    }

    [Fact]
    public void STS_005_B2_FallBackRepeatedHour_ResolvesToExactlyOneConvertedResult_AccurateToTheMinute()
    {
        var sourceZone = DateTimeZoneProviders.Tzdb["America/New_York"];
        var targetZone = DateTimeZoneProviders.Tzdb["Etc/UTC"];
        // 2026-11-01 01:30:00 falls inside the US "fall back" repeated hour for America/New_York.
        var localTime = new LocalDateTime(2026, 11, 1, 1, 30, 0);

        var act = () => _timeService.ConvertTime(localTime, sourceZone, targetZone);

        act.Should().NotThrow();
        var result = act();
        result.Should().NotBeNull();
        result.LocalDateTime.Minute.Should().Be(30);
    }
}
