using AD.Api;
using AD.Api.Collections;
using AD.Api.Constraints;
using AD.Api.Core.Authentication;
using AD.Api.Core.Authentication.Jwt;
using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Filters;
using AD.Api.Core.Security;
using AD.Api.Core.Security.Encryption;
using AD.Api.Core.Serialization.Json;
using AD.Api.Expressions;
using AD.Api.Extensions.Startup;
using AD.Api.Mapping;
using AD.Api.Middleware;
using AD.Api.Services;
using AD.Api.Services.Enums;
using AD.Api.Settings;
using AD.Api.Startup;
using Microsoft.IdentityModel.Logging;
using NLog;
using NLog.Web;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Reflection;

#region EXPLICIT LOADS

Referencer.LoadAll((in Referencer referer) =>
{
    referer
        .Reference<IConnectionService>()
        .Reference<UnsafeDictionary<int>>();
});

#endregion

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

    IConfiguration config = builder.Configuration;

    Assembly[] assemblies = AssemblyLoader.GetAppAssemblies(AppDomain.CurrentDomain);

    var settings = config
            .GetSection("CustomJwt")
            .Get<CustomJwtSettings>();

    Console.Write(settings);

    // Add services to the container.

    // Add Authentication - Kerberos/Negotiate
    //builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    //    .AddNegotiate(options =>
    //    {
    //        options.Validate();
    //    });

    //builder.Services.AddAuthentication()
    //    //.AddJwtBearer("Auth0", options =>
    //    //{
    //    //    options.Audience = "myCustomJwt";
    //    //    options.Authority = "https://doesnotexist.com";
    //    //})
    //    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAD"));

    //builder.Services.AddAuthorization(options =>
    //{
    //    var policyBuilder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme); // Auth0
    //    policyBuilder = policyBuilder.RequireAuthenticatedUser();
    //    options.DefaultPolicy = policyBuilder.Build();
    //});

    builder.Services
        .AddResolvedServicesFromAssemblies(builder.Configuration, assemblies, exclude =>
        {
            exclude.Add(typeof(IExpressionCache<,>))
                   .Add<LambdaComparisonVisitor>()
                   .Add<LambdaExpressionEqualityComparer>()
                   .Add<LambdaExpressionHasherVisitor>();
        })
        .AddEnumDictionaryGeneration(x =>
        {
            x.Register<ActiveDirectorySyntax>()
             .Register<FilteredRequestType>()
             .Register<GroupType>()
             .Register<SamAccountType>()
             .Register<UserAccountControl>()
             .Register<WellKnownObjectValue>();
        })
        .AddMemoryCache()
        .Configure<RouteOptions>(options =>
        {
            options.ConstraintMap.Add(SidRouteConstraint.ConstraintName, typeof(SidRouteConstraint));
        });

    builder.Services.AddEnumStringDictionary<AuthorizedRole>(out var roles, freeze: true)
                    .AddEnumStringDictionary<ResultCode>(out var resultCodes, freeze: true);

    builder.AddJwtAuthentication(roles);

    PropertyConverter converter = PropertyConverter.AddToServices(builder.Services, config, (conversions) =>
    {
        ReadOnlySpan<byte> timeConvertAttributes = "accountExpires badPasswordTime lastLogon lastLogonTimestamp pwdLastSet whenChanged whenCreated"u8;

        conversions.AddMany(timeConvertAttributes, AttributeSerialization.WriteDateTimeOffset);
        conversions.Add("groupType", AttributeSerialization.WriteEnumValue<GroupType>);
        conversions.Add("objectSid", AttributeSerialization.WriteObjectSID);
        conversions.Add("sAMAccountType", AttributeSerialization.WriteEnumValue<SamAccountType>);
        conversions.Add("userAccountControl", AttributeSerialization.WriteEnumValue<UserAccountControl>);

        if (config
            .GetSection("Settings")
            .GetSection("Serialization")
            .GetValue("WriteSimpleObjectClass", false))
        {
            conversions.Add("objectClass", AttributeSerialization.WriteObjectClassSimple);
        }
    });

    if (OperatingSystem.IsWindows())
    {
        IConfigurationSection section = config.GetSection("Settings").GetSection("Encryption");
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

    builder.Services
        .AddControllers()
        .AddADApiJsonConfiguration(builder, converter);
    //.AddNewtonsoftJson(options =>
    //{
    //    options.AddADApiConfiguration(textSettings);
    //});

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerWithOptions(config.GetSection("SwaggerInfo"));

    IdentityModelEventSource.ShowPII = builder.Environment.IsDevelopment();

    var app = builder.Build();
    converter.AddScopeFactory(app.Services.GetRequiredService<IServiceScopeFactory>());

    app.UseExceptionHandler(new ExceptionHandlerOptions()
    {
        AllowStatusCode404Response = false,
        CreateScopeForErrors = false,
        ExceptionHandler = new ErrorHandlingMiddleware(resultCodes).Invoke,
    });

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseRequestLoggerMiddleware()
       .UseDomainReaderMiddleware();
    //.UseMultipleSchemaAuthenticationMiddleware();
    //.UseImpersonationMiddleware();

    app.UseAuthentication()
       .UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception e)
{
    logger.Error(e, "Stopped API because of fatal exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    LogManager.Shutdown();
}