using System.Globalization;
using System.Text;
using NodaTime.Text;
using TimezoneUtility.Models;

namespace TimezoneUtility.Services.BatchConversion;

/// <summary>Result of a write attempt: either success, or a failure identifying which file could not be written.</summary>
public sealed record BatchOutputWriteResult
{
    /// <summary>Non-null when a write failure occurred (REQ-029).</summary>
    public string? WriteError { get; init; }

    public bool IsSuccess => WriteError is null;

    public static BatchOutputWriteResult Success() => new();

    public static BatchOutputWriteResult Failure(string error) => new() { WriteError = error };
}

/// <summary>
/// SYS-009 Output Writer: emits the successful-output CSV (<c>&lt;basename&gt;.converted.csv</c>) and the
/// invalid-row report CSV (<c>&lt;basename&gt;.invalid-rows.csv</c>) per the documented schemas, always
/// writing both files even when one result set is empty (REQ-019, REQ-020), and reporting write failures
/// without discarding already-computed in-memory results (REQ-029).
/// </summary>
public static class BatchOutputWriter
{
    private static readonly OffsetDateTimePattern OutputPattern =
        OffsetDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd'T'HH':'mm':'sso<G>");

    /// <summary>Computes the path for the successful-output CSV given the input file path and output directory.</summary>
    public static string SuccessfulOutputPath(string inputFilePath, string? outputDir) =>
        BuildPath(inputFilePath, outputDir, ".converted.csv");

    /// <summary>Computes the path for the invalid-row report CSV given the input file path and output directory.</summary>
    public static string InvalidRowReportPath(string inputFilePath, string? outputDir) =>
        BuildPath(inputFilePath, outputDir, ".invalid-rows.csv");

    private static string BuildPath(string inputFilePath, string? outputDir, string suffix)
    {
        var basename = Path.GetFileNameWithoutExtension(inputFilePath);
        var dir = string.IsNullOrWhiteSpace(outputDir)
            ? Path.GetDirectoryName(Path.GetFullPath(inputFilePath)) ?? "."
            : outputDir;
        return Path.Combine(dir, basename + suffix);
    }

    /// <summary>Writes both output files. Neither in-memory list is discarded if a write fails.</summary>
    public static BatchOutputWriteResult WriteAll(
        string inputFilePath,
        string? outputDir,
        IReadOnlyList<RowConversionResult> successes,
        IReadOnlyList<InvalidRowRecord> failures)
    {
        var successPath = SuccessfulOutputPath(inputFilePath, outputDir);
        var invalidPath = InvalidRowReportPath(inputFilePath, outputDir);

        try
        {
            var dir = Path.GetDirectoryName(successPath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            WriteSuccessfulCsv(successPath, successes);
            WriteInvalidRowsCsv(invalidPath, failures);
            return BatchOutputWriteResult.Success();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            var targetDir = string.IsNullOrWhiteSpace(outputDir) ? Path.GetDirectoryName(successPath) ?? "." : outputDir;
            return BatchOutputWriteResult.Failure(
                $"Batch conversion finished ({successes.Count} succeeded, {failures.Count} failed) but could not write output to \"{targetDir}\": {ex.Message}. No results were discarded; re-run with a writable --output-dir to retry writing.");
        }
    }

    private static void WriteSuccessfulCsv(string path, IReadOnlyList<RowConversionResult> results)
    {
        using var writer = new StreamWriter(path, false, Encoding.UTF8);
        writer.WriteLine("row_number,original_timestamp,original_source_timezone,original_target_timezone,converted_timestamp,converted_timezone");
        foreach (var r in results)
        {
            var convertedTimestamp = OutputPattern.Format(r.ConvertedInstant.WithOffset(r.TargetZone.GetUtcOffset(r.ConvertedInstant)));
            writer.WriteLine(string.Join(",",
                r.RowNumber.ToString(CultureInfo.InvariantCulture),
                Escape(r.OriginalTimestamp),
                Escape(r.OriginalSourceTimezone),
                Escape(r.OriginalTargetTimezone),
                Escape(convertedTimestamp),
                Escape(r.TargetZone.Id)));
        }
    }

    private static void WriteInvalidRowsCsv(string path, IReadOnlyList<InvalidRowRecord> records)
    {
        using var writer = new StreamWriter(path, false, Encoding.UTF8);
        writer.WriteLine("row_number,raw_timestamp,raw_source_timezone,raw_target_timezone,reason");
        foreach (var r in records)
        {
            writer.WriteLine(string.Join(",",
                r.RowNumber.ToString(CultureInfo.InvariantCulture),
                Escape(r.RawTimestamp),
                Escape(r.RawSourceTimezone),
                Escape(r.RawTargetTimezone),
                Escape(r.Reason)));
        }
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
        return value;
    }
}
