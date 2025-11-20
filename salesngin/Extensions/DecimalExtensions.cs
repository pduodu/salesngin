namespace posone.Extensions;

    public static class DecimalExtensions
    {
        #region Rounding and Precision

        public static decimal RoundToTwoDecimalPlaces(this decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        public static decimal RoundTo(this decimal value, int decimalPlaces)
        {
            return Math.Round(value, decimalPlaces, MidpointRounding.AwayFromZero);
        }

        public static decimal RoundToNearest(this decimal value, decimal nearest)
        {
            return Math.Round(value / nearest) * nearest;
        }

        public static decimal Ceiling(this decimal value, int decimalPlaces = 2)
        {
            var multiplier = (decimal)Math.Pow(10, decimalPlaces);
            return Math.Ceiling(value * multiplier) / multiplier;
        }

        public static decimal Floor(this decimal value, int decimalPlaces = 2)
        {
            var multiplier = (decimal)Math.Pow(10, decimalPlaces);
            return Math.Floor(value * multiplier) / multiplier;
        }

        #endregion

        #region Financial Calculations

        public static decimal AddTax(this decimal amount, decimal taxRate)
        {
            return amount * (1 + taxRate);
        }

        public static decimal RemoveTax(this decimal amountWithTax, decimal taxRate)
        {
            return amountWithTax / (1 + taxRate);
        }

        public static decimal GetTaxAmount(this decimal amount, decimal taxRate)
        {
            return amount * taxRate;
        }

        public static decimal ApplyDiscount(this decimal amount, decimal discountPercentage)
        {
            return amount * (1 - discountPercentage / 100);
        }

        public static decimal GetDiscountAmount(this decimal amount, decimal discountPercentage)
        {
            return amount * (discountPercentage / 100);
        }

        public static decimal CalculatePercentage(this decimal value, decimal total)
        {
            return total == 0 ? 0 : (value / total) * 100;
        }

        public static decimal PercentageOf(this decimal percentage, decimal total)
        {
            return (percentage / 100) * total;
        }

        #endregion

        #region Comparison and Validation

        public static bool IsPositive(this decimal value)
        {
            return value > 0;
        }

        public static bool IsNegative(this decimal value)
        {
            return value < 0;
        }

        public static bool IsZero(this decimal value)
        {
            return value == 0;
        }

        public static bool IsBetween(this decimal value, decimal min, decimal max, bool inclusive = true)
        {
            return inclusive ? (value >= min && value <= max) : (value > min && value < max);
        }

        public static decimal Clamp(this decimal value, decimal min, decimal max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        public static decimal Abs(this decimal value)
        {
            return Math.Abs(value);
        }

        #endregion

        #region String Formatting

        public static string ToCurrency(this decimal value, string currencySymbol = "₵")
        {
            return $"{currencySymbol}{value:N2}";
        }

        public static string ToFormattedString(this decimal value, string format = "N2")
        {
            return value.ToString(format);
        }

        public static string ToPercentage(this decimal value, int decimalPlaces = 1)
        {
            return $"{Math.Round(value, decimalPlaces)}%";
        }

        public static string ToWords(this decimal value)
        {
            var integerPart = (int)Math.Truncate(value);
            var decimalPart = (int)Math.Round((value - integerPart) * 100);

            var result = integerPart.ToWords();

            if (decimalPart > 0)
            {
                result += $" and {decimalPart}/100";
            }

            return result;
        }

        #endregion

        #region Nullable Decimal Extensions

        public static decimal GetValueOrDefault(this decimal? value, decimal defaultValue = 0m)
        {
            return value ?? defaultValue;
        }

        public static string ToCurrency(this decimal? value, string currencySymbol = "₵", string nullDisplay = "N/A")
        {
            return value?.ToCurrency(currencySymbol) ?? nullDisplay;
        }

        public static bool HasValue(this decimal? value)
        {
            return value.HasValue && value.Value != 0;
        }

        #endregion
        public static decimal ToSafeDecimal(this object value, decimal defaultValue = 0m)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            if (decimal.TryParse(value.ToString(), out var result))
                return result;

            return defaultValue;
        }
    }

