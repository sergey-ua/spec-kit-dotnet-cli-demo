using FluentAssertions;
using NodaTime;
using TimezoneUtility.Models;
using TimezoneUtility.Services.BatchConversion;

namespace TimezoneUtility.Unit.Services.BatchConversion;

/// <summary>
/// T011: Unit tests for SuccessfulResultFormatter.
/// Verifies REQ-005, REQ-007, REQ-025, REQ-027.
/// Corresponds to STP-005-A, STP-008-A, STP-008-B (STS-005-A1, STS-005-A2, STS-008-A1, STS-008-B1).
/// </summary>
public class SuccessfulResultFormatterTests
{
    [Fact]
    public void STS_008_A1_Result_ContainsOriginalValuesAndConvertedFields()
    {
        var row = new BatchConversionRow
        {
            RowNumber = 4,
            RawTimestamp = "2026-05-01 10:00:00",
            RawSourceTimezone = "Europe/London",
            RawTargetTimezone = "Asia/Kolkata"
        };
        var targetZone = DateTimeZoneProviders.Tzdb["Asia/Kolkata"];
        var instant = Instant.FromUtc(2026, 5, 1, 9, 0, 0);

        var result = SuccessfulResultFormatter.Format(row, targetZone, instant);

        result.RowNumber.Should().Be(4);
        result.OriginalTimestamp.Should().Be("2026-05-01 10:00:00");
        result.OriginalSourceTimezone.Should().Be("Europe/London");
        result.OriginalTargetTimezone.Should().Be("Asia/Kolkata");
        result.TargetZone.Id.Should().Be("Asia/Kolkata");
        result.ConvertedLocalDateTime.Should().Be(instant.InZone(targetZone).LocalDateTime);
    }

    [Fact]
    public void STS_005_A1_DateBoundaryCrossing_IsCapturedInConvertedResult()
    {
        var row = new BatchConversionRow
        {
            RowNumber = 1,
            RawTimestamp = "2026-01-15 23:30:00",
            RawSourceTimezone = "America/Los_Angeles",
            RawTargetTimezone = "Asia/Tokyo"
        };
        var sourceZone = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
        var targetZone = DateTimeZoneProviders.Tzdb["Asia/Tokyo"];
        var instant = new LocalDateTime(2026, 1, 15, 23, 30, 0).InZoneLeniently(sourceZone).ToInstant();

        var result = SuccessfulResultFormatter.Format(row, targetZone, instant);

        result.ConvertedLocalDateTime.Date.Should().Be(new LocalDate(2026, 1, 16));
    }

    [Fact]
    public void STS_005_A2_SameTimezone_NoOp_ProducesUnchangedTimestamp()
    {
        var row = new BatchConversionRow
        {
            RowNumber = 1,
            RawTimestamp = "2026-05-20 14:00:00",
            RawSourceTimezone = "Asia/Singapore",
            RawTargetTimezone = "Asia/Singapore"
        };
        var zone = DateTimeZoneProviders.Tzdb["Asia/Singapore"];
        var instant = new LocalDateTime(2026, 5, 20, 14, 0, 0).InZoneLeniently(zone).ToInstant();

        var result = SuccessfulResultFormatter.Format(row, zone, instant);

        result.ConvertedLocalDateTime.Should().Be(new LocalDateTime(2026, 5, 20, 14, 0, 0));
    }
}
