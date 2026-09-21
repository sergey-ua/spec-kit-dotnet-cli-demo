using FluentAssertions;
using NodaTime;
using TimezoneUtility.Models;
using TimezoneUtility.Services.BatchConversion;
using TimezoneUtility.Services.TimeConversion;

namespace TimezoneUtility.Unit.Services.BatchConversion;

/// <summary>
/// Unit tests for BatchRowsOrchestrator (part of SYS-011): the SYS-011 -&gt; SYS-006 dependency-edge
/// failure-isolation guarantee. Verifies REQ-006, REQ-011, REQ-NF-001.
/// Corresponds to STP-006-B (STS-006-B1).
/// </summary>
public class BatchRowsOrchestratorTests
{
    private static BatchConversionRow Row(int rowNumber) => new()
    {
        RowNumber = rowNumber,
        RawTimestamp = "2026-01-01 00:00:00",
        RawSourceTimezone = "UTC",
        RawTargetTimezone = "UTC"
    };

    [Fact]
    public void STS_006_B1_UnhandledExceptionFromRowTwo_OrchestratorContinuesToRowsThreeAndFour()
    {
        var rows = new List<BatchConversionRow> { Row(1), Row(2), Row(3), Row(4) };
        var timeService = new TimeService();
        var invokedRows = new List<int>();

        Func<BatchConversionRow, TimezoneUtility.Services.TimeConversion.ITimeService, BatchRowOutcome> processRow =
            (row, ts) =>
            {
                invokedRows.Add(row.RowNumber);
                if (row.RowNumber == 2)
                {
                    // Simulate SYS-006 raising an unhandled (non-isolated) exception for this row -
                    // i.e. a fault that escapes BatchRowProcessor's own internal try/catch entirely.
                    throw new InvalidOperationException("simulated non-isolated SYS-006 fault");
                }

                return new BatchRowOutcome
                {
                    Success = new RowConversionResult
                    {
                        RowNumber = row.RowNumber,
                        OriginalTimestamp = row.RawTimestamp,
                        OriginalSourceTimezone = row.RawSourceTimezone,
                        OriginalTargetTimezone = row.RawTargetTimezone,
                        ConvertedInstant = Instant.FromUtc(2026, 1, 1, 0, 0, 0),
                        TargetZone = DateTimeZone.Utc
                    }
                };
            };

        var (successes, failures) = BatchRowsOrchestrator.ProcessAll(rows, timeService, processRow);

        // SYS-011 must still have invoked SYS-006 for rows 3 and 4 after row 2's unhandled exception.
        invokedRows.Should().Equal(1, 2, 3, 4);

        // Rows 1, 3, and 4 each reach a terminal (success) state; row 2's fault is isolated as a failure
        // rather than aborting the run.
        successes.Select(s => s.RowNumber).Should().Equal(1, 3, 4);
        failures.Select(f => f.RowNumber).Should().Equal(2);
    }
}
