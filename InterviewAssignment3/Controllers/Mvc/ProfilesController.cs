using InterviewAssignment3.Common.Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InterviewAssignment3.Controllers.Mvc
{
    [Authorize]
    public class ProfilesController : Controller
    {
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfilesController(ILogger<SignInController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public IActionResult Index([FromQuery] string userId)
        {
            if (String.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("Message", "Error", new { text = $"Invalid argument. Please try again." });
            }

            ApplicationUser? user = _userManager.FindByIdAsync(userId).Result;

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

        public IActionResult Password([FromQuery] string userId)
        {
            if (String.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("Message", "Error", new { text = $"Invalid argument. Please try again." });
            }

            ApplicationUser? user = _userManager.FindByIdAsync(userId).Result;

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

        public IActionResult Primary([FromQuery] string userId)
        {
            if (String.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("Message", "Error", new { text = $"Invalid argument. Please try again." });
            }

            ApplicationUser? user = _userManager.FindByIdAsync(userId).Result;

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

        public IActionResult Email([FromQuery] string userId)
        {
            if (String.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("Message", "Error", new { text = $"Invalid argument. Please try again." });
            }

            ApplicationUser? user = _userManager.FindByIdAsync(userId).Result;

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
