using System.Linq.Expressions;

namespace AD.Api.Validation
{
    public interface IValidatableProperty<T>
    {
        Expression<Func<object, T?>>? GetValidatableProperty();
    }
}

