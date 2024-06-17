using AD.Api.Collections;
using AD.Api.Core.Ldap.Enums;
using AD.Api.Core.Ldap.Services.Connections;
using AD.Api.Core.Security.Encryption;
using AD.Api.Core.Serialization;
using AD.Api.Extensions.Collections;
using AD.Api.Extensions.Startup;
using AD.Api.Mapping;
using AD.Api.Middleware;
using AD.Api.Services;
using AD.Api.Services.Enums;
using AD.Api.Startup;
using Microsoft.IdentityModel.Logging;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

#region EXPLICIT LOADS

Referencer.LoadAll((in Referencer referer) =>
{
    referer
        //.Reference<Add>()
        //.Reference<And>()
        //.Reference<LdapPropertyConverter>()
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

builder.Configuration
    .AddEnvironmentVariables()
    .AddJsonFile("defaultAttributes.json", true, false);
if (builder.Environment.IsDevelopment())
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

Assembly[] assemblies = AssemblyLoader.GetAppAssemblies(AppDomain.CurrentDomain);
builder.Services
    .AddResolvedServicesFromAssemblies(builder.Configuration, assemblies)
    .AddEnumDictionaryGeneration(x =>
    {
        x.Register<ActiveDirectorySyntax>(freeze: true)
         .Register<GroupType>(freeze: true)
         .Register<ResultCode>(freeze: true)
         .Register<UserAccountControl>(freeze: true)
         .Register<WellKnownObjectValue>(freeze: true);
    });

var converter = PropertyConverter.AddToServices(builder.Services, (conversions) =>
{
    ReadOnlySpan<byte> timeConvertAttributes = "accountExpires badPasswordTime lastLogon lastLogonTimestamp pwdLastSet whenChanged whenCreated"u8;

    conversions.AddMany(timeConvertAttributes, AttributeSerialization.WriteDateTimeOffset);
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
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
        options.AllowInputFormatterExceptionMessages = builder.Environment.IsDevelopment();
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new ResultEntryConverter(converter));
        options.JsonSerializerOptions.Converters.Add(new ResultEntryCollectionConverter(converter));
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });
//.AddNewtonsoftJson(options =>
//{
//    options.AddADApiConfiguration(textSettings);
//});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithOptions(builder.Configuration.GetSection("SwaggerInfo"));

IdentityModelEventSource.ShowPII = builder.Environment.IsDevelopment();

var app = builder.Build();

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
