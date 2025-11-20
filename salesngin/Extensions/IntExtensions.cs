namespace salesngin.Extensions;

public static class IntExtensions
{
    #region Mathematical Extensions

    public static bool IsEven(this int number)
    {
        return number % 2 == 0;
    }

    public static bool IsOdd(this int number)
    {
        return number % 2 != 0;
    }

    public static bool IsPrime(this int number)
    {
        if (number <= 1) return false;
        if (number == 2) return true;
        if (number % 2 == 0) return false;

        for (int i = 3; i <= Math.Sqrt(number); i += 2)
        {
            if (number % i == 0) return false;
        }
        return true;
    }

    public static int Factorial(this int number)
    {
        if (number < 0) throw new ArgumentException("Factorial is not defined for negative numbers");
        if (number == 0 || number == 1) return 1;

        int result = 1;
        for (int i = 2; i <= number; i++)
        {
            result *= i;
        }
        return result;
    }

    public static double Power(this int baseNumber, int exponent)
    {
        return Math.Pow(baseNumber, exponent);
    }

    #endregion

    #region Range and Validation

    public static bool IsBetween(this int number, int min, int max, bool inclusive = true)
    {
        return inclusive ? (number >= min && number <= max) : (number > min && number < max);
    }

    public static int Clamp(this int number, int min, int max)
    {
        return Math.Min(Math.Max(number, min), max);
    }

    public static bool IsPositive(this int number)
    {
        return number > 0;
    }

    public static bool IsNegative(this int number)
    {
        return number < 0;
    }

    public static bool IsZero(this int number)
    {
        return number == 0;
    }

    #endregion

    #region String Formatting

    public static string ToOrdinal(this int number)
    {
        if (number <= 0) return number.ToString();

        return (number % 100) switch
        {
            11 or 12 or 13 => number + "th",
            _ => (number % 10) switch
            {
                1 => number + "st",
                2 => number + "nd",
                3 => number + "rd",
                _ => number + "th"
            }
        };
    }

    public static string ToWords(this int number)
    {
        if (number == 0) return "zero";

        string[] ones = { "", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
        string[] teens = { "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
        string[] tens = { "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

        if (number < 0) return "negative " + (-number).ToWords();
        if (number < 10) return ones[number];
        if (number < 20) return teens[number - 10];
        if (number < 100) return tens[number / 10] + (number % 10 != 0 ? "-" + ones[number % 10] : "");
        if (number < 1000) return ones[number / 100] + " hundred" + (number % 100 != 0 ? " " + (number % 100).ToWords() : "");

        return number.ToString(); // For numbers 1000 and above, return the number as string
    }

    public static string ToFormattedString(this int number, string format = "N0")
    {
        return number.ToString(format);
    }

    #endregion

    #region Time Extensions

    public static TimeSpan Days(this int number)
    {
        return TimeSpan.FromDays(number);
    }

    public static TimeSpan Hours(this int number)
    {
        return TimeSpan.FromHours(number);
    }

    public static TimeSpan Minutes(this int number)
    {
        return TimeSpan.FromMinutes(number);
    }

    public static TimeSpan Seconds(this int number)
    {
        return TimeSpan.FromSeconds(number);
    }

    #endregion

    #region Iteration

    public static void Times(this int number, Action action)
    {
        for (int i = 0; i < number; i++)
        {
            action();
        }
    }

    public static void Times(this int number, Action<int> action)
    {
        for (int i = 0; i < number; i++)
        {
            action(i);
        }
    }

    public static IEnumerable<T> Times<T>(this int number, Func<int, T> func)
    {
        for (int i = 0; i < number; i++)
        {
            yield return func(i);
        }
    }

    #endregion
}

