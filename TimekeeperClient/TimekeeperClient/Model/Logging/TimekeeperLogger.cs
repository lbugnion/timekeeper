using Microsoft.Extensions.Logging;
using System;

public class TimekeeperLogger : ILogger
{
    private readonly TimekeeperLoggerConfiguration _config;
    private readonly string _name;
    public const string HighlightCode = "HIGHLIGHT--";

    public TimekeeperLogger(string name, TimekeeperLoggerConfiguration config)
    {
        _name = name;
        _config = config;
    }

    public IDisposable BeginScope<TState>(TState state) => default;

    public bool IsEnabled(LogLevel logLevel) =>
        logLevel >= _config.MinimumLogLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        string color;
        string prefix;

        switch (logLevel)
        {
            case LogLevel.Trace:
                color = ConsoleCodes.FgGreen;
                prefix = "trce";
                break;

            case LogLevel.Debug:
                color = ConsoleCodes.FgGreen;
                prefix = "debg";
                break;

            case LogLevel.Warning:
                color = ConsoleCodes.BgYellow;
                prefix = "warn";
                break;

            case LogLevel.Error:
                color = ConsoleCodes.FgRed;
                prefix = "errr";
                break;

            case LogLevel.Critical:
                color = ConsoleCodes.FgRed;
                prefix = "crit";
                break;

            default:
                color = ConsoleCodes.FgBlue;
                prefix = "info";
                break;
        }

        var message = state.ToString();

        if (message.StartsWith(HighlightCode))
        {
            color = ConsoleCodes.BgMagenta;
            message = message.Replace(HighlightCode, string.Empty);
        }

        var timestamp = DateTime.Now.ToString("yyyy:MM:dd HH:mm:ss:fff");

        Console.WriteLine($"{ConsoleCodes.BgCyan}{_name} @ {timestamp} {color}{prefix}: {message}\x1b[0m");
    }

    private static class ConsoleCodes
    {
        public const string BgBlack = "\x1b[40m";
        public const string BgBlue = "\x1b[44m";
        public const string BgCyan = "\x1b[46m";
        public const string BgGreen = "\x1b[42m";
        public const string BgMagenta = "\x1b[45m";
        public const string BgRed = "\x1b[41m";
        public const string BgWhite = "\x1b[47m";
        public const string BgYellow = "\x1b[43m";
        public const string Blink = "\x1b[5m";
        public const string Bright = "\x1b[1m";
        public const string Dim = "\x1b[2m";
        public const string FgBlack = "\x1b[30m";
        public const string FgBlue = "\x1b[34m";
        public const string FgCyan = "\x1b[36m";
        public const string FgGreen = "\x1b[32m";
        public const string FgMagenta = "\x1b[35m";
        public const string FgRed = "\x1b[31m";
        public const string FgWhite = "\x1b[37m";
        public const string FgYellow = "\x1b[33m";
        public const string Hidden = "\x1b[8m";
        public const string Reset = "\x1b[0m";
        public const string Reverse = "\x1b[7m";
        public const string Underscore = "\x1b[4m";
    }
}
