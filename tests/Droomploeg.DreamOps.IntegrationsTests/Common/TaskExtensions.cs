using System.Diagnostics;

namespace Droomploeg.DreamOps.IntegrationsTests.Common;

internal static class TaskExtensions
{
    internal static Task WaitWithDelay(this Func<Task<bool>> func, TimeSpan maxWaitTime, string? timeOutMessage = null)
    {
        return func.WaitWithDelay(maxWaitTime, TimeSpan.FromSeconds(0.5), timeOutMessage);
    }

    internal static async Task WaitWithDelay(this Func<Task<bool>> func, TimeSpan maxWaitTime, TimeSpan pollingTime, string? timeOutMessage = null)
    {
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < maxWaitTime)
        {
            if (await func())
            {
                return;
            }

            await Task.Delay(pollingTime);
        }

        throw new TimeoutException(timeOutMessage ?? $"Waited for {maxWaitTime} but condition was never met.");
    }
}
