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
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using InterviewAssignment3.Middleware;
using InterviewAssignment3.Common.Objects;
using Microsoft.AspNetCore.Authentication.Cookies;
using InterviewAssignment3.Logging;
using InterviewAssignment3.Common;
using Microsoft.AspNetCore.Identity;
using Pipeline.BackgroundServices;
using InterviewAssignment3.Common.Services;


//Examples: https://docs.microsoft.com/en-us/aspnet/core/migration/50-to-60-samples?view=aspnetcore-6.0


//Using DEBUG directives for now because static content not loading when launching from Visual Studio.


//UseWindowsService() sets ContentRootPath. Changing this value after the builder is created
//causes an error. So we need to set the ContentRootPath to the same value that UseWindowsService
//sets it to in order to avoid the error.
WebApplicationOptions webApplicationOptions = new()
{
    ContentRootPath = AppContext.BaseDirectory,
    Args = args,
    ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName
};

#if DEBUG
    var builder = WebApplication.CreateBuilder();
#else
    var builder = WebApplication.CreateBuilder(webApplicationOptions);
#endif

builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"))
    .AddConsole()
    .AddFileLogger(loggerConfig =>
    {
        loggerConfig.LogFilePath = builder.Configuration["LogFilePath"];
        loggerConfig.ArchiveDirectoryPath = builder.Configuration["ArchiveDirectoryPath"];
        loggerConfig.ArchiveFileName = builder.Configuration["ArchiveFileName"];
        loggerConfig.ArchiveInterval = builder.Configuration["ArchiveInterval"];
        loggerConfig.ArchiveFileSizeThreshold = Int64.Parse(builder.Configuration["ArchiveFileSizeThreshold"]);
    });

var hostingConfig = new ConfigurationBuilder()
     .AddJsonFile("hosting.json", optional: false)
     .Build();

string urls = hostingConfig.GetValue<string>("urls");

#pragma warning disable CA1416 // Validate platform compatibility

#if !DEBUG
builder.Host.UseWindowsService();
#endif


builder.WebHost.UseHttpSys(options =>
    {
        ConfigureHttpSys(options);
    })
    .UseConfiguration(hostingConfig);  //Will read "urls" key from hosting.json
#pragma warning restore CA1416 // Validate platform compatibility

ConfigureConfiguration(builder.Configuration);
ConfigureServices(builder.Configuration, builder.Services);

var app = builder.Build();

Microsoft.Extensions.Logging.ILogger logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation($"Starting web server on {urls}");

ConfigureLifetime(logger, app, app.Lifetime);
ConfigureMiddleware(logger, app, app.Services, app.Environment);
ConfigureEndpoints(logger, app, app.Services);

app.Run();

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
    services.AddScoped<IUserStore<ApplicationUser>>(serviceProvider =>
    {
        return new FakeApplicationUserStore(applicationUsers);
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

    //var repositoryInterfaceTypeToRepositoryTypeMapping = UnitOfWorkHelper.GetRepositoryInterfaceTypeToRepositoryTypeMapping(prefix: "Sql", suffix: "Repository");

    ////The two connection strings need to be different so that corresponding connections are pooled separately because of an ADO bug where
    ////some settings from transaction connections do not reset.
    ////https://github.com/dotnet/SqlClient/issues/96
    //string forGeneralUseConnectionStringKey = "ForGeneralUse";
    //string forUseWithTransactionsConnectionStringKey = "ForUseWithTransactions";
    //string forGeneralUseConnectionString = Microsoft.Extensions.Configuration.ConfigurationExtensions.GetConnectionString(configuration, forGeneralUseConnectionStringKey);
    //string forUseWithTransactionsConnectionString = Microsoft.Extensions.Configuration.ConfigurationExtensions.GetConnectionString(configuration, forUseWithTransactionsConnectionStringKey);

    //if (forGeneralUseConnectionString == forUseWithTransactionsConnectionString)
    //{
    //    throw new Exception($"Connection string '{forGeneralUseConnectionStringKey}' must be different than '{forUseWithTransactionsConnectionStringKey}' so that a different connection pool will be created for each connection string.");
    //}

    //services.AddSingleton(serviceProvider =>
    //{
    //    IUnitOfWork forGeneralUseUnitOfWork = new SqlUnitOfWork(serviceProvider, forGeneralUseConnectionString, repositoryInterfaceTypeToRepositoryTypeMapping) { WarningThresholdInMilliseconds = configSettings.DatabaseExecutionDurationWarningThresholdInMilliseconds };
    //    IUnitOfWork forUseWithTransactionsUnitOfWork = new SqlUnitOfWork(serviceProvider, forUseWithTransactionsConnectionString, repositoryInterfaceTypeToRepositoryTypeMapping);
    //    InterviewAssignment3.DataAccess.Storage storage = new InterviewAssignment3.DataAccess.Storage(forGeneralUseUnitOfWork, forUseWithTransactionsUnitOfWork, configSettings.MachineAffinitiesCacheExpirationInSeconds, configSettings.ExecutingCommandsCacheExpirationInSeconds);
    //    return storage;
    //});

    services.AddSingleton<IHostedService, PopulateWithTestDataBackgroundService>();
    services.AddSingleton(new EnvironmentService() { MachineName = System.Environment.MachineName, ProcessName = Process.GetCurrentProcess().ProcessName, ProcessId = Process.GetCurrentProcess().Id, RunningAsUser = $@"{System.Environment.UserDomainName}\{System.Environment.UserName}" });
}

void ConfigureLifetime(Microsoft.Extensions.Logging.ILogger logger, IApplicationBuilder app, IHostApplicationLifetime appLife)
{
    appLife.ApplicationStarted.Register(() => ApplicationStarted(logger));
    appLife.ApplicationStopping.Register(() => ApplicationStopping(logger, app));
    appLife.ApplicationStopped.Register(() => ApplicationStopped(logger));
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

void ApplicationStarted(Microsoft.Extensions.Logging.ILogger logger)
{
    logger.LogInformation($@"Application starting on process ID {Process.GetCurrentProcess().Id}, name '{Process.GetCurrentProcess().ProcessName}' and running as '{System.Environment.UserDomainName}\{System.Environment.UserName}'.");
}

void ApplicationStopping(Microsoft.Extensions.Logging.ILogger logger, IApplicationBuilder app)
{
    logger.LogInformation($"Application stopping on process ID {Process.GetCurrentProcess().Id}.");
}

void ApplicationStopped(Microsoft.Extensions.Logging.ILogger logger)
{
    logger.LogInformation($"Application stopped on process ID {Process.GetCurrentProcess().Id}.");
}


void ConfigureHttpSys(Microsoft.AspNetCore.Server.HttpSys.HttpSysOptions options, string? overrideUrl = null)
{
    //options.Authentication.Schemes = Microsoft.AspNetCore.Server.HttpSys.AuthenticationSchemes.NTLM | Microsoft.AspNetCore.Server.HttpSys.AuthenticationSchemes.Negotiate;
    //options.Authentication.AllowAnonymous = true;
    options.Authentication.Schemes = Microsoft.AspNetCore.Server.HttpSys.AuthenticationSchemes.None;
    options.Authentication.AllowAnonymous = true;
    options.MaxConnections = -1;
    options.MaxRequestBodySize = 100_000_000;
    if (!String.IsNullOrWhiteSpace(overrideUrl))
    {
        options.UrlPrefixes.Add(overrideUrl);
    }
}
