﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Filters
{
    /// <summary>
    /// A filter class which indicates that the provided equality statement must be <see langword="false"/>.
    /// </summary>
    public sealed class Not : FilterStatementBase
    {
        private readonly EqualityStatement _equalStatement;

        internal EqualityStatement EqualStatement => _equalStatement;
        public sealed override FilterType Type => FilterType.Not;

        /// <summary>
        /// The property name used in the LDAP filter.
        /// </summary>
        public string Property => _equalStatement.Property;

        /// <summary>
        /// The converted <see cref="string"/> value the property must equal.
        /// </summary>
        public string Value => _equalStatement.GetValue();

        /// <summary>
        /// Initializes a new instance of <see cref="Not"/> with the specified property name and value.
        /// </summary>
        /// <param name="property">The LDAP property name of the filter.</param>
        /// <param name="value">The value that the filter must not equal.</param>
        public Not(string property, IConvertible value)
        {
            _equalStatement = new Equal(property, value);
        }
        internal Not(EqualityStatement equalityStatement)
        {
            _equalStatement = equalityStatement;
        }

        public override void WriteTo(JsonWriter writer, NamingStrategy strategy, JsonSerializer serializer)
        {
            string name = strategy.GetPropertyName(nameof(Not), false);
            writer.WritePropertyName(name);

            writer.WriteStartObject();

            _equalStatement.WriteTo(writer, strategy, serializer);

            writer.WriteEndObject();
        }

        public sealed override StringBuilder WriteTo(StringBuilder builder)
        {
            builder.Append("(!");

            return _equalStatement.WriteTo(builder).Append((char)41);
        }
    }
}
