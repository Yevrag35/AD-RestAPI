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
    public interface IGroupSettings : ISearchSettings
    {
        /// <summary>
        /// Indicates that the default Group query should always include the "Member" property 
        /// regardless of the default properties set or explicit properties sent in the URL query path.
        /// </summary>
        bool IncludeMembers { get; }
    }
    public interface IUserSettings : ISearchSettings
    {
    }

    public sealed class SearchDefaults
    {
        public SearchSettings Computer { get; set; } = new SearchSettings();
        public SearchSettings Generic { get; set; } = new SearchSettings();
        public SearchSettings Group { get; set; } = new SearchSettings();
        public SearchSettings User { get; set; } = new SearchSettings();
    }

    public sealed class SearchSettings : IComputerSettings, IUserSettings, IGenericSettings, IGroupSettings
    {
        public bool? IncludeMembers { get; set; }
        bool IGroupSettings.IncludeMembers => this.IncludeMembers.GetValueOrDefault();
        public int Size { get; set; }
        public string[]? Properties { get; set; }
    }
}
