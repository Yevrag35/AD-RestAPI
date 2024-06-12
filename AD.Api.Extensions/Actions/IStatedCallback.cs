namespace AD.Api.Actions
{
    public interface IStatedCallback<T>
    {
        T Invoke();
    }
}
