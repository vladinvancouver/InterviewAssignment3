using System;
using System.Collections.Generic;
using System.Text;

namespace InterviewAssignment3.Logging
{
    public enum FileLoggerArchiveInterval
    {
        /// <summary>
        /// Don't archive based on time
        /// </summary>
        None = 0,

        /// <summary>
        /// Archive daily
        /// </summary>
        Day = 1
    }
}
