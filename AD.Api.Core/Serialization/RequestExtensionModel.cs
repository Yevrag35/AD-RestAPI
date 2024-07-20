using AD.Api.Serialization.Json;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Serialization
{
    [PrivateExtensionDataClass(typeof(RequestExtensionModel))]
    public abstract class RequestExtensionModel : IJsonOnDeserialized, IValidatableObject
    {
        [PrivateExtensionData]
        private readonly IDictionary<string, object?> _extensionData;

        protected RequestExtensionModel(IEnumerable<string> extraJsonPropertyNames)
        {
            _extensionData = new ExclusionaryJsonDictionary(extraJsonPropertyNames);
        }

        protected IEnumerable<string> GetFaultingProperty<T>(Expression<Func<T, object?>> memberExpression, string additionalKeyName)
        {
            if (_extensionData.ContainsKey(additionalKeyName))
            {
                return [additionalKeyName];
            }
            else if (TryGetMemberName(memberExpression, out string? memberName))
            {
                return [memberName];
            }
            else
            {
                return [];
            }
        }
        protected IEnumerable<string> GetFaultingProperty<T>(Expression<Func<T, object?>> memberExpression, IEnumerable<string> additionalKeyNames)
        {
            foreach (string key in additionalKeyNames)
            {
                if (_extensionData.ContainsKey(key))
                {
                    return [key];
                }
            }

            return TryGetMemberName(memberExpression, out string? memberName) ? [memberName] : [];
        }
        public void OnDeserialized()
        {
            this.OnDeserialized((IReadOnlyDictionary<string, object?>)_extensionData);
        }

        /// <summary>
        /// When overridden in a derived class, provides a way to access the extension data after deserialization.
        /// Derived classes can use this method to set properties based on additional property key names.
        /// </summary>
        /// <param name="extensionData">
        /// The 
        /// </param>
        protected abstract void OnDeserialized(IReadOnlyDictionary<string, object?> extensionData);
        private static bool TryGetMemberName(LambdaExpression expression, [NotNullWhen(true)] out string? memberName)
        {
            if (expression.Body is MemberExpression memEx)
            {
                memberName = memEx.Member.Name;
                return true;
            }
            else if (expression.Body is UnaryExpression unEx && unEx.Operand is MemberExpression unMemEx)
            {
                memberName = unMemEx.Member.Name;
                return true;
            }
            else
            {
                memberName = null;
                return false;
            }
        }
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return [];
        }
    }
}
