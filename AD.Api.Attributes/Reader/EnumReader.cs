namespace AD.Api.Ldap.Attributes
{
    [Obsolete("Use 'IEnumValues' instead.")]
    public static class EnumReader<T> where T : Attribute
    {
        public static bool TryGetAttribute(Enum e, [NotNullWhen(true)] out T? attribute)
        {
            attribute = default;
            FieldInfo? fi = GetFieldInfo(e);

            if (fi is null)
                return false;

            if (fi.CustomAttributes.Any(att => typeof(T).IsAssignableFrom(att.AttributeType)))
            {
                attribute = fi.GetCustomAttributes<T>()?.FirstOrDefault();
            }

            return !(attribute is null);
        }

        public static bool TryGetAttributes(Enum e, [MaybeNullWhen(false)] out T[] attributes)
        {
            attributes = null;
            FieldInfo? fi = GetFieldInfo(e);

            if (fi is null)
                return false;

            if (fi.CustomAttributes.Any(att => typeof(T).IsAssignableFrom(att.AttributeType)))
            {
                attributes = fi.GetCustomAttributes<T>()?.ToArray();
            }

            return !(attributes is null || attributes.Length <= 0);
        }

        private static FieldInfo? GetFieldInfo(Enum e)
        {
            return e
                .GetType()
                .GetRuntimeField(e.ToString());
        }
    }
}
