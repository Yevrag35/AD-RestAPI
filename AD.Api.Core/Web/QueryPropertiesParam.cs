using AD.Api.Components;
using AD.Api.Core.Schema;
using AD.Api.Core.Web.Extensions;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Primitives;
using System.Collections.Frozen;

namespace AD.Api.Core.Web;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class FromQueryPropertiesAttribute : ModelBinderAttribute, IFromQueryMetadata
{
	private static readonly Type _binderType = typeof(QueryPropertiesBinding);

	[Obsolete("The logic for this property is not yet implemented.", true)]
	public string? RestrictedToClass { get; init; }

	public FromQueryPropertiesAttribute()
		: base(_binderType)
	{
	}
}

public sealed class QueryPropertiesBinding : IModelBinder
{
	private const string DEFAULT = "default";
	private static readonly Type _stringArrayType = typeof(string[]);
	private delegate bool ClassOverlapDelegate(in SchemaProperty property, string name, ISet<string> set);
	private static readonly ValidationStateEntry _suppress = new()
	{
		SuppressValidation = true,
	};

	public Task BindModelAsync(ModelBindingContext bindingContext)
	{
		if (!_stringArrayType.Equals(bindingContext.ModelMetadata.UnderlyingOrModelType))
		{
			return Task.CompletedTask;
		}

		var oneOf = GetValue(bindingContext);
		if (oneOf.TryGetT2(out var result, out string? propertiesString))
		{
			bindingContext.Result = result;
			return Task.CompletedTask;
		}

		Span<char> splitBy = [',', '+', ' '];

		string domain = GetQueryValue(bindingContext.HttpContext.Request.Query, DomainQuery.DomainModelName, string.Empty);

		var schemaSvc = bindingContext.HttpContext.RequestServices.GetRequiredService<ISchemaService>();

		bindingContext.Result = schemaSvc.IsFunctional
			? GetSchemaValidatedResult(bindingContext, propertiesString, splitBy, schemaSvc, domain)
			: GetNonSchemaValidatedResult(bindingContext, propertiesString, splitBy);

		return Task.CompletedTask;
	}

	[Obsolete("The logic for this method is not yet implemented.", true)]
	private static void AddUnknownClassPropertyError(ModelBindingContext context, string propertyName, string className, SchemaClassPropertyDictionary schemaDict)
	{
		string message = string.Format(Errors.Validation_SchemaProperty_UnknownClassProperty, propertyName, className, schemaDict.DomainName);
		_ = context.ModelState.TryAddModelError(context.ModelName, message);
	}
	private static void AddUnknownPropertyError(ModelBindingContext context, string propertyName, SchemaClassPropertyDictionary schemaDict)
	{
		string message = string.Format(Errors.Validation_SchemaProperty_UnknownProperty, propertyName, schemaDict.DomainName);
		_ = context.ModelState.TryAddModelError(context.ModelName, message);
	}
	[return: NotNullIfNotNull(nameof(defaultValue))]
	private static string? GetQueryValue(IQueryCollection query, [ConstantExpected] string key, string? defaultValue)
	{
		return query.TryGetValue(key, out StringValues stringValues) && stringValues.Count > 0
			? stringValues[0] ?? defaultValue
			: defaultValue;
	}
	private static ModelBindingResult GetNonSchemaValidatedResult(ModelBindingContext context, string propertyString, Span<char> splitBy)
	{
		string[] properties = ArrayPool<string>.Shared.Rent(propertyString.Length);
		int count = 0;

		foreach (Range section in propertyString.AsSpan().SplitAny(splitBy))
		{
			ReadOnlySpan<char> trimmed = propertyString[section].Trim();
			string propertyName = new(trimmed);
			properties[count++] = propertyName;
		}

		string[] result = new string[count];
		Array.Copy(properties, result, count);
		ArrayPool<string>.Shared.Return(properties);

		return ModelBindingResult.Success(result);
	}

	[Obsolete("The logic for this method is not yet implemented.", true)]
	private static string? GetRestrictedClassName(ModelMetadata metadata)
	{
		if (metadata is not DefaultModelMetadata defMeta)
		{
			return null;
		}

		return defMeta.Attributes.Attributes.OfType<FromQueryPropertiesAttribute>().FirstOrDefault()?.RestrictedToClass;
	}
	private static ModelBindingResult GetSchemaValidatedResult(ModelBindingContext context, string propertyString, Span<char> splitBy, ISchemaService schemaSvc, string domain)
	{
		if (!schemaSvc.ContainsDomain(domain))
		{
			return ModelBindingResult.Failed();
		}

		//string? restrictedClassName = GetRestrictedClassName(context.ModelMetadata);

		ref readonly SchemaClassPropertyDictionary schema = ref schemaSvc[domain];
		ReadOnlySpan<char> chars = propertyString;

		//OverlapValidator validator;
		//if (!string.IsNullOrEmpty(restrictedClassName))
		//{
		//    var set = context.HttpContext.RequestServices.GetRequiredService<IPooledItem<HashSet<string>>>().Value;
		//    validator = OverlapValidator.Create(restrictedClassName, set);
		//}
		//else
		//{
		//    validator = OverlapValidator.Ignore;
		//}

		string[] properties = ArrayPool<string>.Shared.Rent(chars.Length);
		int count = 0;
		foreach (Range section in chars.SplitAny(splitBy))
		{
			ReadOnlySpan<char> trimmed = chars[section].Trim();
			if (trimmed.Equals(DEFAULT, StringComparison.OrdinalIgnoreCase))
			{
				properties[count++] = DEFAULT;
				continue;
			}

			string propertyName = trimmed.ToString();
			//if (!schema.TryGetValue(propertyName, out SchemaProperty schProp) || !validator.Overlaps(in schProp))
			if (!schema.ContainsKey(propertyName))
			{
				AddUnknownPropertyError(context, propertyName, schema);
				continue;
			}

			properties[count++] = propertyName;
		}

		ModelBindingResult result = context.ModelState.IsValid
			? ReadModelIntoSuccessResult(properties, count)
			: ReturnErroredSuccess(context);

		ArrayPool<string>.Shared.Return(properties);
		return result;
	}
	private static Either<string, ModelBindingResult> GetValue(ModelBindingContext context)
	{
		string? value = context.GetFirstValue();
		if (value is null)
		{
			bool isRequired = context.IsValueRequired();
			return context.ModelMetadata.IsNullableValueType || !isRequired
				? ModelBindingResult.Success(null)
				: ModelBindingResult.Failed();
		}
		else if (value.AsSpan().IsWhiteSpace())
		{
			return ModelBindingResult.Success(string.Empty);
		}
		else
		{
			return value;
		}
	}
	private static ModelBindingResult ReadModelIntoSuccessResult(string[] properties, int count)
	{
		string[] finalArray = new string[count];
		Array.Copy(properties, finalArray, count);
		return ModelBindingResult.Success(finalArray);
	}
	private static ModelBindingResult ReturnErroredSuccess(ModelBindingContext context)
	{
		string[] empty = [];
		context.ValidationState[empty] = _suppress;
		return ModelBindingResult.Success(empty);
	}

	[Obsolete("The logic for this struct is not yet functionally accurate.", true)]
	private readonly struct OverlapValidator
	{
		private readonly ClassOverlapDelegate _func;
		private readonly bool _hasSet;
		private readonly string? _restrictedClassName;
		private readonly ISet<string>? _set;

		[MemberNotNullWhen(true, nameof(_set))]
		private readonly bool HasSet => _hasSet;
		internal readonly string RestrictedClassName => _restrictedClassName ?? string.Empty;

		private OverlapValidator(ClassOverlapDelegate ignore)
		{
			_func = ignore;
			_restrictedClassName = null;
			_set = FrozenSet<string>.Empty;
			_hasSet = true;
		}
		private OverlapValidator(ClassOverlapDelegate func, string? restrictedClassName, HashSet<string> set)
		{
			_func = func;
			_restrictedClassName = restrictedClassName;
			_set = set;
			_hasSet = true;
		}

		internal readonly bool Overlaps(in SchemaProperty property)
		{
			return _func(in property, this.RestrictedClassName, this.HasSet ? _set : FrozenSet<string>.Empty);
		}

		internal static OverlapValidator Create(string restrictedClassName, HashSet<string> set)
		{
			return new OverlapValidator(DoesClassOverlap, restrictedClassName, set);
		}
		internal static readonly OverlapValidator Ignore = new(IgnoreClassOverlap);

		private static bool DoesClassOverlap(in SchemaProperty property, string name, ISet<string> set)
		{
			return property.ClassOverlaps(name, set);
		}
		private static bool IgnoreClassOverlap(in SchemaProperty property, string name, ISet<string> set)
		{
			return true;
		}
	}
}
