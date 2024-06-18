namespace AD.Api.Core.Settings
{
    public abstract class PathedSettings
    {
        public abstract void ResolvePaths(ReadOnlySpan<char> basePath);
    }
}

