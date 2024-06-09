namespace AD.Api.Extensions.Startup
{
    /// <summary>
    /// A <see langword="static"/> class that returns the assemblies loaded in the current application domain.
    /// </summary>
    public static class AssemblyLoader
    {
        /// <summary>
        /// Returns the assemblies loaded in the specified application domain.
        /// </summary>
        /// <param name="appDomain">The application domain to query.</param>
        /// <returns>
        /// An array of <see cref="Assembly"/> objects representing the assemblies loaded in the specified 
        /// application domain.
        /// </returns>
        public static Assembly[] GetAppAssemblies(AppDomain appDomain)
        {
            Assembly[] assemblies = appDomain.GetAssemblies();
            SortAssemblies(assemblies);

            return assemblies;
        }

        [Conditional("DEBUG")]
        private static void SortAssemblies(Assembly[] assemblies)
        {
            Array.Sort(assemblies, (x, y) =>
            {
                return StringComparer.OrdinalIgnoreCase.Compare(x.FullName, y.FullName);
            });
        }
    }
}

