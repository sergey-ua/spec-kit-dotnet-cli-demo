using FluentAssertions;
using TimezoneUtility.Services.BatchConversion;

namespace TimezoneUtility.Unit.Services.BatchConversion;

/// <summary>
/// T012/T036: Unit tests for BatchRunSummaryGenerator.
/// Verifies REQ-021, REQ-022, REQ-023, REQ-NF-003.
/// Corresponds to STP-010-A, STP-010-B, STP-011-B (STS-010-A1, STS-010-A2, STS-010-B1, STS-010-B2, STS-011-B1).
/// </summary>
public class BatchRunSummaryGeneratorTests
{
    [Fact]
    public void STS_010_B1_IncompleteFailedRowCount_SummaryReflectsIncompleteCountNotTrueCount()
    {
        // Given SYS-007's recorded entry count is engineered to be incomplete (2 entries missing from an
        // expected 5, i.e. only 3 were actually recorded and handed to SYS-010).
        const int trueFailedCount = 5;
        const int sys007RecordedFailedCount = trueFailedCount - 2;

        // When SYS-010 computes the run summary by reading SYS-007's (incomplete) failed-row count.
        var summary = BatchRunSummaryGenerator.Generate(
            "batch.csv",
            successCount: 7,
            failureCount: sys007RecordedFailedCount,
            noRowsProcessed: false);

        // Then the displayed failed count reflects SYS-007's incomplete count of 3, not the true count of 5 —
        // demonstrating the documented dependency-failure-impact that SYS-010's accuracy is bound to SYS-007's
        // completeness (REQ-021, REQ-023).
        summary.FailedCount.Should().Be(3);
        summary.FailedCount.Should().NotBe(trueFailedCount);

        var rendered = BatchRunSummaryGenerator.RenderHumanReadable(summary, "batch.converted.csv", "batch.invalid-rows.csv");
        rendered.Should().Contain("Failed:                3");
    }

    [Fact]
    public void STS_010_B2_HeaderOnlyFile_ProducesNoRowsProcessedMessageNotZeroSummary()
    {
        // Given a header-only input file with zero data rows has completed the SYS-011 pipeline.
        var summary = BatchRunSummaryGenerator.Generate("empty.csv", successCount: 0, failureCount: 0, noRowsProcessed: true);

        // When SYS-010 computes the run summary,
        // Then SYS-010 produces the specific "no rows processed" message rather than a summary showing
        // total=0 succeeded=0 failed=0 without explanation.
        var rendered = BatchRunSummaryGenerator.RenderHumanReadable(summary, "empty.converted.csv", "empty.invalid-rows.csv");

        rendered.Should().Contain("No rows were processed");
        rendered.Should().NotContain("Total rows processed:");
        rendered.Should().NotContain("Succeeded:");
        rendered.Should().NotContain("Failed:");
    }

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
