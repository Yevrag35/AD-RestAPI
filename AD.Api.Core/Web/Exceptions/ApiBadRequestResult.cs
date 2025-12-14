using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AD.Api.Core.Web;

public sealed class ApiBadRequestResult : ErrorObjectResult
{
	protected override int StaticStatusCode => StatusCodes.Status400BadRequest;

	public ApiBadRequestResult(string message, ResultCode resultCode)
		: base(message, in resultCode)
	{
		this.StatusCode = this.StaticStatusCode;
	}
	public ApiBadRequestResult(ModelStateDictionary failedModelState)
		: base(new ModelStateErrorBody(failedModelState))
	{
		this.StatusCode = this.StaticStatusCode;
	}
}

