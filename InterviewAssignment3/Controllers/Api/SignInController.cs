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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InterviewAssignment3.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class SignInController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly List<ApplicationUser> _applicationUsers;

        public SignInController(ILogger<SignInController> logger, UserManager<ApplicationUser> userManager, List<ApplicationUser> applicationUsers)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _applicationUsers = applicationUsers ?? throw new ArgumentNullException(nameof(applicationUsers));
        }

        [Route("Reset")]
        [HttpGet]
        public async Task<IActionResult> ResetAsync()
        {
            try
            {
                await BugRules.ResetUsersAsync(_logger, _userManager, _applicationUsers);
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

        [Route("SignInWithCredentials")]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> SignInWithCredentialsAsync([FromBody] DataTransfer.Objects.Credentials credentials)
        {
            try
            {
                string genericMessageUnableToSignIn = "Invalid username or password.";

                if (credentials == null)
                {
                    throw new ArgumentException("Credentials cannot be read.");
                }

                if (String.IsNullOrWhiteSpace(credentials.Username))
                {
                    throw new ArgumentException("'Username' cannot be blank.");
                }

                if (String.IsNullOrWhiteSpace(credentials.Password))
                {
                    throw new ArgumentException("'Password' cannot be blank.");
                }

                var user = await _userManager.FindByNameAsync(credentials.Username);

                if (user == null)
                {
                    //We do not want to reveal to the user that the username does not exist.
                    _logger.LogInformation($"Cannot find user account for username '{credentials.Username}'. Username was manually entered.");
                    throw new ArgumentException(genericMessageUnableToSignIn);
                }

                if (!user.IsAccountEnabled)
                {
                    //We do not want to reveal to the user that the account is not enabled.
                    _logger.LogInformation($"Account is not enabled for username '{credentials.Username}'.");
                    throw new ArgumentException(genericMessageUnableToSignIn);
                }

                //Note, it is important to let users know that their account is locked out. But this message can be used by hackers to
                //to guess account names.
                if (user.IsAccountLockedOut)
                {
                    _logger.LogInformation($"Account is locked out for username '{credentials.Username}'.");
                    throw new ArgumentException("Your account is locked out. This may occur if there were several failed login attempts. Wait a while and try again. If this persists, please contact your System Administrator.");
                }

                if (await _userManager.CheckPasswordAsync(user, credentials.Password))
                {
                    var claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties
                        {
                            IsPersistent = credentials.IsPersistent
                        });

                    _logger.LogInformation($"Successful sign in with username / password for username '{credentials.Username}'.");
                    return Ok();
                }

                _logger.LogInformation($"Failed sign in attempt for username '{credentials.Username}'. Likely cause is incorrect password.");
                throw new ArgumentException(genericMessageUnableToSignIn);
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

        [Route("Register")]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> RegisterAsync([FromBody] DataTransfer.Objects.NewUser newUser)
        {
            try
            {
                if (newUser == null)
                {
                    return BadRequest($"Cannot read user information.");
                }

                if (String.IsNullOrWhiteSpace(newUser.FirstName))
                {
                    throw new ArgumentException("'First name' cannot be blank.");
                }

                if (newUser.FirstName.Length > 100)
                {
                    throw new ArgumentException($"'Last name' must 100 characters or less.");
                }

                if (String.IsNullOrWhiteSpace(newUser.LastName))
                {
                    throw new ArgumentException("'Last name' cannot be blank.");
                }

                if (newUser.FirstName.Length > 100)
                {
                    throw new ArgumentException($"'Last name' must 100 characters or less.");
                }

                if (String.IsNullOrWhiteSpace(newUser.Username))
                {
                    throw new ArgumentException("'Username' cannot be blank.");
                }

                if (newUser.Username.Length > 100)
                {
                    throw new ArgumentException($"'Last name' must 100 characters or less.");
                }

                if (String.IsNullOrWhiteSpace(newUser.Password))
                {
                    throw new ArgumentException("'Password' cannot be blank.");
                }

                if (newUser.Password.Length > 100)
                {
                    throw new ArgumentException($"'Last name' must 100 characters or less.");
                }

                ApplicationUser applicationUser = new()
                {
                    UserName = newUser.Username,
                    FirstName = BugRules.RemoveUnicodeCharacters(newUser.FirstName),
                    LastName = BugRules.RemoveUnicodeCharacters(newUser.LastName),
                    EmailAddress = String.Empty,
                    Phone = String.Empty,
                    Street = String.Empty,
                    City = String.Empty,
                    Region = String.Empty,
                    Postal = String.Empty,
                    Country = String.Empty,
                    IsAccountEnabled = true
                };
                IdentityResult identityResult = await _userManager.CreateAsync(applicationUser, newUser.Password);

                if (!identityResult.Succeeded)
                {
                    string errorMessage = String.Join(' ', identityResult.Errors.Select(obj => obj.Description));

                    if (String.IsNullOrWhiteSpace(errorMessage))
                    {
                        throw new ArgumentException("Failed to register. Please try again.");
                    }
                    else
                    {
                        throw new ArgumentException(errorMessage);
                    }
                }

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