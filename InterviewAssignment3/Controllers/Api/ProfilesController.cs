using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using InterviewAssignment3.Common;
using InterviewAssignment3.Common.Objects;
using InterviewAssignment3.DataTransfer.Objects;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InterviewAssignment3.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfilesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfilesController(ILogger<SignInController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [Route("GetCompleteProfile")]
        [HttpGet]
        public async Task<IActionResult> GetCompleteProfileAsync([FromQuery] string userId)
        {
            try
            {
                ApplicationUser? user = await _userManager.FindByIdAsync(userId);

                if (user is null)
                {
                    _logger.LogWarning($"User ID '{userId}' not found.");
                    throw new ArgumentException("Profile not found.");
                }

                DataTransfer.Objects.Profile profile = new()
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    EmailAddress = user.EmailAddress,
                    UnconfirmedEmailAddress = user.UnconfirmedEmailAddress,
                    Phone = user.Phone,
                    Street = user.Street,
                    City = user.City,
                    Region = user.Region,
                    Postal = user.Postal,
                    Country = user.Country
                };

                return Ok(profile);
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        [Route("SavePasswordProfile")]
        [HttpPut]
        public async Task<IActionResult> SavePasswordProfileAsync([FromBody] DataTransfer.Objects.PasswordProfile profile)
        {
            try
            {
                if (profile is null)
                {
                    throw new ArgumentException($"Cannot read profile.");
                }

                ApplicationUser? user = await _userManager.FindByIdAsync(profile.UserId);

                if (user is null)
                {
                    _logger.LogWarning($"User ID '{profile.UserId}' not found.");
                    throw new ArgumentException("Profile not found.");
                }

                if (!user.IsAccountEnabled)
                {
                    throw new ArgumentException($"Profile cannot be updated.");
                }

                if (String.IsNullOrWhiteSpace(profile.CurrentPassword))
                {
                    throw new ArgumentException($"'Current password' cannot be blank.");
                }

                if (String.IsNullOrWhiteSpace(profile.NewPassword))
                {
                    throw new ArgumentException($"'New password' cannot be blank.");
                }

                if (profile.NewPassword.Length > 100)
                {
                    throw new ArgumentException($"'Password' must 100 characters or less.");
                }

                if (profile.NewPassword.ToUpperInvariant() != profile.ReEnteredPassword.ToUpperInvariant())
                {
                    throw new ArgumentException($"Passwords entered do not match.");
                }

                //Note, it is important to let users know that their account is locked out. But this message can be used by hackers to
                //to guess account names.
                if (user.IsAccountLockedOut)
                {
                    throw new ArgumentException("Your account is locked out. This may occur if there were several failed login attempts. Wait a while and try again. If this persists, please contact Support.");
                }

                IdentityResult identityResult = await _userManager.ChangePasswordAsync(user, profile.CurrentPassword, profile.NewPassword);
                if (identityResult.Succeeded)
                {
                    return Ok();
                }
                else
                {
                    string errorMessage = String.Join(' ', identityResult.Errors.Select(obj => obj.Description));

                    if (String.IsNullOrWhiteSpace(errorMessage))
                    {
                        throw new ArgumentException("Failed to change password. Please try again.");
                    }
                    else
                    {
                        throw new ArgumentException(errorMessage);
                    }
                }
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        [Route("SavePrimaryProfile")]
        [HttpPut]
        public async Task<IActionResult> SavePrimaryProfileAsync([FromBody] DataTransfer.Objects.PrimaryProfile profile)
        {
            try
            {
                if (profile is null)
                {
                    throw new ArgumentException($"Cannot read profile.");
                }

                ApplicationUser? user = await _userManager.FindByIdAsync(profile.UserId);

                if (user is null)
                {
                    _logger.LogWarning($"User ID '{profile.UserId}' not found.");
                    throw new ArgumentException("Profile not found.");
                }

                if (!user.IsAccountEnabled)
                {
                    throw new ArgumentException($"Profile cannot be updated.");
                }

                if (String.IsNullOrWhiteSpace(profile.FirstName))
                {
                    throw new ArgumentException($"'First name' cannot be blank.");
                }

                if (profile.FirstName.Length > 100)
                {
                    throw new ArgumentException($"'First name' must 100 characters or less.");
                }

                if (String.IsNullOrWhiteSpace(profile.LastName))
                {
                    throw new ArgumentException($"'Last name' cannot be blank.");
                }

                if (profile.LastName.Length > 100)
                {
                    throw new ArgumentException($"'Last name' must 100 characters or less.");
                }

                //Note, it is important to let users know that their account is locked out. But this message can be used by hackers to
                //to guess account names.
                if (user.IsAccountLockedOut)
                {
                    throw new ArgumentException("Your account is locked out. This may occur if there were several failed login attempts. Wait a while and try again. If this persists, please contact Support.");
                }

                //Simulate storage that does not support Unicode.

                user.FirstName = BugRules.RemoveUnicodeCharacters(profile.FirstName);
                user.LastName = BugRules.RemoveUnicodeCharacters(profile.LastName);
                user.Phone = BugRules.RemoveUnicodeCharacters(profile.Phone);
                user.Street = BugRules.RemoveUnicodeCharacters(profile.Street);
                //Bug
                //user.City = BugRules.RemoveUnicodeCharacters((profile.City);
                user.Region = BugRules.RemoveUnicodeCharacters(profile.Region);
                user.Postal = BugRules.RemoveUnicodeCharacters(profile.Postal);
                user.Country = BugRules.RemoveUnicodeCharacters(profile.Country);
                await _userManager.UpdateAsync(user);

                return Ok();
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        [Route("SaveEmailProfile")]
        [HttpPut]
        public async Task<IActionResult> SaveEmailProfileAsync([FromBody] DataTransfer.Objects.EmailProfile profile)
        {
            try
            {
                if (profile is null)
                {
                    throw new ArgumentException($"Cannot read profile.");
                }

                ApplicationUser? user = await _userManager.FindByIdAsync(profile.UserId);

                if (user is null)
                {
                    _logger.LogWarning($"User ID '{profile.UserId}' not found.");
                    throw new ArgumentException("Profile not found.");
                }

                if (!user.IsAccountEnabled)
                {
                    throw new ArgumentException($"Profile cannot be updated.");
                }

                if (String.IsNullOrWhiteSpace(profile.NewEmailAddress))
                {
                    throw new ArgumentException($"'New email name' cannot be blank.");
                }

                if (profile.NewEmailAddress.Length > 100)
                {
                    throw new ArgumentException($"'New email name' must 100 characters or less.");
                }

                if (profile.NewEmailAddress.ToUpperInvariant() != profile.ReEnteredEmailAddress.ToUpperInvariant())
                {
                    throw new ArgumentException($"Email addresses entered do not match.");
                }

                //Note, it is important to let users know that their account is locked out. But this message can be used by hackers to
                //to guess account names.
                if (user.IsAccountLockedOut)
                {
                    throw new ArgumentException("Your account is locked out. This may occur if there were several failed login attempts. Wait a while and try again. If this persists, please contact Support.");
                }

                user.UnconfirmedEmailAddress = BugRules.RemoveUnicodeCharacters(profile.NewEmailAddress);
                await _userManager.UpdateAsync(user);

                return Ok();
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }
    }
}