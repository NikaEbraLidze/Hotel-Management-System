using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace hms.Api.Logging
{
    public sealed class FileLogger : ILogger
    {
        private static readonly object Sync = new();

        private readonly string _categoryName;
        private readonly string _filePath;

        public FileLogger(string categoryName, string filePath)
        {
            _categoryName = categoryName;
            _filePath = filePath;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            var logEntry = new StringBuilder()
                .Append('[').Append(DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz")).Append("] ")
                .Append('[').Append(logLevel).Append("] ")
                .Append('[').Append(_categoryName).Append("] ")
                .AppendLine(message);

            if (exception is not null)
            {
                logEntry.AppendLine(exception.ToString());
            }

            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            lock (Sync)
            {
                File.AppendAllText(_filePath, logEntry.ToString(), Encoding.UTF8);
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
