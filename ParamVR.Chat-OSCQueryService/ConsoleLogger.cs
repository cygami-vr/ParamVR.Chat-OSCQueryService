using Microsoft.Extensions.Logging;
using VRC.OSCQuery;

public class ConsoleLogger : ILogger<OSCQueryService>
{
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => Console.WriteLine(formatter(state, exception));
}
