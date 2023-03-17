using System;
using System.Collections.Generic;
using System.Text;

namespace InterviewAssignment3.DataTransfer.Objects
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = String.Empty;
    }
}
