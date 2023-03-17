using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewAssignment3.Logging
{
    public class FileLoggerOptions
    {
        private string _logFilePath = String.Empty;
        private string _archiveDirectoryPath = String.Empty;
        private string _archiveInterval = FileLoggerArchiveInterval.None.ToString();

        public string LogFilePath
        {
            get
            {
                return _logFilePath;
            }
            set
            {
                if (Path.IsPathRooted(value))
                {
                    _logFilePath = value;
                }
                else
                {
                    _logFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), value);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath));
            }
        }

        public string ArchiveDirectoryPath
        {
            get
            {
                return _archiveDirectoryPath;
            }
            set
            {
                if (Path.IsPathRooted(value))
                {
                    _archiveDirectoryPath = value;
                }
                else
                {
                    _archiveDirectoryPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), value);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(_archiveDirectoryPath));
            }
        }

        public string ArchiveFileName { get; set; } = String.Empty;

        public string ArchiveInterval
        {
            get
            {
                return _archiveInterval;
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    _archiveInterval = FileLoggerArchiveInterval.None.ToString();
                    return;
                }

                if (Enum.TryParse<FileLoggerArchiveInterval>(value, ignoreCase: true, out FileLoggerArchiveInterval result))
                {
                    _archiveInterval = result.ToString();
                    return;
                }

                throw new ArgumentException($"Property '{nameof(ArchiveInterval)}' does not support value '{value}'.");
            }
        }

        public long ArchiveFileSizeThreshold { get; set; } = 0;
    }
}
