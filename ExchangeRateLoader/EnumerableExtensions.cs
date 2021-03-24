using System;
using System.Collections.Generic;

namespace ExchangeRateLoader
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Paginate<T>(this IEnumerable<T> seq, int pageSize)
        {
            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            using var enumerator = seq.GetEnumerator();

            while (true)
            {
                var t = Seek(enumerator, pageSize);
                if (t != null)
                    yield return t;
                else yield break;
            }

            static IEnumerable<T>? Seek(IEnumerator<T> iterator, int count)
            {
                return iterator.MoveNext() ? Enumerable(iterator.Current, iterator, count - 1) : null;

                static IEnumerable<T> Enumerable(T first, IEnumerator<T> iterator, int count)
                {
                    yield return iterator.Current;
                    while (count > 0 && iterator.MoveNext())
                    {
                        yield return iterator.Current;
                        count--;
                    }
                }
            }
        }
    }
}
