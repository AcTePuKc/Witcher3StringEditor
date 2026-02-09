using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Witcher3StringEditor.Extensions;

public static class TaskLoggingExtensions
{
    public static void SafeFireAndForget(this Task task, string? context = null)
    {
        ArgumentNullException.ThrowIfNull(task);

        _ = task.ContinueWith(
            t => LogException(t.Exception, context),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted,
            TaskScheduler.Default);
    }

    public static void SafeFireAndForget(this ValueTask task, string? context = null)
    {
        _ = task.AsTask().ContinueWith(
            t => LogException(t.Exception, context),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted,
            TaskScheduler.Default);
    }

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

    public static async ValueTask LogExceptions(this ValueTask task, string? context = null)
    {
        try
        {
            await task;
        }
        catch (Exception exception)
        {
            LogException(exception, context);
            throw;
        }
    }

    public static async ValueTask<T> LogExceptions<T>(this ValueTask<T> task, string? context = null)
    {
        try
        {
            return await task;
        }
        catch (Exception exception)
        {
            LogException(exception, context);
            throw;
        }
    }

    private static void LogException(AggregateException? exception, string? context)
    {
        if (exception is null)
        {
            return;
        }

        LogException((Exception)exception, context);
    }

    private static void LogException(Exception exception, string? context)
    {
        if (string.IsNullOrWhiteSpace(context))
        {
            Log.Error(exception, "Background task failed with an exception.");
            return;
        }

        Log.Error(exception, "Background task failed: {Context}.", context);
    }
}
