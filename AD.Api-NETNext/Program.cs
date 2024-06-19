using AD.Api.Collections;
using AD.Api.Core.Ldap;
using AD.Api.Core.Security.Encryption;
using AD.Api.Core.Serialization;
using AD.Api.Expressions;
using AD.Api.Extensions.Startup;
using AD.Api.Mapping;
using AD.Api.Services;
using AD.Api.Services.Enums;
using AD.Api.Startup;
using Microsoft.IdentityModel.Logging;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

#region EXPLICIT LOADS

Referencer.LoadAll((in Referencer referer) =>
{
    referer
        .Reference<IConnectionService>()
        .Reference<UnsafeDictionary<int>>();
});

#endregion

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
});

builder.Host.UseDefaultServiceProvider((context, options) =>
{
    bool isDev = context.HostingEnvironment.IsDevelopment();
    options.ValidateOnBuild = isDev;
    options.ValidateScopes = isDev;
});

Assembly[] assemblies = AssemblyLoader.GetAppAssemblies(AppDomain.CurrentDomain);

builder.Configuration.AddEnvironmentVariables();
if (!builder.Environment.IsProduction())
{
    builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, false);
}
else
{
    builder.Configuration.AddJsonFile("appsettings.json", true, false);
}

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
        x.Register<ActiveDirectorySyntax>(freeze: true)
         .Register<GroupType>(freeze: true)
         .Register<ResultCode>(freeze: true)
         .Register<SamAccountType>(freeze: true)
         .Register<UserAccountControl>(freeze: true)
         .Register<WellKnownObjectValue>(freeze: true);
    });

PropertyConverter converter = PropertyConverter.AddToServices(builder.Services, builder.Configuration, (conversions) =>
{
    ReadOnlySpan<byte> timeConvertAttributes = "accountExpires badPasswordTime lastLogon lastLogonTimestamp pwdLastSet whenChanged whenCreated"u8;

    conversions.AddMany(timeConvertAttributes, AttributeSerialization.WriteDateTimeOffset);
    conversions.Add("groupType", AttributeSerialization.WriteEnumValue<GroupType>);
    conversions.Add("objectSid", AttributeSerialization.WriteObjectSID);
    conversions.Add("sAMAccountType", AttributeSerialization.WriteEnumValue<SamAccountType>);
    conversions.Add("userAccountControl", AttributeSerialization.WriteEnumValue<UserAccountControl>);
});

if (OperatingSystem.IsWindows())
{
    IConfigurationSection section = builder.Configuration.GetSection("Settings").GetSection("Encryption");
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
builder.Services.AddSwaggerWithOptions(builder.Configuration.GetSection("SwaggerInfo"));

IdentityModelEventSource.ShowPII = builder.Environment.IsDevelopment();

var app = builder.Build();
converter.AddScopeFactory(app.Services.GetRequiredService<IServiceScopeFactory>());

//app.UseExceptionHandler(new ExceptionHandlerOptions
//{
//    ExceptionHandler = new ErrorHandlingMiddleware().Invoke
//});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

//app.UseAuthentication();
//app.UseAuthorization();

//app.UseMultipleSchemaAuthenticationMiddleware();

//app.UseImpersonationMiddleware();

app.MapControllers();

app.Run();
