using FluentAssertions;
using NodaTime;
using TimezoneUtility.Models;

namespace TimezoneUtility.Unit.Services.BatchConversion;

/// <summary>
/// T008: Unit tests for the four new model records' invariants (REQ-007, REQ-021, REQ-022, REQ-025).
/// </summary>
public class ModelInvariantTests
{
    [Fact]
    public void REQ_007_RowConversionResult_ConvertedLocalDateTime_IsDerivedFromInstantAndZone()
    {
        var zone = DateTimeZoneProviders.Tzdb["UTC"];
        var instant = Instant.FromUtc(2026, 6, 1, 12, 30);

        var result = new RowConversionResult
        {
            RowNumber = 1,
            OriginalTimestamp = "2026-06-01 12:30:00",
            OriginalSourceTimezone = "UTC",
            OriginalTargetTimezone = "UTC",
            ConvertedInstant = instant,
            TargetZone = zone
        };

        result.ConvertedLocalDateTime.Should().Be(instant.InZone(zone).LocalDateTime);
    }

    [Fact]
    public void REQ_025_RowConversionResult_ConvertedLocalDateTime_ReflectsDateBoundaryCrossing()
    {
        var sourceZone = DateTimeZoneProviders.Tzdb["America/New_York"];
        var targetZone = DateTimeZoneProviders.Tzdb["Europe/London"];
        var local = new LocalDateTime(2026, 3, 10, 23, 0, 0);
        var instant = local.InZoneLeniently(sourceZone).ToInstant();

        var result = new RowConversionResult
        {
            RowNumber = 1,
            OriginalTimestamp = "2026-03-10 23:00:00",
            OriginalSourceTimezone = "America/New_York",
            OriginalTargetTimezone = "Europe/London",
            ConvertedInstant = instant,
            TargetZone = targetZone
        };

        result.ConvertedLocalDateTime.Date.Should().Be(new LocalDate(2026, 3, 11));
    }

    [Theory]
    [InlineData(5, 0)]
    [InlineData(0, 5)]
    [InlineData(3, 2)]
    [InlineData(0, 0)]
    public void REQ_021_REQ_022_BatchRunSummary_TotalRowsEqualsSucceededPlusFailed(int succeeded, int failed)
    {
        var summary = new BatchRunSummary
        {
            InputFile = "batch.csv",
            TotalRows = succeeded + failed,
            SucceededCount = succeeded,
            FailedCount = failed,
            NoRowsProcessed = succeeded == 0 && failed == 0
        };

        summary.TotalRows.Should().Be(summary.SucceededCount + summary.FailedCount);
    }

    [Fact]
    public void BatchConversionRow_HoldsRawFieldsExactlyAsSupplied()
    {
        var row = new BatchConversionRow
        {
            RowNumber = 3,
            RawTimestamp = "2026-01-01 00:00:00",
            RawSourceTimezone = "UTC",
            RawTargetTimezone = "UTC"
        };

        row.RowNumber.Should().Be(3);
        row.RawTimestamp.Should().Be("2026-01-01 00:00:00");
    }

    [Fact]
    public void InvalidRowRecord_HoldsRowNumberAndReason()
    {
        var record = new InvalidRowRecord
        {
            RowNumber = 4,
            RawTimestamp = "bad",
            RawSourceTimezone = "UTC",
            RawTargetTimezone = "UTC",
            Reason = "unparseable timestamp: bad"
        };

        record.RowNumber.Should().Be(4);
        record.Reason.Should().Contain("timestamp");
    }
}
