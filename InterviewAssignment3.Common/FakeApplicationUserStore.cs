using InterviewAssignment3.Common.Objects;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InterviewAssignment3.Common
{
    public class FakeApplicationUserStore : IUserStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>
    {
        private List<ApplicationUser> _users;

        public FakeApplicationUserStore(List<ApplicationUser> users)
        {
            _users = users ?? throw new ArgumentNullException(nameof(users));
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            _users.Add(user);
            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            _users.RemoveAll(o => o.NormalizedUserName == user.NormalizedUserName);
            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            ApplicationUser? applicationUser = _users.FirstOrDefault(o => o.Id.ToUpperInvariant() == userId.ToUpperInvariant());
            return await Task.FromResult(applicationUser);
        }

        public async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            ApplicationUser? applicationUser = _users.FirstOrDefault(o => o.NormalizedUserName == normalizedUserName);
            return await Task.FromResult(applicationUser);

        }

        public async Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return await Task.FromResult(user.NormalizedUserName);
        }

        public async Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return await Task.FromResult(user.PasswordHash);
        }

        public async Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return await Task.FromResult(user.Id);
        }

        public async Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return await Task.FromResult(user.UserName);
        }

        public async Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return await Task.FromResult(!String.IsNullOrWhiteSpace(user.PasswordHash));
        }

        public async Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName ?? String.Empty;
            await Task.CompletedTask;
        }

        public async Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash ?? String.Empty;
            await Task.CompletedTask;
        }

        public async Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
        {
            user.UserName = userName ?? String.Empty;
            await Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            int index = _users.FindIndex(o => o.NormalizedUserName == user.NormalizedUserName);
            if (index >= 0)
            {
                _users[index] = user;
                return await Task.FromResult(IdentityResult.Success);
            }
            else
            {
                return await Task.FromResult(IdentityResult.Failed(new IdentityError() { Code = "UserNotFound", Description = "User not found." }));
            }
        }

        public void Dispose()
        {

        }
    }
}
