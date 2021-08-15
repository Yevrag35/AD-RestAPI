using AD.Api.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AD.Api.Extensions
{
    public static class ArrayExtensions
    {
        public static void ForEach<T>(this IValueCollection<T> col, Action<T> action)
        {
            for (int i = 0; i < col.Count; i++)
            {
                action(col[i]);
            }
        }
        public static TOutput[] ToArray<TInput, TOutput>(this TInput[] array, Func<TInput, TOutput> conversion)
        {
            if (null == array || array.Length <= 0)
                return null;

            TOutput[] outputArr = new TOutput[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                outputArr[i] = conversion(array[i]);
            }

            return outputArr;
        }
    }
}
