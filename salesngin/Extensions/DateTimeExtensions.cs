namespace salesngin.Extensions;

public static class DateTimeExtensions
{

    public static int Age(this DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }
    public static DateTime StartOfDay(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day);
    }

    public static DateTime EndOfDay(this DateTime date)
    {
        return date.StartOfDay().AddDays(1).AddTicks(-1);
    }

    public static bool IsBetween(this DateTime date, DateTime start, DateTime end)
    {
        return date >= start && date <= end;
    }
    public static string ToFormattedString(this DateTime date, string format = "dd-MMM-yyyy")
    {
        return date.ToString(format);
    }

    public static string ToFriendlyDateString(this DateTime date)
    {
        return date.ToString("dd MMM yyyy");
    }

    public static string ToFriendlyTimeString(this DateTime date)
    {
        return date.ToString("hh:mm tt");
    }

    public static string ToFriendlyDateTimeString(this DateTime date)
    {
        return $"{date.ToFriendlyDateString()} at {date.ToFriendlyTimeString()}";
    }

    public static string ToFriendlyString(this DateTime date)
    {
        var now = DateTime.Now;
        var timeSpan = now - date;

        if (timeSpan.TotalDays < 1)
        {
            if (timeSpan.TotalHours < 1)
            {
                if (timeSpan.TotalMinutes < 1)
                    return "Just now";
                return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes == 1 ? "" : "s")} ago";
            }
            return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours == 1 ? "" : "s")} ago";
        }

        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays == 1 ? "" : "s")} ago";

        if (timeSpan.TotalDays < 30)
            return $"{(int)(timeSpan.TotalDays / 7)} week{((int)(timeSpan.TotalDays / 7) == 1 ? "" : "s")} ago";

        if (timeSpan.TotalDays < 365)
            return $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) == 1 ? "" : "s")} ago";

        return $"{(int)(timeSpan.TotalDays / 365)} year{((int)(timeSpan.TotalDays / 365) == 1 ? "" : "s")} ago";
    }

    public static DateTime AddBusinessDays(this DateTime date, int days)
    {
        var result = date;
        var increment = days > 0 ? 1 : -1;
        days = Math.Abs(days);

        for (int i = 0; i < days; i++)
        {
            do
            {
                result = result.AddDays(increment);
            }
            while (result.IsWeekend());
        }

        return result;
    }

    public static int BusinessDaysUntil(this DateTime fromDate, DateTime toDate)
    {
        var days = 0;
        var current = fromDate;

        while (current.Date < toDate.Date)
        {
            current = current.AddDays(1);
            if (current.IsWeekday())
                days++;
        }

        return days;
    }


    public static string ToRelativeTimeString(this DateTime date)
    {
        var timeSpan = DateTime.Now - date;

        if (timeSpan.TotalSeconds < 60)
            return $"{(int)timeSpan.TotalSeconds} seconds ago";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minutes ago";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hours ago";
        if (timeSpan.TotalDays < 30)
            return $"{(int)timeSpan.TotalDays} days ago";
        if (timeSpan.TotalDays < 365)
            return $"{(int)(timeSpan.TotalDays / 30)} months ago";

        return $"{(int)(timeSpan.TotalDays / 365)} years ago";
    }

    public static DateTime ToUtc(this DateTime date)
    {
        return TimeZoneInfo.ConvertTimeToUtc(date);
    }

    public static DateTime ToLocal(this DateTime date)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(date, TimeZoneInfo.Local);
    }

    public static DateTime? ToUtc(this DateTime? date)
    {
        return date.HasValue ? TimeZoneInfo.ConvertTimeToUtc(date.Value) : (DateTime?)null;
    }

    public static DateTime? ToLocal(this DateTime? date)
    {
        return date.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(date.Value, TimeZoneInfo.Local) : (DateTime?)null;
    }

    public static DateTime? ToStartOfDay(this DateTime? date)
    {
        return date.HasValue ? date.Value.StartOfDay() : (DateTime?)null;
    }

    public static DateTime? ToEndOfDay(this DateTime? date)
    {
        return date.HasValue ? date.Value.EndOfDay() : (DateTime?)null;
    }
    public static int DaysUntil(this DateTime date)
    {
        return (date - DateTime.Now).Days;
    }
    public static bool IsToday(this DateTime date)
    {
        return date.Date == DateTime.Now.Date;
    }
    public static bool IsWeekend(this DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }
    public static bool IsMonday(this DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Monday;
    }
    public static bool IsWeekday(this DateTime date)
    {
        return !date.IsWeekend();
    }
    public static bool IsFuture(this DateTime date)
    {
        return date > DateTime.Now;
    }
    public static bool IsPast(this DateTime date)
    {
        return date < DateTime.Now;
    }
    public static bool IsLeapYear(this DateTime date)
    {
        return DateTime.IsLeapYear(date.Year);
    }
    public static int DaysInMonth(this DateTime date)
    {
        return DateTime.DaysInMonth(date.Year, date.Month);
    }
    public static int WeeksInMonth(this DateTime date)
    {
        var firstDay = new DateTime(date.Year, date.Month, 1);
        var lastDay = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        return (int)Math.Ceiling((lastDay - firstDay).TotalDays / 7.0);
    }
    public static DateTime FirstDayOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }
    public static DateTime LastDayOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
    }
    public static DateTime FirstDayOfYear(this DateTime date)
    {
        return new DateTime(date.Year, 1, 1);
    }
    public static DateTime LastDayOfYear(this DateTime date)
    {
        return new DateTime(date.Year, 12, 31);
    }
    public static DateTime FirstDayOfWeek(this DateTime date, DayOfWeek firstDayOfWeek = DayOfWeek.Sunday)
    {
        int diff = (7 + (date.DayOfWeek - firstDayOfWeek)) % 7;
        return date.AddDays(-1 * diff).Date;
    }
    public static DateTime LastDayOfWeek(this DateTime date, DayOfWeek firstDayOfWeek = DayOfWeek.Sunday)
    {
        int diff = (7 - (date.DayOfWeek - firstDayOfWeek)) % 7;
        return date.AddDays(diff).Date;
    }
    public static int WeeksUntil(this DateTime date)
    {
        return (int)Math.Ceiling((date - DateTime.Now).TotalDays / 7.0);
    }
}


// Reusable across your entire application
// DateTime today = DateTime.Now;
// DateTime dayStart = today.StartOfDay();
// DateTime dayEnd = today.EndOfDay();
// bool inRange = someDate.IsBetween(dayStart, dayEnd);