namespace AD.Api.Ldap.Path
{
    public struct CommonName
    {
        private readonly string? _cn;

        public string Value => _cn ?? string.Empty;

        private CommonName(string? wholeString)
        {
            _cn = wholeString;
        }

        public static CommonName Create(string name, bool isOU = false)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name.StartsWith(Strings.CN_Prefix) || name.StartsWith(Strings.OU_Prefix))
            {
                return new CommonName(name);
            }

            (string Name, string Format) tuple = (name, isOU ? Strings.OU_Prefix : Strings.CN_Prefix);

            return new CommonName(string.Create(tuple.Name.Length + tuple.Format.Length, tuple, (chars, state) =>
            {
                int fLength = state.Format.Length;

                state.Format.AsSpan().CopyTo(chars);
                state.Name.AsSpan().CopyTo(chars.Slice(fLength));
            }));
        }

        public static implicit operator CommonName(string name) => Create(name);
    }
}
