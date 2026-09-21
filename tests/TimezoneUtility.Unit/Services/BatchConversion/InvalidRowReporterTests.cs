using FluentAssertions;
using TimezoneUtility.Models;
using TimezoneUtility.Services.BatchConversion;

namespace TimezoneUtility.Unit.Services.BatchConversion;

/// <summary>
/// T023: Unit tests for InvalidRowReporter.
/// Verifies REQ-012, REQ-013, REQ-014, REQ-015, REQ-NF-002.
/// Corresponds to STP-007-A, STP-007-B (STS-007-A1, STS-007-B1).
/// </summary>
public class InvalidRowReporterTests
{
    [Fact]
    public void STS_007_A1_ReportedEntry_ContainsRowNumberAndReason()
    {
        var row = new BatchConversionRow
        {
            RowNumber = 7,
            RawTimestamp = "not-a-date",
            RawSourceTimezone = "UTC",
            RawTargetTimezone = "UTC"
        };

        var entry = InvalidRowReporter.Report(row, "unparseable timestamp: not-a-date");

        entry.RowNumber.Should().Be(7);
        entry.Reason.Should().Be("unparseable timestamp: not-a-date");
        entry.RawTimestamp.Should().Be("not-a-date");
    }

    [Fact]
    public void STS_007_B1_50Of200Rows_EachProducesDistinctEntry_ZeroOmitted()
    {
        var entries = new List<InvalidRowRecord>();
        for (var i = 1; i <= 200; i++)
        {
            if (i % 4 != 0)
            {
                continue; // exactly 50 of 200 fail
            }

            var row = new BatchConversionRow
            {
                RowNumber = i,
                RawTimestamp = "bad",
                RawSourceTimezone = "UTC",
                RawTargetTimezone = "UTC"
            };
            entries.Add(InvalidRowReporter.Report(row, $"unparseable timestamp: bad (row {i})"));
        }

        entries.Should().HaveCount(50);
        entries.Select(e => e.RowNumber).Distinct().Should().HaveCount(50);
    }

    [Fact]
    public void REQ_014_UnrecognizedTimezoneReason_ContainsTheOffendingValue()
    {
        var row = new BatchConversionRow
        {
            RowNumber = 1,
            RawTimestamp = "2026-01-01 00:00:00",
            RawSourceTimezone = "Not/ARealZone",
            RawTargetTimezone = "UTC"
        };

        var entry = InvalidRowReporter.Report(row, "unrecognized timezone: Not/ARealZone");

        entry.Reason.Should().Contain("timezone");
        entry.Reason.Should().Contain("Not/ARealZone");
    }
}
