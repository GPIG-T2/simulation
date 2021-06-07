using System;
using System.Collections.Generic;
using System.Linq;
using Models;

namespace Virus.Test
{
    public static class Extensions
    {
        public static string Format(this string format, params object?[] args)
            => string.Format(format, args);
    }
}
