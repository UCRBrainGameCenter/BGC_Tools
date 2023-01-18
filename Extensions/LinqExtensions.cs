using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.PackageManager;

namespace BGC.Extensions
{
    public static class LinqExtensions
    {
        public static IEnumerable<TResult> Map<TSource, TResult>(
            this IEnumerable<TSource> source,
            IReadOnlyDictionary<TSource, TResult> map)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (map is null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            return source.Select(x => map[x]);
        }
    }
}
