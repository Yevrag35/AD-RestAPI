using AD.Api.Attributes.Services;
using AD.Api.Core.Ldap.Services.Connections;
using AD.Api.Core.Schema;
using AD.Api.Statics;
using AD.Api.Strings.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Buffers;
using System.Collections.Frozen;
using System.Runtime.Versioning;
using System.Text;

namespace AD.Api.Core.Ldap.Services.Schemas
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
            IConnectionService conSvc = provider.GetRequiredService<IConnectionService>();
            string[] classArray = GetClassNames(out int count);
            string[] classNames = classArray.AsSpan(0, count).ToArray();
            ArrayPool<string>.Shared.Return(classArray);
            return new SchemaService(classNames);
        }

        private static string[] GetClassNames(out int count)
        {
            //["user", "computer", "organizationalUnit", "configuration", "RootDSE", "container", "group"];
            ReadOnlySpan<byte> bytes = "user computer organizationalUnit configuration container group"u8;
            Encoding e = Encoding.UTF8;
            Span<char> chars = stackalloc char[e.GetCharCount(bytes)];
            int written = e.GetChars(bytes, chars);
            chars = chars.Slice(0, written);
            count = chars.Count(SPACE) + 1;

            string[] array = ArrayPool<string>.Shared.Rent(count);
            int index = 0;
            foreach (ReadOnlySpan<char> section in chars.SpanSplit(in SPACE))
            {
                array[index++] = section.Trim().ToString();
            }

            count = index;
            return array;
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

