using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using InterviewAssignment3.Common.Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InterviewAssignment3.Controllers.Mvc
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<SignInController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [Route("")]
        [Route("[controller]")]
        [Route("[controller]/Index")]
        public IActionResult Index()
        {
            string userName = User?.Identity?.Name ?? String.Empty;

            if (String.IsNullOrWhiteSpace(userName))
            {
                return RedirectToAction("Index", "SignIn");
            }

            ApplicationUser? user = _userManager.FindByNameAsync(userName).Result;

            if (user is null)
            {
                return RedirectToAction("Message", "Error", new { text = $"User not found." });
            }

            ViewModels.UserInfo model = new()
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            return View(model);
        }
    }
}
