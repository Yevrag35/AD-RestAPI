using AD.Api.Serialization;
using System.ComponentModel.DataAnnotations;

namespace AD.Api.Validation;

public static class ValidationContextExtensions
{
	public static string[] GetMemberNames(this ValidationContext context)
	{
		string? memberName = context.MemberName;
		if (context.ObjectInstance is IExtensionPropertyModel extensionModel)
		{
			memberName = extensionModel.GetFaultingProperty(memberName);
		}

		return [memberName ?? context.DisplayName];
	}
	public static string[] GetMemberNames(this ValidationContext context, string defaultNameIfNull)
	{
		return [context.MemberName ?? defaultNameIfNull];
	}
}