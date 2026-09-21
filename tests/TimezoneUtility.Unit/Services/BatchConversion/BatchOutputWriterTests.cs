using FluentAssertions;
using NodaTime;
using TimezoneUtility.Models;
using TimezoneUtility.Services.BatchConversion;

namespace TimezoneUtility.Unit.Services.BatchConversion;

/// <summary>
/// Unit tests for BatchOutputWriter (SYS-009 Output Writer).
/// Verifies REQ-019, REQ-020, REQ-029.
/// Corresponds to STP-009-A, STP-009-B (STS-009-A1, STS-009-B1).
/// </summary>
public class BatchOutputWriterTests : IDisposable
{
    private static readonly string[] ExpectedInvalidRowNumbers = { "4", "6" };

    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), "tzutil-output-writer-tests-" + Guid.NewGuid());

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
        GC.SuppressFinalize(this);
    }

    private static RowConversionResult MakeSuccess(int rowNumber) => new()
    {
        RowNumber = rowNumber,
        OriginalTimestamp = "2026-01-15 09:00",
        OriginalSourceTimezone = "America/New_York",
        OriginalTargetTimezone = "Asia/Tokyo",
        ConvertedInstant = new LocalDateTime(2026, 1, 15, 9, 0, 0).InZoneLeniently(DateTimeZoneProviders.Tzdb["America/New_York"]).ToInstant(),
        TargetZone = DateTimeZoneProviders.Tzdb["Asia/Tokyo"]
    };

    private static InvalidRowRecord MakeFailure(int rowNumber) => new()
    {
        RowNumber = rowNumber,
        RawTimestamp = "not-a-timestamp",
        RawSourceTimezone = "America/New_York",
        RawTargetTimezone = "Asia/Tokyo",
        Reason = "unparseable timestamp: not-a-timestamp"
    };

    [Fact]
    public void STS_009_A1_TwoOutputArtifacts_ContainExactlyTheirOwnRecordsWithNoIntermixing()
    {
        Directory.CreateDirectory(_tempDir);
        var inputFile = Path.Combine(_tempDir, "batch.csv");
        var successes = new List<RowConversionResult> { MakeSuccess(2), MakeSuccess(3), MakeSuccess(5) };
        var failures = new List<InvalidRowRecord> { MakeFailure(4), MakeFailure(6) };

        var result = BatchOutputWriter.WriteAll(inputFile, outputDir: null, successes, failures);

        result.IsSuccess.Should().BeTrue();

        var successLines = File.ReadAllLines(BatchOutputWriter.SuccessfulOutputPath(inputFile, null));
        var invalidLines = File.ReadAllLines(BatchOutputWriter.InvalidRowReportPath(inputFile, null));

        // header + exactly 3 successful records, no invalid-row content mixed in.
        successLines.Should().HaveCount(4);
        successLines.Skip(1).Should().OnlyContain(l => !l.Contains("unparseable"));

        // header + exactly 2 invalid entries, no successful-record content mixed in.
        invalidLines.Should().HaveCount(3);
        invalidLines.Skip(1).Should().OnlyContain(l => !l.Contains("Asia/Tokyo") || l.StartsWith('4') || l.StartsWith('6'));
        invalidLines.Skip(1).Select(l => l.Split(',')[0]).Should().BeEquivalentTo(ExpectedInvalidRowNumbers);
    }

    [Fact]
    public void STS_009_B1_WriteDestinationUnwritable_ReportsFailure_AndInMemoryResultsRemainRetained()
    {
        // Given the output destination is engineered to reject writes: point outputDir at a path that is
        // itself an existing file, not a directory, so Directory.CreateDirectory fails with IOException —
        // simulating an unwritable destination without depending on OS-specific permission APIs.
        Directory.CreateDirectory(_tempDir);
        var blockingFile = Path.Combine(_tempDir, "blocked-destination");
        File.WriteAllText(blockingFile, "not a directory");
        var unwritableOutputDir = Path.Combine(blockingFile, "nested");

        var inputFile = Path.Combine(_tempDir, "batch.csv");
        var successes = new List<RowConversionResult> { MakeSuccess(1), MakeSuccess(2), MakeSuccess(3),
            MakeSuccess(4), MakeSuccess(5), MakeSuccess(6), MakeSuccess(7), MakeSuccess(8), MakeSuccess(9), MakeSuccess(10) };
        var failures = new List<InvalidRowRecord>();

        // When SYS-009 attempts to write the successful-output artifact.
        var result = BatchOutputWriter.WriteAll(inputFile, unwritableOutputDir, successes, failures);

        // Then SYS-009 reports the write failure with a distinct error state,
        result.IsSuccess.Should().BeFalse();
        result.WriteError.Should().NotBeNullOrEmpty();

        // and the 10 already-computed successful results remain retained in memory (not discarded) and
        // are inspectable after the failed write attempt.
        successes.Should().HaveCount(10);
        successes.Select(s => s.RowNumber).Should().BeEquivalentTo(Enumerable.Range(1, 10));
    }
}
