using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewAssignment3.DataTransfer.Objects
{
    public class Profile
    {
        public string UserId { get; set; } = String.Empty;
        public string UserName { get; set; } = String.Empty;
        public string FirstName { get; set; } = String.Empty;
        public string LastName { get; set; } = String.Empty;
        public string EmailAddress { get; set; } = String.Empty;
        public string UnconfirmedEmailAddress { get; set; } = String.Empty;
        public string Phone { get; set; } = String.Empty;
        public string Street { get; set; } = String.Empty;
        public string City { get; set; } = String.Empty;
        public string Region { get; set; } = String.Empty;
        public string Postal { get; set; } = String.Empty;
        public string Country { get; set; } = String.Empty;
    }
}
