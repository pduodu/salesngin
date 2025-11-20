namespace salesngin.Extensions;

public static class CollectionExtensions
{
    #region Null and Empty Checks

    public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
    {
        return collection == null || !collection.Any();
    }

    public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> collection)
    {
        return !collection.IsNullOrEmpty();
    }

    public static bool HasItems<T>(this IEnumerable<T> collection)
    {
        return collection?.Any() == true;
    }

    #endregion

    #region Functional Extensions

    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        if (collection == null || action == null) return;

        foreach (var item in collection)
        {
            action(item);
        }
    }

    public static void ForEachWithIndex<T>(this IEnumerable<T> collection, Action<T, int> action)
    {
        if (collection == null || action == null) return;

        var index = 0;
        foreach (var item in collection)
        {
            action(item, index++);
        }
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> collection) where T : class
    {
        return collection?.Where(item => item != null)!;
    }

    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> collection, Func<T, TKey> keySelector)
    {
        var seenKeys = new HashSet<TKey>();
        foreach (var element in collection)
        {
            if (seenKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }

    #endregion

    #region Chunking and Batching

    public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> collection, int chunkSize)
    {
        if (chunkSize <= 0) throw new ArgumentException("Chunk size must be greater than 0", nameof(chunkSize));

        var list = collection.ToList();
        for (int i = 0; i < list.Count; i += chunkSize)
        {
            yield return list.Skip(i).Take(chunkSize);
        }
    }

    public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> collection, int batchSize)
    {
        var batch = new List<T>(batchSize);
        foreach (var item in collection)
        {
            batch.Add(item);
            if (batch.Count == batchSize)
            {
                yield return batch;
                batch = new List<T>(batchSize);
            }
        }
        if (batch.Count > 0)
            yield return batch;
    }

    #endregion

    #region String Operations

    public static string JoinToString<T>(this IEnumerable<T> collection, string separator = ", ")
    {
        return collection == null ? string.Empty : string.Join(separator, collection);
    }

    public static string JoinToString<T>(this IEnumerable<T> collection, string separator, Func<T, string> selector)
    {
        return collection?.Select(selector).JoinToString(separator) ?? string.Empty;
    }

    #endregion

    #region Mathematical Operations

    public static decimal SumCollection<T>(this IEnumerable<T> collection, Func<T, decimal?> selector)
    {
        return collection?.Select(selector).Where(x => x.HasValue).Sum(x => x.Value) ?? 0m;
    }

    public static decimal Average<T>(this IEnumerable<T> collection, Func<T, decimal?> selector)
    {
        var values = collection?.Select(selector).Where(x => x.HasValue).Select(x => x.Value).ToList();
        return values?.Any() == true ? values.Average() : 0m;
    }

    public static T MaxBy<T, TKey>(this IEnumerable<T> collection, Func<T, TKey> selector) where TKey : IComparable<TKey>
    {
        return collection.OrderByDescending(selector).FirstOrDefault();
    }

    public static T MinBy<T, TKey>(this IEnumerable<T> collection, Func<T, TKey> selector) where TKey : IComparable<TKey>
    {
        return collection.OrderBy(selector).FirstOrDefault();
    }

    #endregion

    #region Safe Operations

    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> collection)
    {
        return collection ?? Enumerable.Empty<T>();
    }

    public static T FirstOrDefault<T>(this IEnumerable<T> collection, T defaultValue)
    {
        if (collection == null)
            return defaultValue;

        using (var enumerator = collection.GetEnumerator())
        {
            if (enumerator.MoveNext())
                return enumerator.Current;
        }
        return defaultValue;
    }

    public static List<T> ToSafeList<T>(this IEnumerable<T> collection)
    {
        return collection?.ToList() ?? new List<T>();
    }

    #endregion
}


// This extension method allows you to conditionally apply a filter to an IQueryable.
// Usage example:
// var filtered = dbContext.Users
//     .WhereIf(isActive, u => u.IsActive)
//     .WhereIf(hasEmail, u => u.Email != null);

// Clean, readable chaining
// var results = _context.Sales
//     .WhereIf(!string.IsNullOrEmpty(searchTerm), s => s.SalesCode.Contains(searchTerm))
//     .WhereIf(startDate.HasValue, s => s.SalesDate >= startDate)
//     .WhereIf(endDate.HasValue, s => s.SalesDate <= endDate)
//     .ToList();