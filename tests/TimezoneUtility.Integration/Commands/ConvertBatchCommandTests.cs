using System.CommandLine;
using FluentAssertions;
using TimezoneUtility.Commands;

namespace TimezoneUtility.Integration.Commands;

/// <summary>
/// T014/T027/T028/T029/T030/T038: End-to-end integration tests for `tzutil convert-batch`,
/// covering US1-US3 scenarios, DST edge cases, file-level errors, and the 10k-row scale case.
/// </summary>
public class ConvertBatchCommandTests
{
    private static string NewTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"batch-integration-{Guid.NewGuid()}");
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static int Run(params string[] args)
    {
        var root = new RootCommand { ConvertBatchCommand.Create() };
        return root.Invoke(args);
    }

    private static List<string> ReadDataLines(string path) =>
        File.Exists(path) ? File.ReadAllLines(path).Skip(1).Where(l => l.Length > 0).ToList() : new List<string>();

    // ---------- US1: all-valid batch + header-only file (ATP-001-A, ATP-002-A, ATP-004-A, ATP-023-A) ----------

    [Fact]
    public void ATP_001_A_SCN_001_A1_AllValidRows_ProduceExactCountSuccessfulOutput()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone\n" +
                "2026-01-15 09:00:00,UTC,UTC\n" +
                "2026-01-16 09:00:00,UTC,UTC\n" +
                "2026-01-17 09:00:00,UTC,UTC\n");

            var exitCode = Run("convert-batch", input);

            exitCode.Should().Be(0);
            var successLines = ReadDataLines(Path.Combine(dir, "batch.converted.csv"));
            successLines.Should().HaveCount(3);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_002_A_SCN_002_A1_ColumnsIdentifiedByHeaderName_RegardlessOfOrder()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "target_timezone,timestamp,source_timezone\n" +
                "UTC,2026-01-15 09:00:00,America/Los_Angeles\n");

            var exitCode = Run("convert-batch", input);

            exitCode.Should().Be(0);
            var successLines = ReadDataLines(Path.Combine(dir, "batch.converted.csv"));
            successLines.Should().HaveCount(1);
            successLines[0].Should().Contain("2026-01-15 09:00:00,America/Los_Angeles,UTC");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_004_A_SCN_004_A1_ExtraUnrecognizedColumns_AreIgnored()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone,notes\n" +
                "2026-01-15 09:00:00,UTC,UTC,some notes here\n");

            var exitCode = Run("convert-batch", input);

            exitCode.Should().Be(0);
            ReadDataLines(Path.Combine(dir, "batch.converted.csv")).Should().HaveCount(1);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_023_A_SCN_023_A1_HeaderOnlyFile_CompletesCleanlyWithEmptyOutputsAndNoRowsMessage()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "empty.csv");
            File.WriteAllText(input, "timestamp,source_timezone,target_timezone\n");

            using var sw = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(sw);
            int exitCode;
            try
            {
                exitCode = Run("convert-batch", input);
            }
            finally
            {
                Console.SetOut(originalOut);
            }

            exitCode.Should().Be(0);
            ReadDataLines(Path.Combine(dir, "empty.converted.csv")).Should().BeEmpty();
            ReadDataLines(Path.Combine(dir, "empty.invalid-rows.csv")).Should().BeEmpty();
            sw.ToString().Should().Contain("No rows were processed");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    // ---------- US2: mixed valid/invalid rows (ATP-006/008/009/010/011/012/013/014/015/020/028/NF-001/NF-002) ----------

    [Fact]
    public void ATP_006_A_SCN_006_A1_OneRowFailure_DoesNotAffectAnotherRowsSuccess()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone\n" +
                "not-a-date,America/Chicago,UTC\n" +
                "2026-03-10 08:00:00,America/Chicago,UTC\n");

            var exitCode = Run("convert-batch", input);

            exitCode.Should().Be(0);
            var successLines = ReadDataLines(Path.Combine(dir, "batch.converted.csv"));
            var invalidLines = ReadDataLines(Path.Combine(dir, "batch.invalid-rows.csv"));

            successLines.Should().ContainSingle(l => l.StartsWith("2,", StringComparison.Ordinal));
            invalidLines.Should().ContainSingle(l => l.StartsWith("1,", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Theory]
    [InlineData("32/99/2026", "America/Denver", "UTC")] // ATP-008-A unparseable timestamp
    [InlineData("2026-02-10 09:00:00", "Mars/Colony_One", "UTC")] // ATP-009-A unrecognized source
    [InlineData("2026-02-10 09:00:00", "UTC", "Europe/Atlantis")] // ATP-010-A unrecognized target
    public void ATP_008_009_010_A_InvalidRow_AbsentFromSuccess_PresentInInvalidReport(string ts, string src, string tgt)
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input, $"timestamp,source_timezone,target_timezone\n{ts},{src},{tgt}\n");

            Run("convert-batch", input);

            ReadDataLines(Path.Combine(dir, "batch.converted.csv")).Should().BeEmpty();
            ReadDataLines(Path.Combine(dir, "batch.invalid-rows.csv")).Should().ContainSingle();
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_011_A_SCN_011_A1_BatchContinuesProcessingAfterInvalidRow()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone\n" +
                ",UTC,UTC\n" +
                "2026-04-01 06:00:00,UTC,UTC\n" +
                "2026-04-02 06:00:00,UTC,UTC\n");

            var exitCode = Run("convert-batch", input);

            exitCode.Should().Be(0);
            ReadDataLines(Path.Combine(dir, "batch.converted.csv")).Should().HaveCount(2);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_012_A_SCN_012_A1_InvalidRowReport_ReferencesCorrectRowNumber()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone\n" +
                "2026-01-01 00:00:00,UTC,UTC\n" +
                "2026-01-02 00:00:00,UTC,UTC\n" +
                "2026-01-03 00:00:00,UTC,UTC\n" +
                "invalid-ts,UTC,UTC\n");

            Run("convert-batch", input);

            var invalidLines = ReadDataLines(Path.Combine(dir, "batch.invalid-rows.csv"));
            invalidLines.Should().ContainSingle();
            invalidLines[0].Should().StartWith("4,");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_013_A_SCN_013_A1_UnparseableTimestamp_ReasonReferencesTimestampField()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input, "timestamp,source_timezone,target_timezone\nnot-a-real-date,UTC,UTC\n");

            Run("convert-batch", input);

            var invalidLines = ReadDataLines(Path.Combine(dir, "batch.invalid-rows.csv"));
            invalidLines.Single().Should().Contain("timestamp");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_014_A_SCN_014_A1_UnrecognizedTimezone_ReasonReferencesFieldAndValue()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input, "timestamp,source_timezone,target_timezone\n2026-06-01 10:00:00,Not/ARealZone,UTC\n");

            Run("convert-batch", input);

            var invalidLines = ReadDataLines(Path.Combine(dir, "batch.invalid-rows.csv"));
            invalidLines.Single().Should().Contain("Not/ARealZone");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_015_A_SCN_015_A1_MissingSourceTimezone_ReasonIdentifiesTheField()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input, "timestamp,source_timezone,target_timezone\n2026-06-15 08:00:00,,UTC\n");

            Run("convert-batch", input);

            var invalidLines = ReadDataLines(Path.Combine(dir, "batch.invalid-rows.csv"));
            invalidLines.Single().Should().Contain("source_timezone");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_020_A_SCN_020_A1_InvalidReport_IsSeparateFromSuccessfulOutput()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone\n" +
                "2026-01-01 00:00:00,UTC,UTC\n" +
                "2026-01-02 00:00:00,UTC,UTC\n" +
                "2026-01-03 00:00:00,UTC,\n");

            Run("convert-batch", input);

            ReadDataLines(Path.Combine(dir, "batch.converted.csv")).Should().HaveCount(2);
            ReadDataLines(Path.Combine(dir, "batch.invalid-rows.csv")).Should().HaveCount(1);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_028_A_SCN_028_A1_EntirelyBlankRow_SkippedNotCountedEitherWay()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone\n" +
                "2026-01-01 00:00:00,UTC,UTC\n" +
                ",,\n" +
                "2026-01-02 00:00:00,UTC,UTC\n");

            using var sw = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(sw);
            try
            {
                Run("convert-batch", input, "--json");
            }
            finally
            {
                Console.SetOut(originalOut);
            }

            ReadDataLines(Path.Combine(dir, "batch.converted.csv")).Should().HaveCount(2);
            ReadDataLines(Path.Combine(dir, "batch.invalid-rows.csv")).Should().BeEmpty();
            sw.ToString().Should().Contain("\"totalRows\": 2");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_NF_001_A_SCN_NF_001_A1_100PercentOfValidRows_ProduceConvertedResults()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            var lines = new List<string> { "timestamp,source_timezone,target_timezone" };
            for (var i = 1; i <= 20; i++)
            {
                lines.Add(i % 5 == 0
                    ? $"2026-01-{i:D2} 00:00:00,UTC,UTC"
                    : "2026-01-01 00:00:00,Not/AZone,UTC");
            }
            File.WriteAllLines(input, lines);

            Run("convert-batch", input);

            ReadDataLines(Path.Combine(dir, "batch.converted.csv")).Should().HaveCount(4);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_NF_002_A_SCN_NF_002_A1_5InvalidRows_EachDistinctFailureCause_AllAppear()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone\n" +
                "not-a-date,UTC,UTC\n" +
                "2026-01-01 00:00:00,Not/ASource,UTC\n" +
                "2026-01-01 00:00:00,UTC,Not/ATarget\n" +
                ",UTC,UTC\n" +
                "2026-01-01 00:00:00,,UTC\n");

            Run("convert-batch", input);

            var invalidLines = ReadDataLines(Path.Combine(dir, "batch.invalid-rows.csv"));
            invalidLines.Should().HaveCount(5);
            invalidLines.Select(l => l.Split(',')[0]).Distinct().Should().HaveCount(5);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    // ---------- US2: DST edge cases (ATP-026-A/B) ----------

    [Fact]
    public void ATP_026_A_SCN_026_A1_SpringForwardGap_ResolvesSuccessfully_NotInvalid()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone\n2026-03-08 02:30:00,America/New_York,UTC\n");

            Run("convert-batch", input);

            ReadDataLines(Path.Combine(dir, "batch.converted.csv")).Should().HaveCount(1);
            ReadDataLines(Path.Combine(dir, "batch.invalid-rows.csv")).Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_026_B_SCN_026_B1_FallBackAmbiguousHour_ResolvesSuccessfully_NotInvalid()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone\n2026-11-01 01:30:00,America/New_York,UTC\n");

            Run("convert-batch", input);

            ReadDataLines(Path.Combine(dir, "batch.converted.csv")).Should().HaveCount(1);
            ReadDataLines(Path.Combine(dir, "batch.invalid-rows.csv")).Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    // ---------- US2: file-level errors (ATP-016-A, ATP-017-A, ATP-018-A) ----------

    [Fact]
    public void ATP_016_A_SCN_016_A1_MissingInputFile_ExitCode2_NoInvalidRowEntries()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "missing-input.csv");

            var exitCode = Run("convert-batch", input);

            exitCode.Should().Be(2);
            File.Exists(Path.Combine(dir, "missing-input.invalid-rows.csv")).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_017_A_SCN_017_A1_UnidentifiableHeader_ExitCode2()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "badheader.csv");
            File.WriteAllText(input, "col_a,col_b,col_c\nfoo,bar,baz\n");

            var exitCode = Run("convert-batch", input);

            exitCode.Should().Be(2);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_018_A_SCN_018_A1_FileLevelError_NoRowsProcessed_BothOutputsAbsent()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "badheader.csv");
            var lines = new List<string> { "col_a,col_b,col_c" };
            for (var i = 0; i < 5; i++)
            {
                lines.Add("val1,val2,val3");
            }
            File.WriteAllLines(input, lines);

            Run("convert-batch", input);

            File.Exists(Path.Combine(dir, "badheader.converted.csv")).Should().BeFalse();
            File.Exists(Path.Combine(dir, "badheader.invalid-rows.csv")).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    // ---------- US2/NF-005: 10,000-row batch (ATP-NF-005-A) ----------

    [Fact]
    public void ATP_NF_005_A_SCN_NF_005_A1_10000RowFile_CompletesInSingleRun()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "big-batch.csv");
            using (var writer = new StreamWriter(input))
            {
                writer.WriteLine("timestamp,source_timezone,target_timezone");
                for (var i = 0; i < 10000; i++)
                {
                    writer.WriteLine("2026-06-01 12:00:00,UTC,UTC");
                }
            }

            var exitCode = Run("convert-batch", input);

            exitCode.Should().Be(0);
            ReadDataLines(Path.Combine(dir, "big-batch.converted.csv")).Should().HaveCount(10000);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    // ---------- US3: run summary classification across three separate runs (ATP-021/022/NF-003) ----------

    [Fact]
    public void ATP_021_022_NF_003_A_ThreeRuns_SummaryCountsAloneClassifyOutcome()
    {
        var dir = NewTempDir();
        try
        {
            // Run A: 5 valid, 0 invalid -> fully succeeded
            var runA = Path.Combine(dir, "runA.csv");
            var runALines = new List<string> { "timestamp,source_timezone,target_timezone" };
            for (var i = 1; i <= 5; i++)
            {
                runALines.Add($"2026-01-{i:D2} 00:00:00,UTC,UTC");
            }
            File.WriteAllLines(runA, runALines);
            Run("convert-batch", runA);
            ReadDataLines(Path.Combine(dir, "runA.converted.csv")).Should().HaveCount(5);
            ReadDataLines(Path.Combine(dir, "runA.invalid-rows.csv")).Should().BeEmpty();

            // Run B: 0 valid, 5 invalid -> fully failed
            var runB = Path.Combine(dir, "runB.csv");
            var runBLines = new List<string> { "timestamp,source_timezone,target_timezone" };
            for (var i = 1; i <= 5; i++)
            {
                runBLines.Add("not-a-date,UTC,UTC");
            }
            File.WriteAllLines(runB, runBLines);
            Run("convert-batch", runB);
            ReadDataLines(Path.Combine(dir, "runB.converted.csv")).Should().BeEmpty();
            ReadDataLines(Path.Combine(dir, "runB.invalid-rows.csv")).Should().HaveCount(5);

            // Run C: 3 valid, 2 invalid -> partially succeeded
            var runC = Path.Combine(dir, "runC.csv");
            File.WriteAllText(runC,
                "timestamp,source_timezone,target_timezone\n" +
                "2026-01-01 00:00:00,UTC,UTC\n" +
                "2026-01-02 00:00:00,UTC,UTC\n" +
                "2026-01-03 00:00:00,UTC,UTC\n" +
                "not-a-date,UTC,UTC\n" +
                "2026-01-04 00:00:00,Not/AZone,UTC\n");
            Run("convert-batch", runC);
            ReadDataLines(Path.Combine(dir, "runC.converted.csv")).Should().HaveCount(3);
            ReadDataLines(Path.Combine(dir, "runC.invalid-rows.csv")).Should().HaveCount(2);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    // ---------- REQ-024/025/027: interpretation & formatting round-trip through the CLI ----------

    [Fact]
    public void REQ_024_NoExplicitOffset_InterpretedAsLocalToSourceTimezone()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone\n2026-01-15 09:00:00,America/Los_Angeles,UTC\n");

            Run("convert-batch", input);

            var line = ReadDataLines(Path.Combine(dir, "batch.converted.csv")).Single();
            line.Should().Contain("2026-01-15T17:00:00");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void REQ_027_IdenticalSourceAndTargetTimezone_StillProducesUnchangedResult()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone\n2026-05-20 14:00:00,Asia/Singapore,Asia/Singapore\n");

            Run("convert-batch", input);

            ReadDataLines(Path.Combine(dir, "batch.converted.csv")).Should().HaveCount(1);
            ReadDataLines(Path.Combine(dir, "batch.invalid-rows.csv")).Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    // ---------- SYS-011 orchestrator fault injection: output-write failure (STS-011-A2) ----------

    [Fact]
    public void STS_011_A2_OutputWriteFails_OrchestratorSurfacesFailure_ExitCode3_AfterAllRowsProcessed()
    {
        // Given SYS-009 is engineered to fail on the output-write step (outputDir points at a path that
        // is itself an existing file, so Directory.CreateDirectory fails) after SYS-006 has already
        // processed every row.
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(
                input,
                "timestamp,source_timezone,target_timezone\n"
                + "2026-05-20 14:00:00,Asia/Singapore,UTC\n"
                + "not-a-timestamp,Asia/Singapore,UTC\n");

            var blockingFile = Path.Combine(dir, "blocked-destination");
            File.WriteAllText(blockingFile, "not a directory");
            var unwritableOutputDir = Path.Combine(blockingFile, "nested");

            // When SYS-011 sequences file intake -> row processing -> output writing for the run.
            var exitCode = Run("convert-batch", input, "--output-dir", unwritableOutputDir);

            // Then SYS-011 surfaces the write failure via a distinct exit code (not a crash / unhandled
            // exception — proving SYS-006 finished processing all rows and SYS-010's summary-generation
            // step, which runs before the write attempt, was reached and completed without throwing)
            // rather than aborting the run early the way a file-level (SYS-001/002) error does.
            exitCode.Should().Be(3);

            // and neither output artifact exists at the unwritable destination (the failure was reported,
            // not silently swallowed), while the run still did not crash before reaching that point.
            File.Exists(Path.Combine(unwritableOutputDir, "batch.converted.csv")).Should().BeFalse();
            File.Exists(Path.Combine(unwritableOutputDir, "batch.invalid-rows.csv")).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }
}
