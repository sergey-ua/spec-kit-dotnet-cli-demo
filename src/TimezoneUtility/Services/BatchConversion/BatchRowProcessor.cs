using TimezoneUtility.Models;
using TimezoneUtility.Services.TimeConversion;

namespace TimezoneUtility.Services.BatchConversion;

/// <summary>Outcome of processing one row: either a success or a failure, never both.</summary>
public sealed record BatchRowOutcome
{
    public RowConversionResult? Success { get; init; }
    public InvalidRowRecord? Failure { get; init; }
}

/// <summary>
/// SYS-006 Row Processing Controller: orchestrates validator (SYS-003) -&gt; timestamp interpreter (SYS-004)
/// -&gt; conversion engine (SYS-005) -&gt; result formatter (SYS-008) per row, catching per-row failures and
/// routing them to the <see cref="InvalidRowReporter"/> instead of aborting the run (REQ-006, REQ-011, REQ-NF-001).
/// </summary>
public static class BatchRowProcessor
{
    /// <summary>Processes a single row in complete isolation from every other row.</summary>
    public static BatchRowOutcome Process(BatchConversionRow row, ITimeService timeService)
    {
        try
        {
            var validation = BatchRowValidator.Validate(row);
            if (!validation.IsValid)
            {
                return new BatchRowOutcome { Failure = InvalidRowReporter.Report(row, validation.Reason!) };
            }

            if (!BatchTimestampInterpreter.TryInterpret(row.RawTimestamp, validation.SourceZone!, timeService, out var instant))
            {
                return new BatchRowOutcome
                {
                    Failure = InvalidRowReporter.Report(row, $"unparseable timestamp: {row.RawTimestamp}")
                };
            }

            var result = SuccessfulResultFormatter.Format(row, validation.TargetZone!, instant);
            return new BatchRowOutcome { Success = result };
        }
        catch (Exception)
        {
            // Any unexpected fault during interpretation/conversion is isolated to this row (REQ-006, REQ-011):
            // it never aborts the run for the remaining rows.
            return new BatchRowOutcome
            {
                Failure = InvalidRowReporter.Report(row, $"unparseable timestamp: {row.RawTimestamp}")
            };
        }
    }
}
