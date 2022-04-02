using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Filter
{
    public struct PropertyEqual : IFilterStatement
    {
        private const string FORMAT = "({0}={1})";
        private const string STAR = "*";
        private const string BITWISE_OR = "{0}:1.2.840.113556.1.4.803:";
        private const string BITWISE_AND = "{0}:1.2.840.113556.1.4.804:";
        private bool _bitwiseAnd;
        private bool _bitwiseOr;
        private readonly string _prop;
        private readonly string _realProp;
        private readonly string _value;

        public bool IsBitwiseAnd => _bitwiseAnd;
        public bool IsBitwiseOr => _bitwiseOr;
        public string Property => _realProp;
        public string Value => _value;

        public PropertyEqual(string property, IConvertible? value)
        {
            _bitwiseAnd = false;
            _bitwiseOr = false;
            _prop = property;
            _realProp = property;

            if (value == null)
                value = STAR;

            _value = Convert.ToString(value);
        }
        private PropertyEqual(string bitwiseProperty, bool bitwiseAnd, bool bitwiseOr, string realProperty, IConvertible value)
        {
            _prop = bitwiseProperty;
            _bitwiseAnd = bitwiseAnd;
            _bitwiseOr = bitwiseOr;
            _realProp = realProperty;
            _value = Convert.ToString(value, CultureInfo.CurrentCulture);
        }
        public static PropertyEqual BitwiseAnd(string property, int value)
        {
            string realProperty = property;
            if (string.IsNullOrWhiteSpace(property))
                throw new ArgumentNullException(nameof(property));

            property = string.Format(BITWISE_AND, property);

            return new PropertyEqual(property, true, false, realProperty, value);
        }
        public static PropertyEqual BitwiseAnd(string property, Enum enumValue)
        {
            return BitwiseAnd(property, Convert.ToInt32(enumValue));
        }
        public static PropertyEqual BitwiseOr(string property, int value)
        {
            string realProperty = property;
            if (string.IsNullOrWhiteSpace(property))
                throw new ArgumentNullException(nameof(property));

            property = string.Format(BITWISE_OR, property);

            return new PropertyEqual(property, false, true, realProperty, value);
        }
        public static PropertyEqual BitwiseOr(string property, Enum enumValue)
        {
            return BitwiseOr(property, Convert.ToInt32(enumValue));
        }

        public StringBuilder Generate(StringBuilder builder)
        {
            return builder.Append(string.Format(CultureInfo.CurrentCulture, FORMAT, _prop, _value));
        }
    }
}
