using System.Globalization;
using TimezoneUtility.Models;

namespace TimezoneUtility.Services.BatchConversion;

/// <summary>
/// SYS-010 Run Summary Generator: computes total/succeeded/failed counts and the header-only
/// "no rows processed" flag (REQ-021, REQ-022, REQ-023, REQ-NF-003), and renders the console/JSON
/// presentation described in <c>contracts/cli-batch-convert.md</c>.
/// </summary>
public static class BatchRunSummaryGenerator
{
    /// <summary>Builds the run summary for a completed batch run.</summary>
    public static BatchRunSummary Generate(string inputFile, int successCount, int failureCount, bool noRowsProcessed) => new()
    {
        InputFile = inputFile,
        TotalRows = successCount + failureCount,
        SucceededCount = successCount,
        FailedCount = failureCount,
        NoRowsProcessed = noRowsProcessed
    };

    /// <summary>Renders the human-readable console summary, including the successful/invalid output file paths.</summary>
    public static string RenderHumanReadable(BatchRunSummary summary, string successfulOutputFile, string invalidRowReportFile)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"Batch conversion complete: {summary.InputFile}");
        if (summary.NoRowsProcessed)
        {
            sb.AppendLine("  No rows were processed (input file contained only a header row).");
        }
        else
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"  Total rows processed: {summary.TotalRows}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  Succeeded:             {summary.SucceededCount}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  Failed:                {summary.FailedCount}");
        }

        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"Successful output:      {successfulOutputFile}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Invalid-row report:     {invalidRowReportFile}");
        return sb.ToString();
    }
}
