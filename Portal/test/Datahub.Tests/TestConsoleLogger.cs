using System;
using Microsoft.Extensions.Logging;

namespace Datahub.Tests
{
    /// <summary>
    /// Simple console logger for testing purposes that outputs formatted log messages to the console
    /// </summary>
    public class TestConsoleLogger<T> : ILogger<T>
    {
        private readonly string _categoryName;
        private readonly LogLevel _minimumLogLevel;

        public TestConsoleLogger(LogLevel minimumLogLevel = LogLevel.Information)
        {
            _categoryName = typeof(T).Name;
            _minimumLogLevel = minimumLogLevel;
        }

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
        
        public bool IsEnabled(LogLevel logLevel) => logLevel >= _minimumLogLevel;
        
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var logLevelString = GetLogLevelString(logLevel);
            var message = formatter(state, exception);
            
            Console.WriteLine($"[{timestamp}] {logLevelString} [{_categoryName}] {message}");
            
            if (exception != null)
            {
                Console.WriteLine($"[{timestamp}] {logLevelString} [{_categoryName}] Exception: {exception}");
            }
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "TRCE",
                LogLevel.Debug => "DBUG",
                LogLevel.Information => "INFO",
                LogLevel.Warning => "WARN",
                LogLevel.Error => "FAIL",
                LogLevel.Critical => "CRIT",
                LogLevel.None => "NONE",
                _ => logLevel.ToString().ToUpper()
            };
        }
    }

    /// <summary>
    /// Factory for creating TestConsoleLogger instances
    /// </summary>
    public class TestConsoleLoggerFactory : ILoggerFactory
    {
        private readonly LogLevel _minimumLogLevel;
        private bool _disposed = false;

        public TestConsoleLoggerFactory(LogLevel minimumLogLevel = LogLevel.Information)
        {
            _minimumLogLevel = minimumLogLevel;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestConsoleLoggerGeneric(categoryName, _minimumLogLevel);
        }

        public void AddProvider(ILoggerProvider provider)
        {
            // Not implemented for test logger
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }

    /// <summary>
    /// Generic console logger that works with any category name
    /// </summary>
    internal class TestConsoleLoggerGeneric : ILogger
    {
        private readonly string _categoryName;
        private readonly LogLevel _minimumLogLevel;

        public TestConsoleLoggerGeneric(string categoryName, LogLevel minimumLogLevel = LogLevel.Information)
        {
            _categoryName = categoryName;
            _minimumLogLevel = minimumLogLevel;
        }

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
        
        public bool IsEnabled(LogLevel logLevel) => logLevel >= _minimumLogLevel;
        
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var logLevelString = GetLogLevelString(logLevel);
            var message = formatter(state, exception);
            
            Console.WriteLine($"[{timestamp}] {logLevelString} [{_categoryName}] {message}");
            
            if (exception != null)
            {
                Console.WriteLine($"[{timestamp}] {logLevelString} [{_categoryName}] Exception: {exception}");
            }
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "TRCE",
                LogLevel.Debug => "DBUG",
                LogLevel.Information => "INFO",
                LogLevel.Warning => "WARN",
                LogLevel.Error => "FAIL",
                LogLevel.Critical => "CRIT",
                LogLevel.None => "NONE",
                _ => logLevel.ToString().ToUpper()
            };
        }
    }

    internal class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();
        public void Dispose() { }
    }
}