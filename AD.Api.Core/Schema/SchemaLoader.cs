using AD.Api.Core.Ldap;
using System.Runtime.Versioning;

namespace AD.Api.Core.Schema
{
    [SupportedOSPlatform("WINDOWS")]
    public static class SchemaLoader
    {
        public static async Task<SchemaClassPropertyDictionary> LoadSchemaAsync(ConnectionContext context, SemaphoreSlim semaphore, string[] classNames, CancellationToken token = default)
        {
            using SchemaDictionaryBuilder builder = new(context);
            try
            {
                await semaphore.WaitAsync(token).ConfigureAwait(false);

                List<Task> tasks = new(classNames.Length);

                foreach (string className in classNames)
                {
                    tasks.Add(builder.ReadFromAsync(className, token));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }

            return builder.Build();
        }
    }
}

