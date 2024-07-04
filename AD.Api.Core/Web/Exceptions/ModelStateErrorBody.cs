using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using System.Text.Json.Serialization;
using RCode = System.DirectoryServices.Protocols.ResultCode;

namespace AD.Api.Core.Web;

public sealed class ModelStateErrorBody : ErrorBody
{
    [JsonPropertyOrder(int.MaxValue)]
    public Dictionary<string, StringValues> Errors { get; }

    [SetsRequiredMembers]
    public ModelStateErrorBody(ModelStateDictionary modelState)
    {
        Debug.Assert(!modelState.IsValid);
        this.Errors = new(modelState.Count);
        this.Message = "One or more validation errors occurred.";
        this.Result = RCode.ConstraintViolation;
        this.ResultCode = (int)this.Result;

        foreach (var kvp in modelState)
        {
            _ = this.Errors.TryAdd(kvp.Key, kvp.Value.Errors
                    .Select(x => x.ErrorMessage)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray());
        }
    }
}

