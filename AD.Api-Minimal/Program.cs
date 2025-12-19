using AD.Api.Core.Settings;
using AD.Api.Http;
using AD.Api.Startup;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NLog.Extensions.Logging;
using WebApp = Microsoft.AspNetCore.Builder.WebApplication;

var builder = WebApp.CreateBuilder(new WebApplicationOptions
{
	Args = args,
	EnvironmentName = AD.Api.WebEnv.EnvironmentName,
	ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
})
.SetupEnvironment();

builder.Host.ConfigureHostOptions(o => o.ServicesStartConcurrently = true);

builder.Logging.ClearProviders()
			   .SetMinimumLevel(builder.Configuration.GetRequiredSection("Logging").GetRequiredSection("LogLevel").GetValue("Default", LogLevel.Information))
			   .Configure(o =>
				{
					o.ActivityTrackingOptions =
						ActivityTrackingOptions.TraceId |
						ActivityTrackingOptions.SpanId |
						ActivityTrackingOptions.ParentId;
				})
				.AddNLog(new NLogProviderOptions
				{
					IncludeScopes = true,
					CaptureMessageProperties = true,
					AutoShutdown = true,
					CaptureMessageTemplates = true,
				});

// Add Settings Reader for environment variable translation.
builder.Services.AddSettingsReader(out ISettingsReader settingsReader);

builder.Services.Configure<RouteOptions>(options =>
{
	options.ConstraintMap.Add(SidRouteConstraint.ConstraintName, typeof(SidRouteConstraint));
});

builder.Services.AddValidation()
				.AddProblemDetails(x => x.CustomizeProblemDetails = ctx =>
				{
					const string correlationId = "correlationId";
					_ = ctx.ProblemDetails.Extensions.TryAdd(correlationId, ctx.HttpContext.TraceIdentifier);
				});

builder.Services.TryAddSingleton(TimeProvider.System);
