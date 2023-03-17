using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InterviewAssignment3.Common.Services
{
    public class EnvironmentService
    {
        public string MachineName { get; set; } = string.Empty;

        public string ProcessName { get; set; } = string.Empty;

        public int ProcessId { get; set; }

        public string RunningAsUser { get; set; } = string.Empty;
    }
}
