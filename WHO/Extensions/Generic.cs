using System.Collections.Generic;

namespace WHO.Extensions
{
    public static class Generic
    {
        public static string ToKey(this IEnumerable<string> loc) => string.Join(",", loc);

        public static List<string> BuildLocation(IReadOnlyList<string>? parent, string coord)
        {
            List<string> result = parent == null ? new() : new(parent);
            result.Add(coord);
            return result;
        }
    }
}
