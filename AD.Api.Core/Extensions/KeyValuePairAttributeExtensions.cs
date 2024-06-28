using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Extensions
{
    internal static class KeyValuePairAttributeExtensions
    {
        /// <summary>
        /// Projects the current <see cref="KeyValuePair{TKey, TValue}"/> to a <see cref="DirectoryAttribute"/> 
        /// object with its values formatted as an object array.
        /// </summary>
        /// <typeparam name="T">
        ///     <inheritdoc cref="KeyValuePair{TKey, TValue}" path="/typeparam[last()]"/>
        /// </typeparam>
        /// <param name="kvp">The key/value pair being extended.</param>
        /// <param name="valueArray">An array buffer of length 1 to use adding the attribute's values.</param>
        /// <returns>
        ///     The constructed <see cref="DirectoryAttribute"/>.
        /// </returns>
        internal static DirectoryAttribute ToDirectoryAttribute<T>(this KeyValuePair<string, T> kvp, object?[] valueArray)
        {
            DirectoryAttribute dirAttribute;
            if (kvp.Value is object?[] objArr)
            {
                dirAttribute = new(kvp.Key, objArr);
            }
            else if (kvp.Value is Array nonGenArr)
            {
                dirAttribute = new(kvp.Key, values: []);
                for (int i = 0; i < nonGenArr.Length; i++)
                {
                    valueArray[0] = nonGenArr.GetValue(i)?.ToString();
                    dirAttribute.AddRange(valueArray);
                }
            }
            else
            {
                valueArray[0] = kvp.Value?.ToString();
                dirAttribute = new(kvp.Key, valueArray);
            }

            return dirAttribute;
        }
    }
}

