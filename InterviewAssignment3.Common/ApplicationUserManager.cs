using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;
using InterviewAssignment3.Common.Objects;
using Microsoft.AspNetCore.Authentication;

namespace InterviewAssignment3.Common
{
    //Source for UserManager:
    //https://github.com/aspnet/Identity/blob/master/src/Core/UserManager.cs
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators, IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public override Task<bool> HasPasswordAsync(ApplicationUser user)
        {
            return base.HasPasswordAsync(user);
        }

        public override Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            return base.CheckPasswordAsync(user, password);
        }

        protected override Task<PasswordVerificationResult> VerifyPasswordAsync(IUserPasswordStore<ApplicationUser> store, ApplicationUser user, string password)
        {
            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, System.Environment.UserDomainName))
            {
                bool isValid = pc.ValidateCredentials(user.NormalizedUserName, password, ContextOptions.Negotiate);

                if (isValid)
                {
                    return Task.FromResult(PasswordVerificationResult.Success);
                }
                else
                {
                    return Task.FromResult(PasswordVerificationResult.Failed);
                }
            }
        }
    }
}
