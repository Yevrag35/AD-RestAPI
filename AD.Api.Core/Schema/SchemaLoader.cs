using AD.Api.Core.Ldap;
using System.Collections.Immutable;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.Versioning;

namespace AD.Api.Core.Schema;

[SupportedOSPlatform("WINDOWS")]
public static class SchemaLoader
{
	private const string TOP = "top";

	public static async Task<SchemaClassPropertyDictionary> LoadSchemaAsync(ConnectionContext context, SemaphoreSlim semaphore, string[] classNames, CancellationToken token = default)
	{

		using SchemaDictionaryBuilder builder = new(context);
		try
		{
			await semaphore.WaitAsync(token).ConfigureAwait(false);

			List<Task> tasks = new(classNames.Length);

			foreach (string className in classNames)
			{
				tasks.Add(builder.ReadFromAsync(className, GetClassHeirarchy, token));
			}

			await Task.WhenAll(tasks).ConfigureAwait(false);
		}
		finally
		{
			semaphore.Release();
		}

		return builder.Build();
	}

	private static ImmutableArray<string> GetClassHeirarchy(ActiveDirectorySchemaClass schemaClass)
	{
		return [];
	}
	[Obsolete("The logic for this method is not completely accurate (functionally).", true)]
	private static ImmutableArray<string> GetClassHeirarchy_Testing(ActiveDirectorySchemaClass schemaClass)
	{
		List<string> heirarchy = new(2)
		{
			schemaClass.Name,
		};

		ActiveDirectorySchemaClass? parent = schemaClass.SubClassOf;
		while (parent is not null)
		{
			heirarchy.Add(parent.Name);
			if (TOP.Equals(parent.Name, StringComparison.OrdinalIgnoreCase))
			{
				heirarchy.Add(TOP);
				break;
			}

			parent = parent.SubClassOf;
		}

		return [.. heirarchy];
	}
}