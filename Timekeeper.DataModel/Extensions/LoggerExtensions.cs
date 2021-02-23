using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace DocsVideos.Model.Extensions
{
    public static class LoggerExtensions
    {
        public static void LogEntry(
            this ILogger log, 
            [CallerMemberName]string methodName = null)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                return;
            }

            log.LogInformation($"-> {methodName}");
        }

        public static void LogExit(
            this ILogger log,
            [CallerMemberName] string methodName = null)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                return;
            }

            log.LogInformation($"{methodName} ->");
        }

        public static void LogDebug(
            this ILogger log,
            string message,
            [CallerMemberName] string methodName = null)
        {
            log.LogDebug($"In {methodName}: {message}");
        }


        public static void LogTrace(
            this ILogger log,
            string message,
            [CallerMemberName] string methodName = null)
        {
            log.LogTrace($"In {methodName}: {message}");
        }

        public static void LogDebugObject(
            this ILogger log,
            string message,
            [CallerMemberName] string methodName = null,
            params object[] args)
        {
            if (args != null)
            {
                foreach (var arg in args)
                {
                    message = $"{message} / {arg}";
                }
            }

            log.LogDebug($"In {methodName}: {message}");
        }
    }
}
