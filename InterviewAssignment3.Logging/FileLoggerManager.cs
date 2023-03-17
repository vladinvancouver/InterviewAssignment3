using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace InterviewAssignment3.Logging
{
    /// <summary>
    /// Manages common file logging functionlity. This class lives for the duration of the application.
    /// Rollover (in the same directory) occurs based on date placeholders in the file name like %yyyy-%MM-%dd.%username.log
    /// Archiving (moving to another directory) is configured in FileLoggerOptions.
    /// </summary>
    public class FileLoggerManager : IDisposable
    {
        private readonly Channel<Action> _queue = Channel.CreateUnbounded<Action>();
        private static string _userName = Environment.UserName.ToLowerInvariant();

        private readonly IDisposable _onChangeToken;
        private FileLoggerOptions _currentOptions;

        private readonly System.Timers.Timer _timer;

        private readonly Task _outputTask;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private string _logFilePath;

        public FileLoggerManager(IOptionsMonitor<FileLoggerOptions> options)
        {
            _currentOptions = options.CurrentValue;
            _onChangeToken = options.OnChange(updatedOptions => _currentOptions = updatedOptions);


            FileLoggerOptions fileLoggerOptions = GetCurrentConfig();
            DateTime now = DateTime.Now;

            _logFilePath = BuildLogFilePath(fileLoggerOptions.LogFilePath, DateTime.Now, _userName);

            _outputTask = Task.Run(async () => await DequeueAllAsync());

            _queue.Writer.TryWrite(() =>
            {
                Tick();
            });

            //TimeSpan interval = TimeSpan.FromSeconds(1);
            TimeSpan interval = TimeSpan.FromMinutes(1);
            _timer = new System.Timers.Timer(interval.TotalMilliseconds);

            // Hook up the Elapsed event for the timer. 
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        public FileLoggerOptions GetCurrentConfig()
        {
            return _currentOptions;
        }

        public string GetLogFilePath()
        {
            return _logFilePath;
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _queue.Writer.TryWrite(() =>
            {
                Tick();
            });
        }

        private void Tick()
        {
            FileLoggerOptions fileLoggerOptions = GetCurrentConfig();
            DateTime now = DateTime.Now;

            //Get current log file path in case rollover occurs
            string currentLogFile = _logFilePath;

            //Set new log file path
            _logFilePath = BuildLogFilePath(fileLoggerOptions.LogFilePath, DateTime.Now, _userName);

            //Achive based on date
            DateTime lastArchiveDate = now;
            try
            {
                FileInfo fileInfo = new(currentLogFile);

                if (fileInfo.Exists)
                {
                    lastArchiveDate = fileInfo.CreationTime;
                }
            }
            catch { }

            if (IsArchiveByDateRequired(fileLoggerOptions.ArchiveInterval, lastArchiveDate, now))
            {
                ArchiveFile(currentLogFile, fileLoggerOptions.ArchiveDirectoryPath, fileLoggerOptions.ArchiveFileName, now, _userName);
            }

            //Achive based on file size
            long fileSize = 0;
            try
            {
                FileInfo fileInfo = new(currentLogFile);

                if (fileInfo.Exists)
                {
                    fileSize = fileInfo.Length;
                }
            }
            catch { }

            if (IsArchiveByFileSizeRequired(fileLoggerOptions.ArchiveFileSizeThreshold, fileSize))
            {
                ArchiveFile(currentLogFile, fileLoggerOptions.ArchiveDirectoryPath, fileLoggerOptions.ArchiveFileName, now, _userName);
            }
        }

        public static bool IsArchiveByDateRequired(string archiveInterval, DateTime lastArchiveDate, DateTime now)
        {
            FileLoggerArchiveInterval fileLoggerArchiveInterval = Enum.Parse<FileLoggerArchiveInterval>(archiveInterval);

            if (fileLoggerArchiveInterval == FileLoggerArchiveInterval.None)
            {
                return false;
            }
            else if (fileLoggerArchiveInterval == FileLoggerArchiveInterval.Day)
            {
                if (now.Date > lastArchiveDate.Date)
                {
                    return true;
                }

                return false;
            }

            throw new NotImplementedException($"File logger archive interval '{archiveInterval}' is not supported.");
        }

        public static bool IsArchiveByFileSizeRequired(long fileSizeThreshold, long fileSize)
        {
            const long MIN_FILE_SIZE = 1_024;

            if (fileSizeThreshold <= MIN_FILE_SIZE)
            {
                return false;
            }

            if (fileSize >= fileSizeThreshold)
            {
                return true;
            }

            return false;
        }

        public static string BuildLogFilePath(string logFilePathTemplate, DateTime now, string userName)
        {
            string logFilePath = logFilePathTemplate;
            if (String.IsNullOrWhiteSpace(logFilePath))
            {
                throw new Exception($"Log file path not specified.");
            }

            string logDirectoryPath = Path.GetDirectoryName(logFilePath);
            string logFileName = ReplaceFileNamePlaceholders(Path.GetFileName(logFilePath), now, userName);

            return Path.Combine(logDirectoryPath, logFileName);
        }

        public static string ReplaceFileNamePlaceholders(string fileNameTemplate, DateTime now, string userName)
        {
            string yyyy = now.ToString("yyyy");
            string y = now.ToString("yy");
            string MM = now.ToString("MM");
            string M = now.ToString("M");
            string dd = now.ToString("dd");
            string d = now.ToString("d");

            string fileName = fileNameTemplate;
            if (String.IsNullOrWhiteSpace(fileName))
            {
                throw new Exception($"Argument name '{nameof(fileNameTemplate)}' is blank.");
            }

            fileName = fileName.Replace("%yyyy", yyyy);
            fileName = fileName.Replace("%y", y);
            fileName = fileName.Replace("%MM", MM);
            fileName = fileName.Replace("%M", M);
            fileName = fileName.Replace("%dd", dd);
            fileName = fileName.Replace("%d", d);
            fileName = fileName.Replace("%username", userName);

            return fileName;
        }

        private static void ArchiveFile(string sourceFilePath, string archiveDirectoryPath, string archiveFileNameTemplate, DateTime now, string userName)
        {
            if (!File.Exists(sourceFilePath))
            {
                return;
            }

            if (!Path.IsPathRooted(archiveDirectoryPath))
            {
                archiveDirectoryPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), archiveDirectoryPath);
            }

            Directory.CreateDirectory(archiveDirectoryPath);

            string archiveFileName = Path.GetFileName(sourceFilePath);
            if (!String.IsNullOrWhiteSpace(archiveFileNameTemplate))
            {
                archiveFileName = ReplaceFileNamePlaceholders(archiveFileNameTemplate, now, userName);
            }

            if (File.Exists(Path.Combine(archiveDirectoryPath, archiveFileName)))
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(archiveFileName);
                string fileExtension = Path.GetExtension(archiveFileName);

                for (int i = 0; i < 50; i++)
                {
                    archiveFileName = $"{fileNameWithoutExtension}({i + 1}){fileExtension}";
                    if (!File.Exists(Path.Combine(archiveDirectoryPath, archiveFileName)))
                    {
                        break;
                    }
                }

                if (File.Exists(Path.Combine(archiveDirectoryPath, archiveFileName)))
                {
                    archiveFileName = $"{fileNameWithoutExtension}({Guid.NewGuid()}){fileExtension}";
                }
            }

            string archiveFilePath = Path.Combine(archiveDirectoryPath, archiveFileName);


            //Do with retry
            int attempts = 10;

            for (int i = 1; i < attempts; i++)
            {
                try
                {
                    File.Move(sourceFilePath, archiveFilePath);
                    return;
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(GetFileAccessSleepTime(i));
                }
            }

            File.Move(sourceFilePath, archiveFilePath); //Final attempt
        }

        private static int GetFileAccessSleepTime(int attempt)
        {
            double time = 25d;
            double percent = 0.2d;

            for (int i = 1; i <= attempt; i++)
            {
                if (i == 1)
                    continue;

                time = time + (time * percent);
            }

            return (int)time;
        }

        public void Enqueue(string contents)
        {
            _queue.Writer.TryWrite(() =>
            {
                AppendTextToFile(GetLogFilePath(), contents);
            });
        }

        private async Task DequeueAllAsync(CancellationToken cancellationToken = default)
        {
            await foreach (Action action in _queue.Reader.ReadAllAsync(cancellationToken))
            {
                action();
            }
        }

        private static void AppendTextToFile(string filePath, string contents)
        {
            AppendTextToFile(filePath, contents, Encoding.UTF8);
        }

        private static void AppendTextToFile(string filePath, string contents, System.Text.Encoding encoding)
        {
            int attempts = 10;

            for (int i = 1; i < attempts; i++)
            {
                try
                {
                    bool fileExists = File.Exists(filePath);
                    File.AppendAllText(filePath, contents, encoding);
                    if (!fileExists)
                    {
                        File.SetCreationTime(filePath, DateTime.Now);
                    }
                    
                    return;
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(GetFileAccessSleepTime(i));
                }
            }

            File.AppendAllText(filePath, contents, encoding); //Final attempt
        }


        //private static void ArchiveOldFiles(FileLoggerOptions fileLoggerOptions)
        //{
        //    DateTime now = DateTime.Now;
        //    int maxFileCount = 31;
        //    IEnumerable<string> oldLogFilePaths = GetOldLogFilePaths(fileLoggerOptions, now, _userName, maxFileCount);

        //    foreach (string oldLogFilePath in oldLogFilePaths)
        //    {
        //        if (System.IO.File.Exists(oldLogFilePath))
        //        {
        //            ArchiveFile(oldLogFilePath, fileLoggerOptions.ArchiveDirectoryPath);
        //        }
        //    }
        //}

        //public static IEnumerable<string> GetOldLogFilePaths(FileLoggerOptions fileLoggerOptions, DateTime now, string userName, int maxFileCount)
        //{
        //    if (String.IsNullOrWhiteSpace(fileLoggerOptions.ArchiveDirectoryPath))
        //    {
        //        return Enumerable.Empty<string>();
        //    }

        //    if (String.IsNullOrWhiteSpace(fileLoggerOptions.LogFilePath))
        //    {
        //        throw new ArgumentException($"Property {nameof(fileLoggerOptions.LogFilePath)} not specified.");
        //    }

        //    List<string> logFilePaths = new();

        //    for (int i = 0; i < maxFileCount; i++)
        //    {
        //        bool isRolloverRequired = IsRolloverRequired(fileLoggerOptions, DateTime.MinValue, now.AddDays(-1 * (i + 1)));
        //        if (isRolloverRequired)
        //        {
        //            string oldLogFilePath = BuildLogFilePath(fileLoggerOptions.LogFilePath, now.AddDays(-1 * (i + 1)), userName);
        //            logFilePaths.Add(oldLogFilePath);
        //        }
        //    }

        //    return logFilePaths;
        //}


        public void Dispose()
        {
            _onChangeToken.Dispose();
        }
    }
}
