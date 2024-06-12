namespace AD.Api.Pooling
{
    public interface IPoolLeaseReturner<T> where T : class
    {
        void Return(Guid itemId, T? item);
    }
}
