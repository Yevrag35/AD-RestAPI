using AD.Api.Binding.Attributes;
using AD.Api.Core.Security;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics;
using System.Reflection;

namespace AD.Api.Swagger.Filters
{
    public sealed class SidStringFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            foreach (var parameter in context.ApiDescription.ParameterDescriptions)
            {
                var sidAtt = parameter.ParameterInfo().GetCustomAttribute<FromRouteSidAttribute>();
                if (sidAtt is null)
                {
                    continue;
                }

                if (!context.SchemaRepository.Schemas.TryGetValue(nameof(SidString), out var schema))
                {
                    Debug.Fail("Could not find schema for SidString.");
                    return;
                }

                for (int i = operation.Parameters.Count - 1; i >= 0; i--)
                {
                    if (operation.Parameters[i].Name == parameter.Name)
                    {
                        operation.Parameters.RemoveAt(i);
                    }
                }

                operation.Parameters.Insert(0, new OpenApiParameter
                {
                    AllowEmptyValue = false,
                    In = ParameterLocation.Path,
                    Required = true,
                    Name = parameter.Name,
                    Schema = schema,
                });

                return;
            }
        }
    }
}