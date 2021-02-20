using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

public sealed class TimekeeperLoggerProvider : ILoggerProvider
{
    private readonly TimekeeperLoggerConfiguration _config;

    private readonly ConcurrentDictionary<string, TimekeeperLogger> _loggers =
        new ConcurrentDictionary<string, TimekeeperLogger>();

    public TimekeeperLoggerProvider(TimekeeperLoggerConfiguration config)
    {
        _config = config;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(
            categoryName,
            name => new TimekeeperLogger(name, _config));
    }

    public void Dispose() => _loggers.Clear();
}