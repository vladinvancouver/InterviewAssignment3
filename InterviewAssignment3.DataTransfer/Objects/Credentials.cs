using System;
using System.Collections.Generic;
using System.Text;

namespace InterviewAssignment3.DataTransfer.Objects
{
    public class Credentials
    {
        public string Username { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
        public bool IsPersistent { get; set; }
    }
}
