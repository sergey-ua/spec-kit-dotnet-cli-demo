using TimezoneUtility.Models;
using TimezoneUtility.Services.TimeConversion;

namespace TimezoneUtility.Services.BatchConversion;

/// <summary>
/// Part of SYS-011 Batch Conversion Orchestrator: sequences the per-row invocation of SYS-006 (Row
/// Processing Controller) across an entire batch. SYS-006 already isolates row-level failures
/// internally (see <see cref="BatchRowProcessor"/>), but the SYS-011 -&gt; SYS-006 dependency edge
/// documents an additional failure-isolation guarantee (system-design.md): if SYS-006 itself raises an
/// unhandled (non-isolated) exception for a row, SYS-011 must still continue invoking SYS-006 for the
/// remaining rows rather than aborting the whole run (REQ-006, REQ-011, REQ-NF-001; verified by STS-006-B1).
/// </summary>
public static class BatchRowsOrchestrator
{
    /// <summary>
    /// Invokes <paramref name="processRow"/> (defaulting to <see cref="BatchRowProcessor.Process"/>) once
    /// per row, isolating any exception the call itself raises so that a single row's unexpected failure
    /// never prevents the remaining rows from being processed.
    /// </summary>
    public static (List<RowConversionResult> Successes, List<InvalidRowRecord> Failures) ProcessAll(
        IReadOnlyList<BatchConversionRow> rows,
        ITimeService timeService,
        Func<BatchConversionRow, ITimeService, BatchRowOutcome>? processRow = null)
    {
        processRow ??= BatchRowProcessor.Process;

        var successes = new List<RowConversionResult>();
        var failures = new List<InvalidRowRecord>();

        foreach (var row in rows)
        {
            BatchRowOutcome outcome;
            try
            {
                outcome = processRow(row, timeService);
            }
            catch (Exception)
            {
                // SYS-006 failed to isolate its own fault; SYS-011 still isolates it here so that
                // rows after this one are still processed (STS-006-B1).
                failures.Add(InvalidRowReporter.Report(row, "unexpected error during row processing"));
                continue;
            }

            if (outcome.Success is not null)
            {
                successes.Add(outcome.Success);
            }
            else if (outcome.Failure is not null)
            {
                failures.Add(outcome.Failure);
            }
        }

        return (successes, failures);
    }
}
