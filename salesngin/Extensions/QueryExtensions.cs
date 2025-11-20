namespace salesngin.Extensions;

public static class QueryExtensions
{
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }
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
