using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AD.Api.Ldap.Components.PropertySortDirection;

namespace AD.Api.Ldap.Components
{
    public struct PropertySortDirection : IConvertible, IEquatable<PropertySortDirection>, IEquatable<SortDirection>,
        IEquatable<DirectionValue>, IComparable<PropertySortDirection>
    {
        public SortDirection Value { get; }

        public PropertySortDirection(SortDirection value)
        {
            this.Value = value;
        }

        public static readonly PropertySortDirection Default = new(SortDirection.Ascending);

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is SortDirection sd)
                return this.Equals(sd);

            else if (obj is DirectionValue dv)
                return this.Equals(dv);

            else
                return false;
        }

        public bool Equals(SortDirection other)
        {
            return this.Value.Equals(other);
        }

        public bool Equals(DirectionValue other)
        {
            return
                (this.Value == SortDirection.Ascending && (
                    other == DirectionValue.Asc || other == DirectionValue.Ascending))
                ||
                (this.Value == SortDirection.Descending && (
                    other == DirectionValue.Desc || other == DirectionValue.Descending));
        }

        public bool Equals(PropertySortDirection other)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public enum DirectionValue
        {
            Ascending,
            Descending,
            Asc,
            Desc,
        }

        public static implicit operator PropertySortDirection(string? strValue)
        {
            return Enum.TryParse(strValue, true, out DirectionValue value)
                ? FromDirection(value)
                : new PropertySortDirection(SortDirection.Ascending);
        }
        
        public static PropertySortDirection FromDirection(DirectionValue value)
        {
            switch (value)
            {
                case DirectionValue.Asc:
                case DirectionValue.Ascending:
                    return new PropertySortDirection(SortDirection.Ascending);

                case DirectionValue.Desc:
                case DirectionValue.Descending:
                    return new PropertySortDirection(SortDirection.Descending);

                default:
                    goto case DirectionValue.Ascending;
            }
        }

        public static implicit operator SortDirection(PropertySortDirection psd) => psd.Value;

        public TypeCode GetTypeCode()
        {
            return this.Value.GetTypeCode();
        }

        public bool ToBoolean(IFormatProvider? provider)
        {
            return ((IConvertible)this.Value).ToBoolean(provider);
        }

        public byte ToByte(IFormatProvider? provider)
        {
            return ((IConvertible)this.Value).ToByte(provider);
        }

        public char ToChar(IFormatProvider? provider)
        {
            return ((IConvertible)this.Value).ToChar(provider);
        }

        public DateTime ToDateTime(IFormatProvider? provider)
        {
            return ((IConvertible)this.Value).ToDateTime(provider);
        }

        public decimal ToDecimal(IFormatProvider? provider)
        {
            return ((IConvertible)this.Value).ToDecimal(provider);
        }

        public double ToDouble(IFormatProvider? provider)
        {
            return ((IConvertible)this.Value).ToDouble(provider);
        }

        public short ToInt16(IFormatProvider? provider)
        {
            return ((IConvertible)this.Value).ToInt16(provider);
        }

        public int ToInt32(IFormatProvider? provider)
        {
            return ((IConvertible)this.Value).ToInt32(provider);
        }

        public long ToInt64(IFormatProvider? provider)
        {
            return ((IConvertible)this.Value).ToInt64(provider);
        }

        public sbyte ToSByte(IFormatProvider? provider)
        {
            return ((IConvertible)this.Value).ToSByte(provider);
        }

        public float ToSingle(IFormatProvider? provider)
        {
            return ((IConvertible)this.Value).ToSingle(provider);
        }

        public string ToString(IFormatProvider? provider)
        {
            return this.Value.ToString();
        }

        public object ToType(Type conversionType, IFormatProvider? provider)
        {
            return ((IConvertible)this.Value).ToType(conversionType, provider);
        }

        public ushort ToUInt16(IFormatProvider? provider)
        {
            return ((IConvertible)this.Value).ToUInt16(provider);
        }

        public uint ToUInt32(IFormatProvider? provider)
        {
            return ((IConvertible)this.Value).ToUInt32(provider);
        }

        public ulong ToUInt64(IFormatProvider? provider)
        {
            return ((IConvertible)this.Value).ToUInt64(provider);
        }

        public int CompareTo(PropertySortDirection other)
        {
            return this.Value.CompareTo(other.Value);
        }

        #region OPERATORS
        public static bool operator ==(PropertySortDirection x, PropertySortDirection y)
        {
            return x.Value == y.Value;
        }
        public static bool operator !=(PropertySortDirection x, PropertySortDirection y)
        {
            return !(x == y);
        }

        #endregion
    }
}
