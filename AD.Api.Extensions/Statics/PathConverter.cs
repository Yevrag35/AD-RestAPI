namespace AD.Api.Statics
{
    public static class PathConverter
    {
        static readonly string _startupDir = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull(nameof(path))]
        public static string? ToAbsolutePath(string? path)
        {
            return ToAbsolutePath(path, _startupDir);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="basePath"></param>
        /// <exception cref="ArgumentNullException"/>
        /// <returns></returns>
        [return: NotNullIfNotNull(nameof(path))]
        public static string? ToAbsolutePath(string? path, string basePath)
        {
            ArgumentNullException.ThrowIfNull(basePath);

            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            return Path.Combine(basePath, path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        public static string ToAbsolutePath(ReadOnlySpan<char> path, ReadOnlySpan<char> basePath)
        {
            if (path.IsWhiteSpace())
            {
                return string.Empty;
            }
            else if (Path.IsPathRooted(path))
            {
                return path.ToString();
            }

            return Path.Join(basePath, path);
        }
    }
}

