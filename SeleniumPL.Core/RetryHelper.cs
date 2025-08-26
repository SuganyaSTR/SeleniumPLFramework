using Serilog;
using System;
using System.Threading;

namespace SeleniumPL.Core
{
    /// <summary>
    /// Enhanced retry helper with exponential backoff and configurable retry strategies
    /// </summary>
    public static class RetryHelper
    {
        public static T ExecuteWithRetry<T>(
            Func<T> operation,
            int maxAttempts = 3,
            TimeSpan? baseDelay = null,
            bool useExponentialBackoff = true,
            ILogger? logger = null)
        {
            var actualLogger = logger ?? Log.Logger;
            var delay = baseDelay ?? TimeSpan.FromSeconds(1);
            Exception lastException = null!;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    actualLogger.Debug("Executing operation, attempt {Attempt}/{MaxAttempts}", attempt, maxAttempts);
                    return operation();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    actualLogger.Warning("Operation failed on attempt {Attempt}/{MaxAttempts}: {Error}", 
                        attempt, maxAttempts, ex.Message);

                    if (attempt == maxAttempts)
                    {
                        actualLogger.Error("Operation failed after {MaxAttempts} attempts", maxAttempts);
                        throw;
                    }

                    // Calculate delay with exponential backoff
                    var currentDelay = useExponentialBackoff 
                        ? TimeSpan.FromMilliseconds(delay.TotalMilliseconds * Math.Pow(2, attempt - 1))
                        : delay;

                    actualLogger.Information("Retrying in {Delay}ms...", currentDelay.TotalMilliseconds);
                    Thread.Sleep(currentDelay);
                }
            }

            throw lastException;
        }

        public static void ExecuteWithRetry(
            Action operation,
            int maxAttempts = 3,
            TimeSpan? baseDelay = null,
            bool useExponentialBackoff = true,
            ILogger? logger = null)
        {
            ExecuteWithRetry(() =>
            {
                operation();
                return true;
            }, maxAttempts, baseDelay, useExponentialBackoff, logger);
        }
    }
}
