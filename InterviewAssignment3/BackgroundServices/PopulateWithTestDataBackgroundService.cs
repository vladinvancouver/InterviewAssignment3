using InterviewAssignment3.Common;
using InterviewAssignment3.Common.Objects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipeline.BackgroundServices
{
    /// <summary>
    /// This classs checks for feature support the application depends on.
    /// </summary>
    public class PopulateWithTestDataBackgroundService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public PopulateWithTestDataBackgroundService(ILogger<PopulateWithTestDataBackgroundService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //Give time for application to start up all services.
            try
            {
                //await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch { }

            try
            {
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    UserManager<ApplicationUser> userManager =
                        scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                    ApplicationUser applicationUser1 = new()
                    {
                        Id = 1.ToString(),
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

                    if (!identityResult1.Succeeded)
                    {
                        foreach (IdentityError identityError in identityResult1.Errors)
                        {
                            _logger.LogError(identityError.Description);
                        }
                    }

                    ApplicationUser applicationUser2 = new()
                    {
                        Id = 2.ToString(),
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
                    string password2 = "Hello$1";
                    IdentityResult identityResult2 = await userManager.CreateAsync(applicationUser2, password2);

                    if (!identityResult2.Succeeded)
                    {
                        foreach (IdentityError identityError in identityResult1.Errors)
                        {
                            _logger.LogError(identityError.Description);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            // Run your graceful clean-up actions
            await Task.CompletedTask;
        }
    }
}
