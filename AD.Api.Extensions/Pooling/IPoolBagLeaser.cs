namespace AD.Api.Pooling
{
    public interface IPoolBagLeaser<T> : IPoolBag<T> where T : class
    {
        IPooledItem<T> GetPooledItem();
    }
}

