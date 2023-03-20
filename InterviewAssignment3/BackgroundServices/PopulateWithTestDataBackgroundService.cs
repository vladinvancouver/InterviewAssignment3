using InterviewAssignment3.Common;
using InterviewAssignment3.Common.Objects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pipeline.BackgroundServices
{
    /// <summary>
    /// This service creates demo users/login accounts.
    /// </summary>
    public class PopulateWithTestDataBackgroundService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<ApplicationUser> _applicationUsers;

        public PopulateWithTestDataBackgroundService(ILogger<PopulateWithTestDataBackgroundService> logger, IServiceProvider serviceProvider, List<ApplicationUser> applicationUsers)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _applicationUsers = applicationUsers ?? throw new ArgumentNullException(nameof(applicationUsers));
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

                    await BugRules.ResetUsersAsync(_logger, userManager, _applicationUsers);
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
