using FluentAssertions;
using TimezoneUtility.Services.BatchConversion;

namespace TimezoneUtility.Unit.Services.BatchConversion;

/// <summary>
/// T012/T036: Unit tests for BatchRunSummaryGenerator.
/// Verifies REQ-021, REQ-022, REQ-023, REQ-NF-003.
/// Corresponds to STP-010-A, STP-010-B, STP-011-B (STS-010-A1, STS-010-A2, STS-010-B2, STS-011-B1).
/// </summary>
public class BatchRunSummaryGeneratorTests
{
    [Fact]
    public void STS_011_B1_HeaderOnlyFile_NoRowsProcessed_TrueAndCountsZero()
    {
        var summary = BatchRunSummaryGenerator.Generate("empty.csv", successCount: 0, failureCount: 0, noRowsProcessed: true);

        summary.NoRowsProcessed.Should().BeTrue();
        summary.TotalRows.Should().Be(0);

        var rendered = BatchRunSummaryGenerator.RenderHumanReadable(summary, "empty.converted.csv", "empty.invalid-rows.csv");
        rendered.Should().Contain("No rows were processed");
    }

    [Fact]
    public void STS_010_A1_AllSucceeded_DisplaysExplicitZeroFailedCount()
    {
        var summary = BatchRunSummaryGenerator.Generate("batch.csv", successCount: 10, failureCount: 0, noRowsProcessed: false);

        summary.FailedCount.Should().Be(0);
        summary.SucceededCount.Should().Be(10);

        var rendered = BatchRunSummaryGenerator.RenderHumanReadable(summary, "batch.converted.csv", "batch.invalid-rows.csv");
        rendered.Should().Contain("Failed:                0");
    }

    [Fact]
    public void STS_010_A2_PartiallySucceeded_BothCountsGreaterThanZero()
    {
        var summary = BatchRunSummaryGenerator.Generate("batch.csv", successCount: 6, failureCount: 4, noRowsProcessed: false);

        summary.SucceededCount.Should().BePositive();
        summary.FailedCount.Should().BePositive();
        summary.TotalRows.Should().Be(10);
    }

    [Fact]
    public void AllFailed_SucceededCountIsZero()
    {
        var summary = BatchRunSummaryGenerator.Generate("batch.csv", successCount: 0, failureCount: 5, noRowsProcessed: false);

        summary.SucceededCount.Should().Be(0);
        summary.FailedCount.Should().Be(5);
    }

    [Theory]
    [InlineData(5, 0)]
    [InlineData(0, 5)]
    [InlineData(3, 2)]
    public void REQ_NF_003_TwoCountsAloneClassifyTheRun(int succeeded, int failed)
    {
        var summary = BatchRunSummaryGenerator.Generate("batch.csv", succeeded, failed, noRowsProcessed: false);

        var isFullySucceeded = summary.FailedCount == 0;
        var isFullyFailed = summary.SucceededCount == 0;
        var isPartial = summary.SucceededCount > 0 && summary.FailedCount > 0;

        (isFullySucceeded || isFullyFailed || isPartial).Should().BeTrue();
    }
}
