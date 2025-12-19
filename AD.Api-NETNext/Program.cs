using AD.Api;
using AD.Api.Attributes;
using AD.Api.Constraints;
using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Filters;
using AD.Api.Core.Security.Encryption;
using AD.Api.Core.Serialization.Json.Converters;
using AD.Api.Core.Web;
using AD.Api.Core.Web.Validation;
using AD.Api.Expressions;
using AD.Api.Mapping;
using AD.Api.Middleware;
using AD.Api.Serialization.Json;
using AD.Api.Services;
using AD.Api.Services.Enums;
using AD.Api.Startup;
using AD.Api.Swagger.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Logging;
using NLog;
using NLog.Web;
using System.DirectoryServices.ActiveDirectory;

WebApplicationBuilder builder = StartupHelper.CreateWebBuilder(args);
var logger = LogManager.Setup()
					   .LoadConfigurationFromFile("config.nlog")
					   .GetCurrentClassLogger();

builder.Logging.ClearProviders();
builder.Host.UseNLog();

logger.Info("Starting application...");

try
{
	builder.Host.UseDefaultServiceProvider((context, options) =>
	{
		bool isDev = context.HostingEnvironment.IsDevelopment();
		options.ValidateOnBuild = isDev;
		options.ValidateScopes = isDev;
	});

	ConfigurationManager config = builder.Configuration;
	IConfigurationSection settingsSection = config.GetRequiredSection("Settings");
	IServiceCollection services = builder.Services;

	bool isDev = builder.Environment.IsDevelopment();
	// Add services to the container.
	builder.Services
		.AddResolvedServices(x =>
		{
			x.LoadReferences(x =>
			{
				x.Reference<AD.Api.Startup.Services.IServiceTypeExclusions>()
				 .Reference<IAdApiAttribute>()
				 .Reference<ApiResult>();
			});

			x.Configuration = config;
			x.AllowDuplicateServiceRegistrations = !isDev;
			x.IgnoreMultipleDynamicRegistrations = !isDev;
			x.ThrowOnMissingDynamicRegistrationMethod = isDev;
			x.AddExclusions(e =>
			{
				e.Add(typeof(IExpressionCache<,>))
				 .Add<LambdaComparisonVisitor>()
				 .Add<LambdaExpressionEqualityComparer>()
				 .Add<LambdaExpressionHasherVisitor>();
			});
		})
		.AddEnumDictionaryGeneration(x =>
		{
			x.Register<ActiveDirectorySyntax>()
			 .Register<FilteredRequestType>()
			 .Register<GroupType>()
			 .Register<SamAccountType>(freeze: false)
			 .Register<UserAccountControl>()
			 .Register<WellKnownObjectValue>();
		})
		.AddEnumStringDictionary<ResultCode>(out var resultCodes)
		// Add Authentication/Authorization
		.AddApiAuthenticationAuthorization(config, out Action<WebApplication>? builderCallback, out bool isJwt)
		.AddMemoryCache()
		.Configure<RouteOptions>(options =>
		{
			options.ConstraintMap.Add(SidRouteConstraint.ConstraintName, typeof(SidRouteConstraint));
		})
		.TryAddSingleton(TimeProvider.System);

	PropertyConverter converter = PropertyConverter.AddToServices(builder.Services, config, (conversions) =>
	{
		IConfigurationSection section = settingsSection.GetRequiredSection("Serialization");

		conversions.Add<Guid>(section.GetSection("GuidAttributes"), AttributeSerialization.WriteGuid);
		conversions.Add<DateTimeOffset>(section.GetSection("DateTimeAttributes"), AttributeSerialization.WriteDateTimeOffset);
		conversions.Add("groupType", AttributeSerialization.WriteEnumValue<GroupType>);
		conversions.Add("objectSid", AttributeSerialization.WriteObjectSID);
		conversions.Add("sAMAccountType", AttributeSerialization.WriteEnumValue<SamAccountType>);
		conversions.Add("userAccountControl", (writer, ref readonly context) =>
		{

		});

		if (section.GetValue("WriteSimpleObjectClass", false))
		{
			conversions.Add("objectClass", AttributeSerialization.WriteObjectClassSimple);
		}
	});

	if (OperatingSystem.IsWindows())
	{
		IConfigurationSection section = settingsSection.GetSection("Encryption");
		if (section.Exists() && "Certificate".Equals(section.GetValue<string>("Type"), StringComparison.OrdinalIgnoreCase))
		{
			builder.Services.AddSingleton<IEncryptionService, CertificateEncryptionService>();
		}
		else
		{

			builder.Services.AddSingleton<IEncryptionService, WindowsDpapiEncryptionService>();
		}
	}
	else
	{
		builder.Services.AddSingleton<IEncryptionService, CertificateEncryptionService>();
	}

	builder.AddApiControllers(settingsSection, converter, b =>
	{
		return b.Services
			.AddControllers(options =>
			{
				options.AllowEmptyInputInBodyModelBinding = false;
				options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider(JsonSpanCamelCaseNamingPolicy.SpanPolicy));

				if (isJwt)
				{
					options.ModelValidatorProviders.Add(new ScopeAuthorizationValidator());
				}
			})
			.ConfigureApiBehaviorOptions(options =>
			{
				options.SuppressMapClientErrors = true;
				options.InvalidModelStateResponseFactory = context =>
				{
					return new ApiBadRequestResult(context.ModelState);
				};
			});
	});

	// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
	builder.Services.AddEndpointsApiExplorer()
		.AddSwaggerGen(x =>
		{
			x.SchemaFilter<DomainQueryFilter>();
			x.OperationFilter<DomainQueryFilter>();
			x.OperationFilter<SidStringFilter>();
		})
		.AddSwaggerWithOptions(config.GetSection("SwaggerInfo"));

	// Only show PII in development.
	IdentityModelEventSource.ShowPII = builder.Environment.IsDevelopment();

	// Build the application.
	WebApplication app = builder.Build();
	converter.AddScopeFactory(app.Services.GetRequiredService<IServiceScopeFactory>());

	// Configure the HTTP request pipeline.
	if (app.Environment.IsDevelopment())
	{
		app.UseSwagger();
		app.UseSwaggerUI();
	}

	app.UseExceptionHandler(new ExceptionHandlerOptions()
	{
		AllowStatusCode404Response = false,
		CreateScopeForErrors = false,
		ExceptionHandler = new ErrorHandlingMiddleware(resultCodes).Invoke,
	});

	app.UseHttpsRedirection();

	app.UseRequestLoggerMiddleware()
	   .UseDomainReaderMiddleware();

	builderCallback?.Invoke(app);

	app.UseAuthentication()
	   .UseAuthorization();

	app.MapControllers();

	app.Run();
}
catch (Exception e)
{
	logger.Error(e, "Stopped API because of fatal exception -");
	throw;
}
finally
{
	// Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
	LogManager.Shutdown();
}