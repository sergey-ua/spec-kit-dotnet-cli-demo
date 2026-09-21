using FluentAssertions;
using NodaTime;
using NSubstitute;
using TimezoneUtility.Models;
using TimezoneUtility.Services.BatchConversion;
using TimezoneUtility.Services.TimeConversion;

namespace TimezoneUtility.Unit.Services.BatchConversion;

/// <summary>
/// T024: Unit tests for BatchRowProcessor fault isolation.
/// Verifies REQ-006, REQ-011, REQ-NF-001.
/// Corresponds to STP-005-C, STP-006-A, STP-006-B (STS-005-C1, STS-006-A1, STS-006-B1).
/// </summary>
public class BatchRowProcessorTests
{
    private static BatchConversionRow ValidRow(int rowNumber) => new()
    {
        RowNumber = rowNumber,
        RawTimestamp = "2026-01-01 00:00:00",
        RawSourceTimezone = "UTC",
        RawTargetTimezone = "UTC"
    };

    private static BatchConversionRow InvalidRow(int rowNumber) => new()
    {
        RowNumber = rowNumber,
        RawTimestamp = "2026-01-01 00:00:00",
        RawSourceTimezone = "Not/AZone",
        RawTargetTimezone = "UTC"
    };

    [Fact]
    public void STS_006_A1_ConversionThrows_RowIsRoutedToInvalid_OtherRowsUnaffected()
    {
        var timeService = new TimeService();
        var outcome = BatchRowProcessor.Process(InvalidRow(1), timeService);

        outcome.Success.Should().BeNull();
        outcome.Failure.Should().NotBeNull();
    }

    [Fact]
    public void STS_005_C1_UnderlyingConversionEngineThrows_RowIsIsolatedAsFailure()
    {
        var faultyTimeService = Substitute.For<ITimeService>();
        faultyTimeService
            .ConvertTime(Arg.Any<LocalDateTime>(), Arg.Any<DateTimeZone>(), Arg.Any<DateTimeZone>())
            .Returns(_ => throw new InvalidOperationException("simulated conversion engine failure"));

        var outcome = BatchRowProcessor.Process(ValidRow(1), faultyTimeService);

        outcome.Success.Should().BeNull();
        outcome.Failure.Should().NotBeNull();
        outcome.Failure!.RowNumber.Should().Be(1);
    }

    [Fact]
    public void REQ_006_REQ_011_REQ_NF_001_MixedBatch_4Valid16Invalid_AllValidRowsSucceed()
    {
        var timeService = new TimeService();
        var rows = new List<BatchConversionRow>();
        for (var i = 1; i <= 20; i++)
        {
            rows.Add(i % 5 == 0 ? ValidRow(i) : InvalidRow(i));
        }

        var outcomes = rows.Select(r => BatchRowProcessor.Process(r, timeService)).ToList();

        var successes = outcomes.Where(o => o.Success is not null).ToList();
        var failures = outcomes.Where(o => o.Failure is not null).ToList();

        successes.Should().HaveCount(4);
        failures.Should().HaveCount(16);
        successes.Select(o => o.Success!.RowNumber).Should().BeEquivalentTo([5, 10, 15, 20]);
    }
}
