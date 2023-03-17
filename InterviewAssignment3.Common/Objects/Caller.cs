using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InterviewAssignment3.Common.Objects
{
    public class Caller
    {
        public string UserId { get; set; } = String.Empty;
        public string FromRemoteIpAddress { get; set; } = String.Empty;
        public string UserAgent { get; set; } = String.Empty;
    }
}
