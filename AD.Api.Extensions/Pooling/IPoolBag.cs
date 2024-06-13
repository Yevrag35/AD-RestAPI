namespace AD.Api.Pooling
{
    public interface IPoolBag<T> : IPoolReturner<T> where T : class
    {
        T Get();
    }
}

