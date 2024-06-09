using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Nodes;

namespace AD.Api.Extensions
{
    public static class JsonNodeExtensions
    {
        public static bool TryAsArray([NotNullWhen(true)] this JsonNode? node, [NotNullWhen(true)] out JsonArray? value)
        {
            if (node is not null)
            {
                try
                {
                    value = node.AsArray();
                }
                catch
                {
                    value = null;
                }

                return value is not null;
            }

            value = null;
            return false;
        }

        public static bool TryAsObject([NotNullWhen(true)] this JsonNode? node, [NotNullWhen(true)] out JsonObject? value)
        {
            if (node is not null)
            {
                try
                {
                    value = node.AsObject();
                }
                catch
                {
                    value = null;
                }

                return value is not null;
            }

            value = null;
            return false;
        }

        public static bool TryAsValue([NotNullWhen(true)] this JsonNode? node, [NotNullWhen(true)] out JsonValue? value)
        {
            if (node is not null)
            {
                try
                {
                    value = node.AsValue();
                }
                catch
                {
                    value = null;
                }

                return value is not null;
            }

            value = null;
            return false;
        }
    }
}