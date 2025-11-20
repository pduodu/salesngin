using Microsoft.EntityFrameworkCore;
namespace salesngin.Services;

public class CustomCodeGenerator
{
    private static int seed = Environment.TickCount;
    private static readonly object lockObject = new();
    private static readonly Random random = new();
    private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string GenerateCode(int size)
    {
        //const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string code;

        lock (lockObject)
        {
            var random = new Random(Interlocked.Increment(ref seed));
            code = new string(Enumerable.Repeat(chars, size).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        return code;
    }

    // Generates a random alphanumeric code of a specified length
    public static string GenerateRandomAlphanumericCode(int length)
    {
        StringBuilder codeBuilder = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            codeBuilder.Append(Characters[random.Next(Characters.Length)]);
        }

        return codeBuilder.ToString();
    }

}

//public static async Task<string> GenerateCustomIdNumber<T>(string prefixText, DbSet<T> dbSet, Func<T, string> getIdNumber, Func<T, DateTime?> getDateCreated) where T : class
public static class IDNumberGenerator
{
    public static async Task<string> GenerateCustomIdNumber<T>(
        string prefixText,
        string v,
        DbSet<T> dbSet,
        Expression<Func<T, string>> getIdNumber,
        Expression<Func<T, bool>> datePredicate
        ) where T : class
    {
        var currentDate = DateTime.UtcNow;
        // Get the total count of records (if counting all entities is enough)
        var lastRecordNumber = await dbSet.AsNoTracking().CountAsync(datePredicate);

        //lastRecordNumber = entities.Count;
        string suffix = CustomCodeGenerator.GenerateCode(3);
        string newIdNumber = string.Empty;
        // Prepare the parameter for the ID check expression
        var parameter = getIdNumber.Parameters.Single();
        Expression<Func<T, bool>> idCheckExpression = null;
        do
        {
            lastRecordNumber += 1;
            newIdNumber = $"{currentDate:yyyy}{prefixText}{currentDate:MM}{lastRecordNumber:D2}{suffix}";
            // Create the filter for the AnyAsync query
            idCheckExpression = Expression.Lambda<Func<T, bool>>(
                Expression.Equal(getIdNumber.Body, Expression.Constant(newIdNumber)),
                parameter
            );

        } while (await dbSet.AsNoTracking().AnyAsync(idCheckExpression));
        //} while (await dbSet.AsNoTracking().AnyAsync(e => getIdNumber(e) == newIdNumber));

        return newIdNumber;
    }
}

//if (entities.Count > 0)
//{
//    var filteredRecords = entities.Where(e =>
//        getDateCreated(e) != null &&
//        getDateCreated(e).Value.Year == currentYear &&
//        getDateCreated(e).Value.Month == currentMonth
//    ).ToList();

//    recordCount = filteredRecords.Count; //Return Count of records filtered by Year & Months

//    lastRecordNumber = entities.Count;
//}



