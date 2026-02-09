using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Witcher3StringEditor.Extensions;

public static class TaskLoggingExtensions
{
    public static Task LogExceptions(this Task task, string? context = null)
    {
        ArgumentNullException.ThrowIfNull(task);

        _ = task.ContinueWith(
            t => LogException(t.Exception, context),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted,
            TaskScheduler.Default);

        return task;
    }

    private static void LogException(AggregateException? exception, string? context)
    {
        if (exception is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(context))
        {
            Log.Error(exception, "Background task failed with an exception.");
            return;
        }

        Log.Error(exception, "Background task failed: {Context}.", context);
    }
}
