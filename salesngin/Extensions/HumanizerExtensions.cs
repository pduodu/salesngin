namespace salesngin.Extensions;

public static class HumanizerExtensions
{
    public static string UnitDisplayWithQuantity(this string unit, int quantity)
    {
        return $"{quantity} {(quantity == 1 ? unit.Singularize(false) : unit.Pluralize(false))}";
    }

    public static string UnitDisplay(this string unit, int quantity)
    {
        return $"{(quantity == 1 ? unit.Singularize(false) : unit.Pluralize(false))}";
    }

    public static string HumanizeDate(this DateTime date)
    {
        return date.Humanize();
    }
}