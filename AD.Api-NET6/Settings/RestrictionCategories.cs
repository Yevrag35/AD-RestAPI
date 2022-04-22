using AD.Api.Ldap.Operations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace AD.Api.Settings
{
    public class RestrictionCategories
    {
        internal static Lazy<Restriction> Default = new(InitializeDefault);

        public Restriction Delete { get; set; }
        public Restriction Edit { get; set; }
        public Restriction Move { get; set; }
        public Restriction Rename { get; set; }

        internal RestrictionCategories()
        {
        }

        private static Restriction InitializeDefault()
        {
            return new Restriction(Array.Empty<string>());
        }

        public bool TryGetRestriction(OperationType type, [NotNullWhen(true)] out Restriction? restriction)
        {
            restriction = null;
            switch (type)
            {
                case OperationType.Delete:
                    restriction = this.Delete;
                    break;

                case OperationType.Set:
                case OperationType.Add:
                case OperationType.Clear:
                case OperationType.Remove:
                case OperationType.Replace:
                    restriction = this.Edit;
                    break;

                case OperationType.Move:
                    restriction = this.Move;
                    break;

                case OperationType.Rename:
                    restriction = this.Rename;
                    break;

                default:
                    return false;
            }

            return restriction is not null;
        }
    }

    public class Restriction
    {
        public IReadOnlySet<string> ObjectClasses { get; }

        public Restriction(IEnumerable<string> classes)
        {
            this.ObjectClasses = new HashSet<string>(classes, StringComparer.CurrentCultureIgnoreCase);
        }
    }

    public static class RestrictionDependencyInjection
    {
        public static IServiceCollection AddOperationRestrictions(this IServiceCollection services, IConfigurationSection section)
        {
            RestrictionCategories cats = new();
            HashSet<string> used = new(4, StringComparer.CurrentCultureIgnoreCase);
            Type restType = typeof(Restriction);

            foreach (IConfigurationSection child in section.GetChildren())
            {
                string key = child.Key.ToUpper();
                string[] values = child.GetSection("ObjectClasses").Get<string[]>();

                Restriction restriction = new(values);

                Action<Restriction>? action = null;

                switch (key)
                {
                    case "DELETE":
                        action = rest => cats.Delete = rest;
                        goto default;

                    case "MOVE":
                        action = rest => cats.Move = rest;
                        goto default;

                    case "RENAME":
                        action = rest => cats.Rename = rest;
                        goto default;

                    case "EDIT":
                        action = rest => cats.Edit = rest;
                        goto default;

                    default:
                        _ = used.Add(key);
                        break;
                }

                if (action is null)
                    continue;

                action(restriction);
            }

            IEnumerable<PropertyInfo> restProps = cats.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => !used.Contains(x.Name) && x.CanWrite && x.PropertyType.Equals(restType));

            foreach (var pi in restProps)
            {
                pi.SetValue(cats, RestrictionCategories.Default.Value);
            }

            return services.AddSingleton(cats);
        }
    }
}
