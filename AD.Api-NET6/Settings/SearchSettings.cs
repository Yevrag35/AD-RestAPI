namespace AD.Api.Settings
{
    public interface ISearchSettings
    {
        int Size { get; }
        string[]? Properties { get; }
    }
    public interface IComputerSettings : ISearchSettings
    {
    }
    public interface IGenericSettings : ISearchSettings
    {
    }
    public interface IUserSettings : ISearchSettings
    {
    }

    public class SearchDefaults
    {
        public SearchSettings Computer { get; set; } = new SearchSettings();
        public SearchSettings Generic { get; set; } = new SearchSettings();
        public SearchSettings User { get; set; } = new SearchSettings();
    }

    public class SearchSettings : IComputerSettings, IUserSettings, IGenericSettings
    {
        public int Size { get; set; }
        public string[]? Properties { get; set; }
    }
}
