using InterviewAssignment3.Common.Objects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewAssignment3.Common
{
    public static class BugRules
    {
        public static string RemoveUnicodeCharacters(string text)
        {
            //Source: https://stackoverflow.com/questions/123336/how-can-you-strip-non-ascii-characters-from-a-string-in-c

            string ascii = Encoding.ASCII.GetString(
                Encoding.Convert(
                    Encoding.UTF8,
                    Encoding.GetEncoding(
                        Encoding.ASCII.EncodingName,
                        new EncoderReplacementFallback(string.Empty),
                        new DecoderExceptionFallback()
                        ),
                    Encoding.UTF8.GetBytes(text)
                )
            );

            return ascii;
        }

        public static async Task ResetUsersAsync(ILogger logger, UserManager<ApplicationUser> userManager, List<ApplicationUser> applicationUsers)
        {
            applicationUsers.Clear();

            ApplicationUser applicationUser1 = new()
            {
                UserName = "john.smith",
                FirstName = "John",
                LastName = "Smith",
                EmailAddress = "john.smith@gmail.com",
                Phone = "416-467-2800",
                Street = "1201 Bathurst St.",
                City = "Toronto",
                Region = "ON",
                Postal = "M4B 0A3",
                Country = "Canada",
                IsAccountEnabled = true
            };
            string password1 = "Welcome$1";
            IdentityResult identityResult1 = await userManager.CreateAsync(applicationUser1, password1);

            if (identityResult1.Succeeded)
            {
                logger.LogInformation($"Adding user: {applicationUser1.UserName}");
            }
            else
            {
                foreach (IdentityError identityError in identityResult1.Errors)
                {
                    logger.LogError(identityError.Description);
                }
            }

            ApplicationUser applicationUser2 = new()
            {
                UserName = "MeeraBall",
                FirstName = "Meera",
                LastName = "Ball",
                EmailAddress = "MeeraBall@yahoo.com",
                Phone = "206-521-1380",
                Street = "Market Ave.",
                City = "Seattle",
                Region = "WA",
                Postal = "98101",
                Country = "United States",
                IsAccountEnabled = true
            };
            string password2 = "Welcome$2";
            IdentityResult identityResult2 = await userManager.CreateAsync(applicationUser2, password2);

            if (identityResult1.Succeeded)
            {
                logger.LogInformation($"Adding user: {applicationUser2.UserName}");
            }
            else
            {
                foreach (IdentityError identityError in identityResult2.Errors)
                {
                    logger.LogError(identityError.Description);
                }
            }
        }
    }
}
