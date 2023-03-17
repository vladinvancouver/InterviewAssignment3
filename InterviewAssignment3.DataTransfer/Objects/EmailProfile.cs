using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewAssignment3.DataTransfer.Objects
{
    public class EmailProfile
    {
        public string UserId { get; set; } = String.Empty;
        public string NewEmailAddress { get; set; } = String.Empty;
        public string ReEnteredEmailAddress { get; set; } = String.Empty;
    }
}
