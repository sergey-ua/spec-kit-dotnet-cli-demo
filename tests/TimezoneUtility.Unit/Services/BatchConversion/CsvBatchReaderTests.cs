using FluentAssertions;
using TimezoneUtility.Services.BatchConversion;

namespace TimezoneUtility.Unit.Services.BatchConversion;

/// <summary>
/// T009: Unit tests for CsvBatchReader header/column resolution.
/// Verifies REQ-001, REQ-002, REQ-004, REQ-016, REQ-017, REQ-018.
/// Corresponds to STP-001-A, STP-001-B, STP-002-A, STP-002-B
/// (STS-001-A1, STS-001-A2, STS-001-B1, STS-002-A1, STS-002-A2, STS-002-B1).
/// </summary>
public class CsvBatchReaderTests
{
    private static string WriteTempCsv(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"csv-reader-test-{Guid.NewGuid()}.csv");
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void STS_001_A1_MissingFile_ReturnsFileLevelError()
    {
        var path = Path.Combine(Path.GetTempPath(), $"does-not-exist-{Guid.NewGuid()}.csv");

        var result = CsvBatchReader.Read(path);

        result.IsSuccess.Should().BeFalse();
        result.FileLevelError.Should().Contain("file not found");
        result.Rows.Should().BeEmpty();
    }

    [Fact]
    public void STS_002_A1_HeaderColumnsResolvedByName_OrderIndependent_ExtraColumnIgnored()
    {
        var csv = "target_timezone,notes,timestamp,source_timezone\n" +
                   "UTC,ignore-me,2026-01-15 09:00:00,America/Los_Angeles\n";
        var path = WriteTempCsv(csv);

        try
        {
            var result = CsvBatchReader.Read(path);

            result.IsSuccess.Should().BeTrue();
            result.Rows.Should().HaveCount(1);
            result.Rows[0].RawTimestamp.Should().Be("2026-01-15 09:00:00");
            result.Rows[0].RawSourceTimezone.Should().Be("America/Los_Angeles");
            result.Rows[0].RawTargetTimezone.Should().Be("UTC");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void STS_002_A2_UnresolvableHeader_ReturnsFileLevelError_NoRowsProcessed()
    {
        var csv = "col_a,col_b,col_c\nfoo,bar,baz\n";
        var path = WriteTempCsv(csv);

        try
        {
            var result = CsvBatchReader.Read(path);

            result.IsSuccess.Should().BeFalse();
            result.FileLevelError.Should().Contain("Could not identify required columns");
            result.Rows.Should().BeEmpty();
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void STS_002_B1_HeaderOnlyFile_ResolvesSuccessfullyWithZeroRows()
    {
        var csv = "timestamp,source_timezone,target_timezone\n";
        var path = WriteTempCsv(csv);

        try
        {
            var result = CsvBatchReader.Read(path);

            result.IsSuccess.Should().BeTrue();
            result.Rows.Should().BeEmpty();
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void REQ_028_EntirelyBlankRow_IsSkipped_NotCountedAsARow()
    {
        var csv = "timestamp,source_timezone,target_timezone\n" +
                   "2026-01-01 00:00:00,UTC,UTC\n" +
                   ",,\n" +
                   "2026-01-02 00:00:00,UTC,UTC\n";
        var path = WriteTempCsv(csv);

        try
        {
            var result = CsvBatchReader.Read(path);

            result.IsSuccess.Should().BeTrue();
            result.Rows.Should().HaveCount(2);
            result.Rows[0].RowNumber.Should().Be(1);
            result.Rows[1].RowNumber.Should().Be(2);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void REQ_002_HeaderNames_AreCaseInsensitive()
    {
        var csv = "TIMESTAMP,Source_Timezone,TARGET_TIMEZONE\n" +
                   "2026-01-01 00:00:00,UTC,UTC\n";
        var path = WriteTempCsv(csv);

        try
        {
            var result = CsvBatchReader.Read(path);

            result.IsSuccess.Should().BeTrue();
            result.Rows.Should().HaveCount(1);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
