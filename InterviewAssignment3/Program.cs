using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using InterviewAssignment3.Middleware;
using InterviewAssignment3.Common.Objects;
using Microsoft.AspNetCore.Authentication.Cookies;
using InterviewAssignment3.Logging;
using InterviewAssignment3.Common;
using Microsoft.AspNetCore.Identity;
using Pipeline.BackgroundServices;
using InterviewAssignment3.Common.Services;
using Microsoft.AspNetCore.Hosting.Server.Features;

ILogger _logger = CreateBootstrapLogger(args);

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder();

    builder.Logging.ClearProviders();
    builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"))
        .AddConsole()
        .AddFileLogger(loggerConfig =>
        {
            loggerConfig.LogFilePath = builder.Configuration["LogFilePath"] ?? throw new ArgumentNullException("Configuration 'LogFilePath' is missing.");
            loggerConfig.ArchiveDirectoryPath = builder.Configuration["ArchiveDirectoryPath"] ?? throw new ArgumentNullException("Configuration 'ArchiveDirectoryPath' is missing.");
            loggerConfig.ArchiveFileName = builder.Configuration["ArchiveFileName"] ?? throw new ArgumentNullException("Configuration 'ArchiveFileName' is missing.");
            loggerConfig.ArchiveInterval = builder.Configuration["ArchiveInterval"] ?? throw new ArgumentNullException("Configuration 'ArchiveInterval' is missing.");
            loggerConfig.ArchiveFileSizeThreshold = Int64.Parse(builder.Configuration["ArchiveFileSizeThreshold"] ?? throw new ArgumentNullException("Configuration 'ArchiveFileSizeThreshold' is missing."));
        });

    builder.Services.Configure<IdentityOptions>(options =>
    {
        // Default Password settings.
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;
    });

    var hostingConfig = new ConfigurationBuilder()
         .AddJsonFile("hosting.json", optional: false)
         .Build();

    ConfigureConfiguration(builder.Configuration);
    ConfigureServices(builder.Configuration, builder.Services);

    _logger.LogInformation("Preparing the runtime environment.");
    WebApplication app = builder.Build();

    _logger = app.Services.GetRequiredService<ILogger<Program>>();

    ConfigureLifetime(_logger, app, app.Lifetime);
    ConfigureMiddleware(_logger, app, app.Services, app.Environment);
    ConfigureEndpoints(_logger, app, app.Services);

    _logger.LogInformation("Running the application.");
    app.Run();
}
catch (Exception e)
{
    _logger.LogError(e, e.Message);
    throw;
}

void ConfigureConfiguration(ConfigurationManager configuration)
{

}

void ConfigureServices(ConfigurationManager configuration, IServiceCollection services)
{
    services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
            builder => builder
                .SetIsOriginAllowed(s => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
            );
    });

    services.AddAuthentication(Microsoft.AspNetCore.Server.HttpSys.HttpSysDefaults.AuthenticationScheme);
    services.AddMvc(options =>
    {
        options.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

    // This is required to allow the api controller access to the HttpContext.
    services.AddHttpContextAccessor();

    services.AddScoped(typeof(Caller));

    //This creates and registers UserManager<TUser>
    //https://github.com/aspnet/Identity/blob/master/src/Core/IdentityServiceCollectionExtensions.cs
    services.AddIdentityCore<ApplicationUser>(options => { });

    List<ApplicationUser> applicationUsers = new List<ApplicationUser>();
    services.AddSingleton(typeof(List<ApplicationUser>), applicationUsers);
    services.AddScoped<IUserStore<ApplicationUser>>(serviceProvider =>
    {
        return new MemoryApplicationUserStore(applicationUsers);
    });

    services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.LoginPath = "/SignIn";
            //options.Events.OnRedirectToLogin = context =>
            //{
            //    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            //    return Task.CompletedTask;
            //};
        });

    //For multipart file upload
    services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(x =>
    {
        x.ValueLengthLimit = int.MaxValue;
        x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart file upload
    });

    JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
    jsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    services.AddSingleton(jsonSerializerOptions);
    services.AddSingleton<IHostedService, PopulateWithTestDataBackgroundService>();
    services.AddSingleton(new EnvironmentService() { MachineName = System.Environment.MachineName, ProcessName = Process.GetCurrentProcess().ProcessName, ProcessId = Process.GetCurrentProcess().Id, RunningAsUser = $@"{System.Environment.UserDomainName}\{System.Environment.UserName}" });
}

void ConfigureLifetime(Microsoft.Extensions.Logging.ILogger logger, IApplicationBuilder app, IHostApplicationLifetime appLife)
{
    appLife.ApplicationStarted.Register(() => ApplicationStarted(logger, app, appLife));
    appLife.ApplicationStopping.Register(() => ApplicationStopping(logger, app));
    appLife.ApplicationStopped.Register(() => ApplicationStopped(logger));

    void ApplicationStarted(Microsoft.Extensions.Logging.ILogger logger, IApplicationBuilder app, IHostApplicationLifetime appLife)
    {
        logger.LogInformation($"Application starting on process ID {System.Diagnostics.Process.GetCurrentProcess().Id} and name '{System.Diagnostics.Process.GetCurrentProcess().ProcessName}'");

        // Access the IServiceProvider
        IServiceProvider serviceProvider = app.ApplicationServices;

        IServerAddressesFeature? serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
        if (serverAddressesFeature is { })
        {
            foreach (string address in serverAddressesFeature.Addresses)
            {
                logger.LogInformation($"Server is running on {address}");
            }
        }
    }

    void ApplicationStopping(Microsoft.Extensions.Logging.ILogger logger, IApplicationBuilder app)
    {
        logger.LogInformation($"Application stopping on process ID {System.Diagnostics.Process.GetCurrentProcess().Id}.");
    }


    void ApplicationStopped(Microsoft.Extensions.Logging.ILogger logger)
    {
        logger.LogInformation($"Application stopped on process ID {System.Diagnostics.Process.GetCurrentProcess().Id}.");
    }
}

void ConfigureMiddleware(ILogger logger, IApplicationBuilder app, IServiceProvider services, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
    }

    app.UseCors("CorsPolicy");

    app.UseAuthentication();
    app.UseStatusCodePages();
    app.UseRouting();
    app.UseAuthorization();
    app.UseStaticFiles();

    app.Use(async (context, next) =>
    {
        await next();

        if (context.Response.StatusCode == 401 || context.Response.StatusCode == 403)
        {
            logger.LogError($"User '{(context.User?.Identity?.Name == null ? "Anonymous" : context.User.Identity.Name)}' received status code '{context.Response.StatusCode}' to path '{context.Request.Path}'.");
        }
    });

    app.UseCaller();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
    });
}

void ConfigureEndpoints(ILogger logger, IEndpointRouteBuilder app, IServiceProvider services)
{
    
}

/// <summary>
/// A bootstrap logger is a temporary logger you crate very early in the application lifecycle, before the full host and dependecncy incject (DI) container are built.
/// </summary>
static ILogger CreateBootstrapLogger(string[] args)
{
    // This method must return some kind of logger without throwing an error.

    // Try to create as lcose to fully functional logger as possible.
    try
    {
        string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        IConfigurationRoot earlyConfiguration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build(); //same sources/order as the default builder

        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole()
                .AddFileLogger(loggerConfig =>
                {
                    loggerConfig.LogFilePath = earlyConfiguration["LogFilePath"] ?? throw new ArgumentNullException("Configuration 'LogFilePath' is missing.");
                    loggerConfig.ArchiveDirectoryPath = earlyConfiguration["ArchiveDirectoryPath"] ?? throw new ArgumentNullException("Configuration 'ArchiveDirectoryPath' is missing.");
                    loggerConfig.ArchiveFileName = earlyConfiguration["ArchiveFileName"] ?? throw new ArgumentNullException("Configuration 'ArchiveFileName' is missing.");
                    loggerConfig.ArchiveInterval = earlyConfiguration["ArchiveInterval"] ?? throw new ArgumentNullException("Configuration 'ArchiveInterval' is missing.");
                    loggerConfig.ArchiveFileSizeThreshold = Int64.Parse(earlyConfiguration["ArchiveFileSizeThreshold"] ?? throw new ArgumentNullException("Configuration 'ArchiveFileSizeThreshold' is missing."));
                });
        });

        return loggerFactory.CreateLogger(nameof(Program));
    }
    catch (Exception e)
    {
        Console.WriteLine($"Failed to create a fully functional bootstrap logger. Will try to create a simpler bootstrap logger. {e.Message}");
    }

    // Now, try to create a dumb down version of a logger.
    try
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole()
                .AddFileLogger(loggerConfig =>
                {
                    loggerConfig.LogFilePath = System.IO.Path.Combine(AppContext.BaseDirectory, "log.txt");
                    loggerConfig.ArchiveDirectoryPath = System.IO.Path.Combine(AppContext.BaseDirectory, "archive");
                    loggerConfig.ArchiveFileName = "log.%yyyy-%%MM-%dd.txt";
                    loggerConfig.ArchiveInterval = "Day";
                    loggerConfig.ArchiveFileSizeThreshold = 0;
                });
        });
    }
    catch (Exception e)
    {
        Console.WriteLine($"Failed to create a dubm down bootstrap logger. Will try to create a simpler bootstrap logger. {e.Message}");
    }

    // At this point, just create the simplest possible logger.
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole();
        });

        return loggerFactory.CreateLogger(nameof(Program));
    }
}