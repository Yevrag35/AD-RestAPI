using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Strings
{
    public static class Base64Extensions
    {
        public static int GetByteLength(ReadOnlySpan<char> base64Text)
        {
            if (base64Text.IsEmpty)
            {
                return 0;
            }

            return ((base64Text.Length * 3) + 3) / 4;
        }
    }
}

