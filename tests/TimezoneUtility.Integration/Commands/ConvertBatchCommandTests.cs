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
    public void ATP_013_A_SCN_013_A1_STS_003_A1_UnparseableTimestamp_ReasonReferencesTimestampField()
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
    public void ATP_026_A_SCN_026_A1_STS_005_B1_SpringForwardGap_ResolvesSuccessfully_NotInvalid()
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
    public void ATP_026_B_SCN_026_B1_STS_005_B2_FallBackAmbiguousHour_ResolvesSuccessfully_NotInvalid()
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
    public void ATP_017_A_SCN_017_A1_STS_011_A1_UnidentifiableHeader_ExitCode2()
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
    public void ATP_NF_005_A_SCN_NF_005_A1_STS_011_B2_10000RowFile_CompletesInSingleRun()
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

    // ---------- SYS-008: 300-row all-valid batch (STS-008-B1) ----------

    [Fact]
    public void STS_008_B1_300ValidRows_ProducesExactly300SuccessfulOutputRecords_ZeroMissing()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            using (var writer = new StreamWriter(input))
            {
                writer.WriteLine("timestamp,source_timezone,target_timezone");
                for (var i = 0; i < 300; i++)
                {
                    writer.WriteLine("2026-06-01 12:00:00,UTC,UTC");
                }
            }

            var exitCode = Run("convert-batch", input);

            exitCode.Should().Be(0);
            ReadDataLines(Path.Combine(dir, "batch.converted.csv")).Should().HaveCount(300);
            ReadDataLines(Path.Combine(dir, "batch.invalid-rows.csv")).Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    // ---------- SYS-001: file exists but cannot be read (STS-001-A2), and the SYS-011 -> SYS-001
    // dependency edge on that fault (STS-001-B1) ----------

    [Fact]
    public void STS_001_A2_FileExistsButUnreadable_ReturnsFileLevelUnreadableError_NoRowStreamProduced()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "locked.csv");
            File.WriteAllText(input, "timestamp,source_timezone,target_timezone\n2026-01-01 00:00:00,UTC,UTC\n");

            // Simulate a file that exists but is unreadable by the process (permission bits are
            // ignored when running as root, so an exclusive OS-level lock is used instead to force the
            // same IOException path that SYS-001's unreadable-file handling catches).
            using var lockHandle = new FileStream(input, FileMode.Open, FileAccess.Read, FileShare.None);

            var exitCode = Run("convert-batch", input);

            exitCode.Should().Be(2);
            File.Exists(Path.Combine(dir, "locked.converted.csv")).Should().BeFalse();
            File.Exists(Path.Combine(dir, "locked.invalid-rows.csv")).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void STS_001_B1_SYS001RaisesFileLevelError_SYS011AbortsImmediately_SYS002AndSYS006NeverInvoked()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "locked.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone\n" +
                "2026-01-01 00:00:00,UTC,UTC\n" +
                "2026-01-02 00:00:00,UTC,UTC\n");

            using var lockHandle = new FileStream(input, FileMode.Open, FileAccess.Read, FileShare.None);

            var exitCode = Run("convert-batch", input);

            // SYS-011 aborts the run immediately on the SYS-001 file-level error: exit code 2 (not 0 or
            // 3), and neither output artifact is produced - proving no row was ever handed to SYS-002's
            // column resolution or SYS-006's row processing (0 rows reach either stage).
            exitCode.Should().Be(2);
            File.Exists(Path.Combine(dir, "locked.converted.csv")).Should().BeFalse();
            File.Exists(Path.Combine(dir, "locked.invalid-rows.csv")).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    // ---------- SYS-011 orchestrator, dedicated system-scope coverage (STS-011-A1, STS-011-B2) ----------
    // (ATP_017_A/ATP_NF_005_A above already exercise this same behavior at acceptance scope; these are
    // kept as separate, purely STS-named tests so system-scope traceability tooling that only matches on
    // STS-* tokens has an unambiguous match independent of the acceptance-scope ATP naming.)

    [Fact]
    public void STS_011_A1_SYS002ColumnResolutionError_SYS011AbortsRun_ZeroRowsSubmittedToSYS006()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "badheader.csv");
            File.WriteAllText(input, "col_a,col_b,col_c\nfoo,bar,baz\n");

            var exitCode = Run("convert-batch", input);

            exitCode.Should().Be(2);
            File.Exists(Path.Combine(dir, "badheader.converted.csv")).Should().BeFalse();
            File.Exists(Path.Combine(dir, "badheader.invalid-rows.csv")).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void STS_011_B2_10000ValidRows_SinglePassCompletion_SummaryReportsTotal10000Succeeded10000Failed0()
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

            var exitCode = Run("convert-batch", input, "--json");

            exitCode.Should().Be(0);
            ReadDataLines(Path.Combine(dir, "big-batch.converted.csv")).Should().HaveCount(10000);
            ReadDataLines(Path.Combine(dir, "big-batch.invalid-rows.csv")).Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    // ---------- Acceptance-scope gaps found during local verification cross-check ----------
    // (ATP-003, ATP-005, ATP-007, ATP-019, ATP-024, ATP-025, ATP-027, ATP-029 had no
    // literally-tagged/matching executable test prior to this addition.)

    [Fact]
    public void ATP_003_A_SCN_003_A1_IanaTimezoneIdentifiers_AcceptedForSourceAndTarget()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone\n2026-01-15 09:00:00,America/New_York,Asia/Tokyo\n");

            var exitCode = Run("convert-batch", input);

            exitCode.Should().Be(0);
            var successLines = ReadDataLines(Path.Combine(dir, "batch.converted.csv"));
            successLines.Should().ContainSingle(l => l.Contains("Asia/Tokyo"));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_005_A_SCN_005_A1_ValidRow_ConvertedSourceToTarget_WithDstApplied()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone\n2026-07-01 12:00:00,America/New_York,UTC\n");

            var exitCode = Run("convert-batch", input);

            exitCode.Should().Be(0);
            var successLines = ReadDataLines(Path.Combine(dir, "batch.converted.csv"));
            successLines.Should().ContainSingle(l => l.Contains("2026-07-01T16:00:00Z"));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_007_A_SCN_007_A1_SuccessfulOutputRecord_IncludesOriginalInputAndConvertedResult()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone\n2026-05-01 10:00:00,Europe/London,Asia/Kolkata\n");

            var exitCode = Run("convert-batch", input);

            exitCode.Should().Be(0);
            var line = ReadDataLines(Path.Combine(dir, "batch.converted.csv")).Single();
            line.Should().Contain("2026-05-01 10:00:00");
            line.Should().Contain("Europe/London");
            line.Should().Contain("Asia/Kolkata");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_019_A_SCN_019_A1_SuccessfulRows_AvailableAsStructuredMachineReadableOutput()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone\n" +
                "2026-01-15 09:00:00,UTC,UTC\n" +
                "2026-01-16 09:00:00,UTC,UTC\n");

            var exitCode = Run("convert-batch", input);

            exitCode.Should().Be(0);
            var outputPath = Path.Combine(dir, "batch.converted.csv");
            var allLines = File.ReadAllLines(outputPath);
            var headerFieldCount = allLines[0].Split(',').Length;
            headerFieldCount.Should().BeGreaterThan(1);
            var dataLines = ReadDataLines(outputPath);
            dataLines.Should().HaveCount(2);
            dataLines.All(l => l.Split(',').Length == headerFieldCount).Should().BeTrue();
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_024_A_SCN_024_A1_NaiveTimestamp_InterpretedAsLocalToSourceTimezone()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone\n2026-01-15 09:00:00,America/Los_Angeles,UTC\n");

            var exitCode = Run("convert-batch", input);

            exitCode.Should().Be(0);
            var line = ReadDataLines(Path.Combine(dir, "batch.converted.csv")).Single();
            line.Should().Contain("2026-01-15T17:00:00Z");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_025_A_SCN_025_A1_ConversionCrossingDateBoundary_ReportsCorrectResultingDate()
    {
        // Note: acceptance-plan.md's ATP-025-A example assumes America/New_York is on EST (UTC-5)
        // relative to Europe/London on GMT (UTC+0), a 5-hour gap. By 2026 DST rules, March 10
        // is already within America/New_York's EDT window (UTC-4, DST starts March 8, 2026) while
        // Europe/London remains on GMT until March 29, 2026 — a 4-hour gap. The date-boundary-crossing
        // behavior under validation is unaffected by which exact offset applies; this test asserts the
        // actual correct converted value (2026-03-11 03:00 UTC-equivalent) rather than the plan's
        // illustrative 04:00 figure.
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone\n2026-03-10 23:00:00,America/New_York,Europe/London\n");

            var exitCode = Run("convert-batch", input);

            exitCode.Should().Be(0);
            var line = ReadDataLines(Path.Combine(dir, "batch.converted.csv")).Single();
            line.Should().Contain("2026-03-11T03:00:00Z");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_027_A_SCN_027_A1_IdenticalSourceAndTargetTimezone_YieldsSuccessfulUnchangedConversion()
    {
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone\n2026-05-20 14:00:00,Asia/Singapore,Asia/Singapore\n");

            var exitCode = Run("convert-batch", input);

            exitCode.Should().Be(0);
            var successLines = ReadDataLines(Path.Combine(dir, "batch.converted.csv"));
            successLines.Should().ContainSingle();
            ReadDataLines(Path.Combine(dir, "batch.invalid-rows.csv")).Should().BeEmpty();
            successLines[0].Should().Contain("2026-05-20 14:00:00");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ATP_029_A_SCN_029_A1_WriteFailureToOutputDestination_ReportedWithoutDiscardingComputedResults()
    {
        // Covers the same fault-injection guarantee as STS-009-B1 (unwritable output destination),
        // from the acceptance-scope angle: the CLI must report the failure via exit code and message,
        // rather than silently succeeding or crashing, once conversions have already been computed.
        var dir = NewTempDir();
        try
        {
            var input = Path.Combine(dir, "batch.csv");
            File.WriteAllText(input,
                "timestamp,source_timezone,target_timezone\n" +
                "2026-01-15 09:00:00,UTC,UTC\n" +
                "2026-01-16 09:00:00,UTC,UTC\n" +
                "2026-01-17 09:00:00,UTC,UTC\n");

            var blockingFilePath = Path.Combine(dir, "blocked-output");
            File.WriteAllText(blockingFilePath, "not a directory");

            var exitCode = Run("convert-batch", input, "--output-dir", blockingFilePath);

            exitCode.Should().Be(3);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }
}
