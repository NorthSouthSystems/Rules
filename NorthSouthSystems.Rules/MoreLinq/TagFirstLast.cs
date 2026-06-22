using MoreLinq;

namespace Nss.Rules;

public static class TagFirstLastExtensions
{
    public static IEnumerable<(T Value, bool IsFirst, bool IsLast)> TagFirstLast<T>(this IEnumerable<T> source) =>
        source.TagFirstLast((v, iF, iL) => (v, iF, iL));
}