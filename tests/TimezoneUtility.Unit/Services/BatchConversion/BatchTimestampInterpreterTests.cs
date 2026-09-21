using FluentAssertions;
using NodaTime;
using TimezoneUtility.Services.BatchConversion;
using TimezoneUtility.Services.TimeConversion;

namespace TimezoneUtility.Unit.Services.BatchConversion;

/// <summary>
/// T010: Unit tests for BatchTimestampInterpreter.
/// Verifies REQ-024. Corresponds to STP-004-A, STP-004-B (STS-004-A1, STS-004-B1).
/// </summary>
public class BatchTimestampInterpreterTests
{
    private readonly ITimeService _timeService = new TimeService();

    [Fact]
    public void STS_004_A1_NoExplicitOffset_IsInterpretedAsLocalToSourceTimezone()
    {
        var sourceZone = DateTimeZoneProviders.Tzdb["America/Chicago"];

        var ok = BatchTimestampInterpreter.TryInterpret("2026-03-10 14:00:00", sourceZone, _timeService, out var instant);

        ok.Should().BeTrue();
        var expected = new LocalDateTime(2026, 3, 10, 14, 0, 0).InZoneLeniently(sourceZone).ToInstant();
        instant.Should().Be(expected);
    }

    [Fact]
    public void STS_004_B1_ExplicitOffset_TakesPrecedenceOverSourceTimezone()
    {
        var sourceZone = DateTimeZoneProviders.Tzdb["America/New_York"];

        var withOffsetOk = BatchTimestampInterpreter.TryInterpret(
            "2026-03-10T14:00:00-05:00", sourceZone, _timeService, out var withOffsetInstant);
        var withoutOffsetOk = BatchTimestampInterpreter.TryInterpret(
            "2026-03-10 14:00:00", sourceZone, _timeService, out var withoutOffsetInstant);

        withOffsetOk.Should().BeTrue();
        withoutOffsetOk.Should().BeTrue();

        // The offset form is anchored to -05:00 regardless of the source zone's own offset for that date,
        // while the no-offset form is anchored to the source zone's own (possibly different) offset.
        withOffsetInstant.Should().NotBe(withoutOffsetInstant);
        withOffsetInstant.Should().Be(Instant.FromUtc(2026, 3, 10, 19, 0, 0));
    }

    [Fact]
    public void UnparseableTimestamp_ReturnsFalse()
    {
        var sourceZone = DateTimeZoneProviders.Tzdb["UTC"];

        var ok = BatchTimestampInterpreter.TryInterpret("not-a-date", sourceZone, _timeService, out _);

        ok.Should().BeFalse();
    }

    [Fact]
    public void EmptyTimestamp_ReturnsFalse()
    {
        var sourceZone = DateTimeZoneProviders.Tzdb["UTC"];

        var ok = BatchTimestampInterpreter.TryInterpret("", sourceZone, _timeService, out _);

        ok.Should().BeFalse();
    }
}
