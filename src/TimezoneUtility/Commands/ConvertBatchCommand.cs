using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using TimezoneUtility.Models;
using TimezoneUtility.Services.BatchConversion;
using TimezoneUtility.Services.TimeConversion;

namespace TimezoneUtility.Commands;

/// <summary>
/// SYS-011 Batch Conversion Orchestrator: the CLI entry point for 'tzutil convert-batch'. Sequences
/// file intake (SYS-001/002) -&gt; per-row processing (SYS-006) -&gt; output writing (SYS-009) -&gt; run
/// summary (SYS-010), owning the empty-file completion path and the file-level/write-failure exit codes
/// defined in contracts/cli-batch-convert.md.
/// </summary>
public static class ConvertBatchCommand
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>Creates the 'convert-batch' command.</summary>
    public static Command Create()
    {
        var inputFileArg = new Argument<string>("input-file", "Path to the input CSV file");

        var outputDirOption = new Option<string?>(
            ["--output-dir"],
            "Directory to write both output files into (defaults to the input file's directory)");

        var jsonOption = new Option<bool>(
            ["--json"],
            "Print the run summary as JSON instead of human-readable text");

        var command = new Command("convert-batch", "Convert a batch of timestamps listed in a CSV file")
        {
            inputFileArg,
            outputDirOption,
            jsonOption
        };

        command.SetHandler(
            context => Execute(
                context,
                context.ParseResult.GetValueForArgument(inputFileArg),
                context.ParseResult.GetValueForOption(outputDirOption),
                context.ParseResult.GetValueForOption(jsonOption)));

        return command;
    }

    private static void Execute(InvocationContext context, string inputFile, string? outputDir, bool json)
    {
        var readResult = CsvBatchReader.Read(inputFile);
        if (!readResult.IsSuccess)
        {
            Console.Error.WriteLine($"Error: {readResult.FileLevelError}");
            context.ExitCode = 2;
            Environment.ExitCode = 2;
            return;
        }

        var timeService = new TimeService();
        var successes = new List<RowConversionResult>();
        var failures = new List<InvalidRowRecord>();

        foreach (var row in readResult.Rows)
        {
            var outcome = BatchRowProcessor.Process(row, timeService);
            if (outcome.Success is not null)
            {
                successes.Add(outcome.Success);
            }
            else if (outcome.Failure is not null)
            {
                failures.Add(outcome.Failure);
            }
        }

        var noRowsProcessed = readResult.Rows.Count == 0;
        var summary = BatchRunSummaryGenerator.Generate(inputFile, successes.Count, failures.Count, noRowsProcessed);

        var writeResult = BatchOutputWriter.WriteAll(inputFile, outputDir, successes, failures);
        if (!writeResult.IsSuccess)
        {
            Console.Error.WriteLine($"Error: {writeResult.WriteError}");
            context.ExitCode = 3;
            Environment.ExitCode = 3;
            return;
        }

        var successfulOutputFile = BatchOutputWriter.SuccessfulOutputPath(inputFile, outputDir);
        var invalidRowReportFile = BatchOutputWriter.InvalidRowReportPath(inputFile, outputDir);

        if (json)
        {
            var payload = new
            {
                inputFile = summary.InputFile,
                totalRows = summary.TotalRows,
                succeededCount = summary.SucceededCount,
                failedCount = summary.FailedCount,
                noRowsProcessed = summary.NoRowsProcessed,
                successfulOutputFile,
                invalidRowReportFile
            };
            Console.WriteLine(JsonSerializer.Serialize(payload, JsonOptions));
        }
        else
        {
            Console.Write(BatchRunSummaryGenerator.RenderHumanReadable(summary, successfulOutputFile, invalidRowReportFile));
        }

        context.ExitCode = 0;
        Environment.ExitCode = 0;
    }
}
