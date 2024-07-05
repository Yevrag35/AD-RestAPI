using AD.Api.Attributes.Services;
using AD.Api.Core.Ldap;
using AD.Api.Statics;
using AD.Api.Strings.Extensions;
using System.Buffers;
using System.Collections.Frozen;
using System.Runtime.Versioning;
using System.Text;

namespace AD.Api.Core.Schema
{
    public interface ISchemaService
    {
        ref readonly SchemaClassPropertyDictionary this[string key] { get; }

        int Count { get; }
        bool IsFunctional { get; }
    }

    [DynamicDependencyRegistration]
    [DebuggerDisplay("Count = {Count}")]
    public class SchemaService : ISchemaService
    {
        private FrozenDictionary<string, SchemaClassPropertyDictionary> _library;

        internal string[] ClassNames { get; }
        public int Count => _library.Count;
        public virtual bool IsFunctional => true;

        public ref readonly SchemaClassPropertyDictionary this[string key] => ref _library[key ?? string.Empty];

        protected SchemaService()
        {
            _library = FrozenDictionary<string, SchemaClassPropertyDictionary>.Empty;
            this.ClassNames = [];
        }
        internal SchemaService(string[] classNames)
        {
            _library = FrozenDictionary<string, SchemaClassPropertyDictionary>.Empty;
            this.ClassNames = classNames;
        }

        internal virtual void AddSchemaDictionary(IDictionary<string, SchemaClassPropertyDictionary> dictionary)
        {
            _library = dictionary.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        }

        [DynamicDependencyRegistrationMethod]
        private static void AddToServices(IServiceCollection services)
        {
            if (!OperatingSystem.IsWindows())
            {
                services.AddSingleton<SchemaService, NoSchema>();
            }
            else
            {
                services.AddSingleton(CreateSchemaService);
            }

            services.AddSingleton<ISchemaService>(provider => provider.GetRequiredService<SchemaService>());
        }

        private static readonly char SPACE = CharConstants.SPACE;
        [SupportedOSPlatform("WINDOWS")]
        private static SchemaService CreateSchemaService(IServiceProvider provider)
        {
            return new SchemaService([
                "user",
                "computer",
                "organizationalUnit",
                "configuration",
                "container",
                "contact",
                "group"
            ]);
        }

        private sealed class NoSchema : SchemaService
        {
            public override bool IsFunctional => false;

            internal NoSchema()
                : base()
            {
            }

            internal override void AddSchemaDictionary(IDictionary<string, SchemaClassPropertyDictionary> dictionary)
            {
                return;
            }
        }
    }
}

