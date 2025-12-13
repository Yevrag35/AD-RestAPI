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
		this.Message = "One or more validation errors occurred in the request body.";
		this.Result = RCode.ConstraintViolation;
		this.ResultCode = (int)this.Result;

		foreach (var kvp in modelState)
		{
			StringValues messages = ProjectErrorMessagesOrEmpty(kvp.Value.Errors);
			if (messages.Count > 0)
			{
				_ = this.Errors.TryAdd(kvp.Key, messages);
			}
		}
	}

	private static IEnumerable<string> EnumerateMessages(ModelErrorCollection collection)
	{
		foreach (ModelError error in collection)
		{
			string? exMsg = error.Exception?.Message;
			string msg = error.ErrorMessage;
			yield return msg;

			if (!string.IsNullOrEmpty(exMsg) && !msg.Equals(exMsg, StringComparison.OrdinalIgnoreCase))
			{
				yield return exMsg;
			}
		}
	}
	private static StringValues ProjectErrorMessagesOrEmpty(ModelErrorCollection collection)
	{
		if (collection.Count == 0)
		{
			return StringValues.Empty;
		}

		return new StringValues(EnumerateMessages(collection).ToArray());
	}
}

