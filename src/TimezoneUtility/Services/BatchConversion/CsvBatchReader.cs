using TimezoneUtility.Models;

namespace TimezoneUtility.Services.BatchConversion;

/// <summary>
/// Result of a batch read: either a file-level error, or the resolved rows (SYS-001 CSV File Intake
/// + SYS-002 Header/Column Resolver).
/// </summary>
public sealed record CsvBatchReadResult
{
    /// <summary>Non-null when a file-level error occurred (REQ-016-REQ-018); processing must short-circuit.</summary>
    public string? FileLevelError { get; init; }

    /// <summary>The parsed data rows (blank rows excluded per REQ-028). Empty when <see cref="FileLevelError"/> is set.</summary>
    public IReadOnlyList<BatchConversionRow> Rows { get; init; } = Array.Empty<BatchConversionRow>();

    public bool IsSuccess => FileLevelError is null;
}

/// <summary>
/// SYS-001 CSV File Intake + SYS-002 Header/Column Resolver: opens the input CSV file, resolves the
/// required columns by name (case-insensitive, order-independent), and yields <see cref="BatchConversionRow"/>
/// instances for every non-blank data row.
/// </summary>
public static class CsvBatchReader
{
    private static readonly string[] RequiredColumns = ["timestamp", "source_timezone", "target_timezone"];

    /// <summary>Reads and resolves the given input CSV file.</summary>
    /// <param name="inputFilePath">Path to the input CSV file.</param>
    public static CsvBatchReadResult Read(string inputFilePath)
    {
        if (!File.Exists(inputFilePath))
        {
            return new CsvBatchReadResult
            {
                FileLevelError = $"Could not open input file \"{inputFilePath}\": file not found."
            };
        }

        string[] lines;
        try
        {
            lines = File.ReadAllLines(inputFilePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new CsvBatchReadResult
            {
                FileLevelError = $"Could not open input file \"{inputFilePath}\": {ex.Message}"
            };
        }

        if (lines.Length == 0)
        {
            return new CsvBatchReadResult
            {
                FileLevelError = $"Could not identify required columns in \"{inputFilePath}\". Expected a header row containing \"timestamp\", \"source_timezone\", and \"target_timezone\" (case-insensitive, any order). Found columns: none."
            };
        }

        var headerCells = ParseCsvLine(lines[0]);
        var columnIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < headerCells.Count; i++)
        {
            var name = headerCells[i].Trim();
            if (!columnIndex.ContainsKey(name))
            {
                columnIndex[name] = i;
            }
        }

        var missing = RequiredColumns.Where(c => !columnIndex.ContainsKey(c)).ToArray();
        if (missing.Length > 0)
        {
            var found = string.Join(", ", headerCells.Select(h => $"\"{h.Trim()}\""));
            return new CsvBatchReadResult
            {
                FileLevelError = $"Could not identify required columns in \"{inputFilePath}\". Expected a header row containing \"timestamp\", \"source_timezone\", and \"target_timezone\" (case-insensitive, any order). Found columns: {found}."
            };
        }

        var timestampIdx = columnIndex["timestamp"];
        var sourceIdx = columnIndex["source_timezone"];
        var targetIdx = columnIndex["target_timezone"];

        var rows = new List<BatchConversionRow>();
        var rowNumber = 0;
        for (var lineIndex = 1; lineIndex < lines.Length; lineIndex++)
        {
            var line = lines[lineIndex];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var cells = ParseCsvLine(line);
            if (cells.All(string.IsNullOrWhiteSpace))
            {
                // Entirely blank row across all columns - skipped, not counted (REQ-028).
                continue;
            }

            rowNumber++;
            rows.Add(new BatchConversionRow
            {
                RowNumber = rowNumber,
                RawTimestamp = GetCell(cells, timestampIdx),
                RawSourceTimezone = GetCell(cells, sourceIdx),
                RawTargetTimezone = GetCell(cells, targetIdx)
            });
        }

        return new CsvBatchReadResult { Rows = rows };
    }

    private static string GetCell(List<string> cells, int index) => index < cells.Count ? cells[index] : string.Empty;

    /// <summary>
    /// Splits one CSV line into cells, supporting comma delimiting and double-quoted values containing commas
    /// (RFC 4180 quoting is not fully required for this demo scale, per plan.md's Technical Context).
    /// </summary>
    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
        }

        result.Add(current.ToString());
        return result;
    }
}
