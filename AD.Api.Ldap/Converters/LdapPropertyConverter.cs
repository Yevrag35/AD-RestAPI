using AD.Api.Ldap.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Converters
{
    /// <summary>
    /// Converts an LDAP property's value.
    /// </summary>
    public abstract class LdapPropertyConverter
    {
        /// <summary>
        /// Determines whether this <see cref="LdapPropertyConverter"/> can the specified object <see cref="Type"/>.
        /// </summary>
        /// <param name="objectType">The type of the object.</param>
        /// <returns>
        ///     <see langword="true"/> if this instance can convert the specified object <see cref="Type"/>;
        ///     otherwise, <see langword="false"/>.
        /// </returns>
        public abstract bool CanConvert(Type objectType);

        /// <summary>
        /// Converts the parsed value from the command line.
        /// </summary>
        /// <param name="attribute">The attribute that the <paramref name="rawValue"/> matched.</param>
        /// <param name="rawValue">The parsed value from the command line to convert.</param>
        /// <param name="existingValue">The existing value, if any, of the member on which this attribute was decorated.</param>
        /// <returns>
        ///     The converted object.
        /// </returns>
        public abstract object? Convert(LdapPropertyAttribute attribute, object? rawValue, object? existingValue);
    }

    /// <summary>
    /// Converts an LDAP proeprty's value to an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to convert the value to.</typeparam>
    public abstract class LdapPropertyConverter<T> : LdapPropertyConverter
    {
        /// <summary>
        /// Converts the parsed value from the command line to an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="attribute">The attribute that the <paramref name="rawValue"/> matched.</param>
        /// <param name="rawValue">The parsed value from the command line to convert.</param>
        /// <param name="existingValue">The existing value, if any, of the member on which this attribute was decorated.</param>
        /// <param name="hasExistingValue"><paramref name="existingValue"/> has a value.</param>
        /// <returns>
        ///     The converted object of type <typeparamref name="T"/>.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="existingValue"/> is of the wrong <see cref="Type"/>.</exception>
        public abstract T? Convert(LdapPropertyAttribute attribute, object? rawValue, T? existingValue, bool hasExistingValue);

        /// <summary>
        /// Converts the parsed value from the command line.
        /// </summary>
        /// <param name="attribute">The attribute that the <paramref name="rawValue"/> matched.</param>
        /// <param name="rawValue">The parsed value from the command line to convert.</param>
        /// <param name="existingValue">The existing value, if any, of the member on which this attribute was decorated.</param>
        /// <returns>
        ///     The converted object.
        /// </returns>
        public sealed override object? Convert(LdapPropertyAttribute attribute, object? rawValue, object? existingValue)
        {
            bool existingIsNull = existingValue is null;
            if (!(existingValue is null || existingValue is T))
                throw new ArgumentException(string.Format("Converter cannot process the existing value.  '{0}' is required.",
                    typeof(T)));

            return this.Convert(attribute, rawValue, existingIsNull ? default : (T?)existingValue, !existingIsNull);
        }

        /// <summary>
        /// Determines whether this <see cref="LdapPropertyConverter{T}"/> can the specified object <see cref="Type"/>.
        /// </summary>
        /// <param name="objectType">The type of the object.</param>
        /// <returns>
        ///     <see langword="true"/> if this instance can convert the specified object <see cref="Type"/>;
        ///     otherwise, <see langword="false"/>.
        /// </returns>
        public sealed override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }
    }
}
