using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InterviewAssignment3.Logging
{
    /// <summary>
    /// Logs to file. An instance of this class gets created for each category.
    /// </summary>
    public class FileLogger : ILogger
    {
        private readonly FileLoggerManager _fileLoggerManager;
        private readonly string _categoryName;

        public FileLogger(string categoryName, FileLoggerManager fileLoggerManager)
        {
            _categoryName = categoryName;
            _fileLoggerManager = fileLoggerManager ?? throw new ArgumentNullException(nameof(fileLoggerManager));
       }

        public IDisposable BeginScope<TState>(TState state) => default!;

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter is null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            string message = $"{_categoryName} - {formatter(state, exception)}";
            string contents = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}\t[{logLevel}] {message}\r\n";
            if (exception is { })
            {
                contents += exception.ToString() + "\r\n";
            }
            _fileLoggerManager.Enqueue(contents);
        }


        //private static void AppendTextToFile(string filePath, string contents)
        //{
        //    AppendTextToFile(filePath, contents, Encoding.UTF8);
        //}

        //private static void AppendTextToFile(string filePath, string contents, System.Text.Encoding encoding)
        //{
        //    int attempts = 10;

        //    for (int i = 1; i < attempts; i++)
        //    {
        //        try
        //        {
        //            File.AppendAllText(filePath, contents, encoding);
        //            return;
        //        }
        //        catch (Exception)
        //        {
        //            System.Threading.Thread.Sleep(GetFileAccessSleepTime(i));
        //        }
        //    }

        //    File.AppendAllText(filePath, contents, encoding); //Final attempt
        //}

        //private static int GetFileAccessSleepTime(int attempt)
        //{
        //    double time = 25d;
        //    double percent = 0.2d;

        //    for (int i = 1; i <= attempt; i++)
        //    {
        //        if (i == 1)
        //            continue;

        //        time = time + (time * percent);
        //    }

        //    return (int)time;
        //}
    }
}
