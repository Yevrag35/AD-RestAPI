using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AD.Api.Extensions
{
    public static class ArrayExtensions
    {
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
