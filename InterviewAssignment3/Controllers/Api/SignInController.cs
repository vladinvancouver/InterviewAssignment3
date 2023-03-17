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

        public SignInController(ILogger<SignInController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

//#if DEBUG
        //[Route("Register")]
        //[HttpGet]
        //public async Task<IActionResult> Register()
        //{
        //    //This method is for testing only
        //    if (System.Diagnostics.Debugger.IsAttached || System.Net.IPAddress.IsLoopback(Request.HttpContext.Connection.RemoteIpAddress))
        //    {
        //        Models.DeltaUser applicationUser1 = new Models.DeltaUser()
        //        {
        //            Id = Guid.NewGuid().ToString(),
        //            UserName = "1234567",
        //            DisplayName = "Vlad 'Principal' Alexander",
        //            IsAccountEnabled = true
        //        };
        //        string password1 = "Hello101$";
        //        IdentityResult identityResult1 = await _userManager.CreateAsync(applicationUser1, password1);

        //        if (!identityResult1.Succeeded)
        //        {
        //            return Ok(identityResult1.Errors);
        //        }

        //        Models.DeltaUser applicationUser2 = new Models.DeltaUser()
        //        {
        //            Id = Guid.NewGuid().ToString(),
        //            UserName = "7654321",
        //            DisplayName = "Vlad 'Manager' Alexander",
        //            IsAccountEnabled = true
        //        };
        //        string password2 = "Hello101$";
        //        IdentityResult identityResult2 = await _userManager.CreateAsync(applicationUser2, password2);

        //        if (!identityResult2.Succeeded)
        //        {
        //            return Ok(identityResult2.Errors);
        //        }


        //        return Ok("Done");
        //    }
        //    else
        //    {
        //        return BadRequest("This method is for testing only.");
        //    }
        //}

        //[Route("Clear")]
        //[HttpGet]
        //public async Task<IActionResult> Clear()
        //{
        //    if (System.Diagnostics.Debugger.IsAttached || System.Net.IPAddress.IsLoopback(Request.HttpContext.Connection.RemoteIpAddress))
        //    {
        //        Models.DeltaUser applicationUser1 = await _userManager.FindByNameAsync("1234567");
        //        if (applicationUser1 != null)
        //            await _userManager.DeleteAsync(applicationUser1);

        //        Models.DeltaUser applicationUser2 = await _userManager.FindByNameAsync("7654321");
        //        if (applicationUser2 != null)
        //            await _userManager.DeleteAsync(applicationUser2);

        //        return Ok("Done");
        //    }
        //    else
        //    {
        //        return BadRequest("This method is for testing only.");
        //    }
        //}
//#endif

        [Route("SignInWithCredentials")]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> SignInWithCredentials([FromBody] DataTransfer.Objects.Credentials credentials)
        {
            try
            {
                string genericMessageUnableToSignIn = "Invalid username or password.";

                if (credentials == null)
                {
                    return BadRequest($"Cannot read credentials.");
                }

                if (String.IsNullOrWhiteSpace(credentials.Username))
                {
                    return Ok(new DataTransfer.Objects.ValidationResult() { IsValid = false, Message = "Username cannot be blank." });
                }

                if (String.IsNullOrWhiteSpace(credentials.Password))
                {
                    return Ok(new DataTransfer.Objects.ValidationResult() { IsValid = false, Message = "Password cannot be blank." });
                }

                var user = await _userManager.FindByNameAsync(credentials.Username);

                if (user == null)
                {
                    //We do not want to reveal to the user that the username does not exist.
                    _logger.LogInformation($"Cannot find user account for username '{credentials.Username}'. Username was manually entered.");
                    return Ok(new DataTransfer.Objects.ValidationResult() { IsValid = false, Message = genericMessageUnableToSignIn });
                }

                if (!user.IsAccountEnabled)
                {
                    //We do not want to reveal to the user that the account is not enabled.
                    _logger.LogInformation($"Account is not enabled for username '{credentials.Username}'.");
                    return Ok(new DataTransfer.Objects.ValidationResult() { IsValid = false, Message = genericMessageUnableToSignIn });
                }

                //Note, it is important to let users know that their account is locked out. But this message can be used by hackers to
                //to guess account names.
                if (user.IsAccountLockedOut)
                {
                    _logger.LogInformation($"Account is locked out for username '{credentials.Username}'.");
                    return Ok(new DataTransfer.Objects.ValidationResult() { IsValid = false, Message = "Your account is locked out. This may occur if there were several failed login attempts. Wait a while and try again. If this persists, please contact your System Administrator." });
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
                    return Ok(new DataTransfer.Objects.ValidationResult() { IsValid = true, Message = "Successul sign in." });
                }

                _logger.LogInformation($"Failed sign in attempt for username '{credentials.Username}'. Likely cause is incorrect password.");
                return Ok(new DataTransfer.Objects.ValidationResult() { IsValid = false, Message = genericMessageUnableToSignIn });
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