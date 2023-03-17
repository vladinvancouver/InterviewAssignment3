using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InterviewAssignment3.Logging
{
    [ProviderAlias("File")]
    public sealed class FileLoggerProvider : ILoggerProvider
    {
        private FileLoggerManager _fileLoggerManager;

        public FileLoggerProvider(FileLoggerManager fileLoggerManager)
        {
            _fileLoggerManager = fileLoggerManager ?? throw new ArgumentNullException(nameof(fileLoggerManager));
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(categoryName, _fileLoggerManager);
        }

        public void Dispose()
        {

        }
    }
}
