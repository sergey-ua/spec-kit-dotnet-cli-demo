using System.CommandLine;
using FluentAssertions;
using TimezoneUtility.Commands;

namespace TimezoneUtility.Contract;

/// <summary>
/// T013/T025/T026/T037: Contract tests for the `convert-batch` CLI option surface and the two CSV
/// output schemas, per contracts/cli-batch-convert.md, contracts/successful-output-schema.md, and
/// contracts/invalid-row-report-schema.md.
/// Verifies REQ-001, REQ-007, REQ-019, REQ-020, REQ-021, REQ-022, REQ-029, REQ-NF-003.
/// Corresponds to STP-009-A, STP-009-B (STS-009-A1, STS-009-B1).
/// </summary>
public class BatchConvertContractTests
{
    private static string RunTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"batch-contract-{Guid.NewGuid()}");
        Directory.CreateDirectory(dir);
        return dir;
    }

    // T013: CLI argument surface contract.
    [Fact]
    public void CLI_Surface_HasInputFileArgument_OutputDirOption_JsonOption()
    {
        var command = ConvertBatchCommand.Create();

        command.Name.Should().Be("convert-batch");
        command.Arguments.Should().ContainSingle(a => a.Name == "input-file");
        command.Options.Should().Contain(o => o.Name == "output-dir");
        command.Options.Should().Contain(o => o.Name == "json");
    }

    [Fact]
    public void CLI_Surface_SupportsHelp()
    {
        var command = ConvertBatchCommand.Create();
        var root = new RootCommand { command };

        var exitCode = root.Invoke(["convert-batch", "--help"]);

        exitCode.Should().Be(0);
    }

    // T013: successful-output CSV schema contract.
    [Fact]
    public void SuccessfulOutputCsv_Schema_MatchesDocumentedHeader()
    {
        var dir = RunTempDir();
        try
        {
            var inputPath = Path.Combine(dir, "batch.csv");
            File.WriteAllText(inputPath,
                "timestamp,source_timezone,target_timezone\n2026-07-01 12:00:00,America/New_York,UTC\n");

            var command = ConvertBatchCommand.Create();
            var root = new RootCommand { command };
            root.Invoke(["convert-batch", inputPath]);

            var outputPath = Path.Combine(dir, "batch.converted.csv");
            File.Exists(outputPath).Should().BeTrue();
            var header = File.ReadLines(outputPath).First();
            header.Should().Be("row_number,original_timestamp,original_source_timezone,original_target_timezone,converted_timestamp,converted_timezone");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    // T025: invalid-row-report CSV schema contract + separateness from successful output.
    [Fact]
    public void InvalidRowReportCsv_Schema_MatchesDocumentedHeader_AndIsSeparateFile()
    {
        var dir = RunTempDir();
        try
        {
            var inputPath = Path.Combine(dir, "batch.csv");
            File.WriteAllText(inputPath,
                "timestamp,source_timezone,target_timezone\n" +
                "2026-07-01 12:00:00,America/New_York,UTC\n" +
                "not-a-date,UTC,UTC\n");

            var command = ConvertBatchCommand.Create();
            var root = new RootCommand { command };
            root.Invoke(["convert-batch", inputPath]);

            var successPath = Path.Combine(dir, "batch.converted.csv");
            var invalidPath = Path.Combine(dir, "batch.invalid-rows.csv");

            File.Exists(successPath).Should().BeTrue();
            File.Exists(invalidPath).Should().BeTrue();
            successPath.Should().NotBe(invalidPath);

            var header = File.ReadLines(invalidPath).First();
            header.Should().Be("row_number,raw_timestamp,raw_source_timezone,raw_target_timezone,reason");

            var invalidLines = File.ReadAllLines(invalidPath).Skip(1).Where(l => l.Length > 0).ToList();
            invalidLines.Should().HaveCount(1);

            var successLines = File.ReadAllLines(successPath).Skip(1).Where(l => l.Length > 0).ToList();
            successLines.Should().HaveCount(1);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    // T026: output-write-failure exit code (3) and error message wording.
    [Fact]
    public void OutputWriteFailure_ExitsWithCode3_AndMessageMentionsCouldNotBeSaved()
    {
        var dir = RunTempDir();
        try
        {
            var inputPath = Path.Combine(dir, "batch.csv");
            File.WriteAllText(inputPath,
                "timestamp,source_timezone,target_timezone\n2026-07-01 12:00:00,America/New_York,UTC\n");

            // Force a write failure deterministically (independent of OS permission model / running-as-root):
            // point --output-dir at a path that is already an ordinary file, so Directory.CreateDirectory
            // (and the subsequent file writes) cannot succeed.
            var blockingFilePath = Path.Combine(dir, "not-a-directory");
            File.WriteAllText(blockingFilePath, "this is a file, not a directory");

            using var sw = new StringWriter();
            var originalErr = Console.Error;
            Console.SetError(sw);
            int exitCode;
            try
            {
                var command = ConvertBatchCommand.Create();
                var root = new RootCommand { command };
                exitCode = root.Invoke(["convert-batch", inputPath, "--output-dir", blockingFilePath]);
            }
            finally
            {
                Console.SetError(originalErr);
            }

            exitCode.Should().Be(3);
            sw.ToString().Should().Contain("could not write output");
            sw.ToString().Should().Contain("No results were discarded");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    // T037: run-summary human-readable and JSON output shape contract.
    [Fact]
    public void RunSummary_Json_ContainsDocumentedFields()
    {
        var dir = RunTempDir();
        try
        {
            var inputPath = Path.Combine(dir, "batch.csv");
            File.WriteAllText(inputPath,
                "timestamp,source_timezone,target_timezone\n2026-07-01 12:00:00,America/New_York,UTC\n");

            var command = ConvertBatchCommand.Create();
            var root = new RootCommand { command };

            using var sw = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(sw);
            try
            {
                root.Invoke(["convert-batch", inputPath, "--json"]);
            }
            finally
            {
                Console.SetOut(originalOut);
            }

            var json = sw.ToString();
            json.Should().Contain("\"inputFile\"");
            json.Should().Contain("\"totalRows\"");
            json.Should().Contain("\"succeededCount\"");
            json.Should().Contain("\"failedCount\"");
            json.Should().Contain("\"noRowsProcessed\"");
            json.Should().Contain("\"successfulOutputFile\"");
            json.Should().Contain("\"invalidRowReportFile\"");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void RunSummary_HumanReadable_DisplaysExplicitZeroFailedCount()
    {
        var dir = RunTempDir();
        try
        {
            var inputPath = Path.Combine(dir, "batch.csv");
            File.WriteAllText(inputPath,
                "timestamp,source_timezone,target_timezone\n2026-07-01 12:00:00,America/New_York,UTC\n");

            var command = ConvertBatchCommand.Create();
            var root = new RootCommand { command };

            using var sw = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(sw);
            try
            {
                root.Invoke(["convert-batch", inputPath]);
            }
            finally
            {
                Console.SetOut(originalOut);
            }

            sw.ToString().Should().Contain("Failed:                0");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }
}
