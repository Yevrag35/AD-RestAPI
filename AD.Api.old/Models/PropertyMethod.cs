using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using AD.Api.Attributes;
using AD.Api.Components;
using AD.Api.Exceptions;
using AD.Api.Extensions;
using AD.Api.Models.Collections;

using Strings = AD.Api.Properties.Resource;

namespace AD.Api.Models
{
    [SupportedOSPlatform("windows")]
    public class PropertyMethod<T> : JsonRequestBase
    {
        private static readonly Action<PropertyMethod<T>> _verifyLists =
            (pm) =>
            {
                if (null == pm.OldValues)
                    pm.OldValues = new ADSortedValueList<T>();

                if (null == pm.NewValues)
                    pm.NewValues = new ADSortedValueList<T>();
            };

        [JsonIgnore]
        private string _reason;

        [JsonIgnore]
        public bool IsInvalid { get; internal set; }

        [JsonProperty("operation", Order = 1)]
        [JsonConverter(typeof(StringEnumConverter))]
        public Operation Operation { get; set; }

        [JsonProperty("oldValues", Order = 2)]
        public IValueCollection<T> OldValues { get; set; }

        [JsonProperty("newValues", Order = 3)]
        public IValueCollection<T> NewValues { get; set; }

        public PropertyMethod()
            : base()
        {
        }

        internal IllegalADOperationException GetException()
        {
            if (!this.IsInvalid)
                return null;

            return new IllegalADOperationException(_reason, this.Operation);
        }
        internal void CheckOperation()
        {
            if (!HasCorrectParameters(out string reason))
            {
                this.IsInvalid = true;
                _reason = reason;
            }
        }
        private bool HasCorrectParameters(out string reason)
        {
            switch (this.Operation)
            {
                case Operation.Set:
                    reason = Strings.Exception_IllegalOp_Set;
                    return ListHasValues(this.NewValues);

                case Operation.Add:
                    reason = Strings.Exception_IllegalOp_Add;
                    return ListHasValues(this.NewValues);

                case Operation.Remove:
                    reason = Strings.Exception_IllegalOp_Remove;
                    return ListHasValues(this.OldValues);

                case Operation.Replace:
                    reason = Strings.Exception_IllegalOp_Replace;
                    return AllListsHaveValues(this.OldValues, this.NewValues);

                default:
                    reason = Strings.Exception_UnknownError;
                    return false;
            }
        }

        private static bool AllListsHaveValues(params IList<T>[] lists)
        {
            if (null == lists || lists.Length <= 0)
                return false;

            return Array.TrueForAll(lists, ls => ListHasValues(ls));
        }
        private static bool ListHasValues(IList<T> list)
        {
            return null != list && list.Count > 0;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            _verifyLists(this);
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            _verifyLists(this);
        }

        public static PropertyMethod<string> NewUniqueStringPropertyMethod()
        {
            return new PropertyMethod<string>
            {
                IsInvalid = false,
                NewValues = new ADSortedValueList<string>((str1, str2) => StringComparer.CurrentCultureIgnoreCase.Compare(str1, str2)),
                OldValues = new ADSortedValueList<string>((str1, str2) => StringComparer.CurrentCultureIgnoreCase.Compare(str1, str2))
            };
        }
    }
}
