using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewAssignment3.DataTransfer.Objects
{
    public class PasswordProfile
    {
        public string UserId { get; set; } = String.Empty;
        public string CurrentPassword { get; set; } = String.Empty;
        public string NewPassword { get; set; } = String.Empty;
        public string ReEnteredPassword { get; set; } = String.Empty;
    }
}
