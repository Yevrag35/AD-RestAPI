using AD.Api.Binding.Attributes;
using AD.Api.Core;
using AD.Api.Core.Ldap;
using AD.Api.Core.Security;
using AD.Api.Reflection;
using AD.Api.Validation;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AD.Api.Swagger.Filters;

public sealed class DomainQueryFilter : IOperationFilter, ISchemaFilter
{
	//public Dictionary<string, HashSet<string>> Attributes { get; }

	//public LdapWebFilter(Dictionary<string, HashSet<string>> setDict)
	public DomainQueryFilter()
	{
		//this.Attributes = setDict;
	}

	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		if (!context.SchemaRepository.Schemas.TryGetValue(nameof(DomainQuery), out var schema)
			||
			schema.Properties.Count <= 0)
		{
			return;
		}

		foreach (var parameter in context.ApiDescription.ParameterDescriptions)
		{
			var domainAtt = parameter.ParameterInfo().GetCustomAttribute<DomainAttribute>();
			if (domainAtt is null)
			{
				continue;
			}

			for (int i = operation.Parameters.Count - 1; i >= 0; i--)
			{
				if (operation.Parameters[i].Name == parameter.Name)
				{
					operation.Parameters.RemoveAt(i);
				}
			}

			foreach (var kvp in schema.Properties)
			{
				operation.Parameters.Add(new OpenApiParameter
				{
					AllowEmptyValue = true,
					In = ParameterLocation.Query,
					Schema = kvp.Value,
					Name = kvp.Key,
				});
			}

			break;
		}
	}

	//private static bool TryLabelDomainQueryProperty(ApiParameterDescription parameter, OpenApiOperation operation, OperationFilterContext context)
	//{

	//}

	public void Apply(OpenApiSchema schema, SchemaFilterContext context)
	{
		if (typeof(DomainQuery).Equals(context.Type))
		{
			schema.Title = nameof(DomainQuery);
			schema.Properties.Clear();
			schema.Properties.Add(DomainQuery.DomainModelName, new OpenApiSchema
			{
				Type = "string",
				Nullable = true,
				Default = new OpenApiNull(),
				Description = "The fully qualifed domain or NetBIOS name of the domain to query. Leave empty to query the default domain.",
				Example = new OpenApiString("contoso.com"),
			});

			OpenApiSchema dcSchema = new()
			{
				Type = "string",
				Nullable = true,
				Default = new OpenApiNull(),
				Description = "The domain controller to query. Leave empty or null to query the default domain controller. Either 'domainController' or 'dc' can be used.",
				Example = new OpenApiString("dc1"),
			};

			schema.Properties.Add(DomainQuery.DomainControllerModelName, dcSchema);
			schema.Properties.Add(DomainQuery.DomainControllerFullModelName, dcSchema);
			return;
		}

		if (typeof(SidString).Equals(context.Type))
		{
			//schema.Nullable = false;
			schema.Title = "ObjectSID";
			schema.Default = null;
			schema.Description = "The Security Identifier (SID) of the object.";
			schema.MinLength = SidString.MinSidStringLength;
			schema.MaxLength = SidString.MaxSidStringLength;
			schema.Example = new OpenApiString("S-1-5-21-000000000-000000000-000000000-500");
			schema.Type = JsonSchemaType.String;
			schema.Items = null;
			schema.AdditionalProperties = null;
			schema.Properties?.Clear();
			schema.Format = "objectSid";
		}

		if (typeof(RelativeName).Equals(context.Type.TryGetNullable(out Type? rNullable) ? rNullable : context.Type))
		{
			schema.Title = nameof(RelativeName);
			schema.Type = JsonSchemaType.String;
			schema.Items = null;
			schema.AdditionalProperties = null;
			schema.MinLength = 2;
			bool isNullable = rNullable is not null
						   && context.MemberInfo.GetCustomAttribute<RequiredAfterDeserializationAttribute>() is null;

			//schema.Nullable = isNullable;
			schema.Default = isNullable ? new OpenApiNull() : new OpenApiString(string.Empty);
			schema.Description = "A relative distinguishedName (RDN) for the given object. If the attribute prefix is missing from the value, then 'CN=' will be prepended.";
			schema.Example = new OpenApiString("CN=John Doe");
			schema.Format = "relativeDistinguishedName";
			schema.Properties?.Clear();
		}

		if (typeof(DistinguishedName).Equals(context.Type.TryGetNullable(out Type? underlying) ? underlying : context.Type))
		{
			schema.Title = nameof(DistinguishedName);
			schema.Type = JsonSchemaType.String;
			schema.Items = null;
			schema.AdditionalProperties = null;
			schema.AdditionalPropertiesAllowed = false;
			schema.Format = "distinguishedName";
			schema.Description = "The LDAP distinguished name of the object.";
			schema.Properties?.Clear();
			schema.Example = new OpenApiString("CN=John Doe,OU=Users,DC=contoso,DC=com");
			//schema.Nullable = underlying is not null
			//			   && context.MemberInfo?.GetCustomAttribute<RequiredAfterDeserializationAttribute>() is null;
		}
	}
}