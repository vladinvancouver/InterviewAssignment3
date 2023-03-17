using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InterviewAssignment3.Common.Objects
{
    public class ApplicationUser
    {
        public string Id { get; set; } = String.Empty;
        public string UserName { get; set; } = String.Empty;
        public string NormalizedUserName { get; set; } = String.Empty;
        public string PasswordHash { get; set; } = String.Empty;
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
        public bool IsAccountLockedOut { get; set; }
        public bool IsAccountEnabled { get; set; }
        public bool IsAdministrator { get; set; }
    }
}
