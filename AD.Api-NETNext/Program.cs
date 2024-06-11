using AD.Api.Collections;
using AD.Api.Core.Ldap.Enums;
using AD.Api.Core.Ldap.Services.Connections;
using AD.Api.Core.Security.Encryption;
using AD.Api.Extensions.Collections;
using AD.Api.Extensions.Startup;
using AD.Api.Middleware;
using AD.Api.Services;
using AD.Api.Services.Enums;
using AD.Api.Startup;
using Microsoft.IdentityModel.Logging;
using System.Reflection;
using System.Text.Json;

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
        x.Register<GroupType>(freeze: true)
         .Register<UserAccountControl>(freeze: true)
         .Register<WellKnownObjectValue>(freeze: true);
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

//builder.Services.AddDefaultSchemaAttributes(builder.Configuration.GetSection("Attributes"));
//builder.Services.AddEncryptionOptions(builder.Configuration.GetSection("Settings").GetSection("Encryption"));
//builder.Services.AddOperationRestrictions(builder.Configuration.GetSection("Settings").GetSection("Restrictions"));
//builder.Services.AddSearchDomains(builder.Configuration.GetSection("Domains"));
//builder.Services.AddSearchDefaultSettings(builder.Configuration.GetSection("Settings").GetSection("SearchDefaults"));
//builder.Services.AddTextSettingOptions(builder.Configuration, out ITextSettings textSettings);

//builder.Services.AddADApiServices();

//builder.Services
//    .AddAutoMapper(assemblies);
//.AddLdapEnumTypes(assemblies);

//builder
//    .ConfigureJson(x => x.Services.AddControllers(), (config, env, options) =>
//    {
//        bool isDev = env.IsDevelopment();

//        IConfigurationSection section = config
//            .GetRequiredSection("Settings")
//            .GetRequiredSection("Serialization");

//        var settings = section.Get<SerializationSettings>() ?? throw new JsonException("Unable to deserialize settings.");

//        settings.SetJsonOptions(options.JsonSerializerOptions);

//        options.AllowInputFormatterExceptionMessages = isDev;
//        options.JsonSerializerOptions.WriteIndented = isDev;
//        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

//        options.JsonSerializerOptions.Converters.AddRange(
//            new LdapFilterStringConverter(),
//            new FilterJsonConverter()
//        );
//    });

builder.Services.AddControllers();
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
