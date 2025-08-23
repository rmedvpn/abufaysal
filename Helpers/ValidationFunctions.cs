using System;
using System.Text.RegularExpressions;

namespace Faysal.Helpers
{
    /// <summary>
    /// Common validation functions converted from WebMatrix helpers.
    /// </summary>
    public static class ValidationFunctions
    {
        // Comparisons
        public static bool IsEqualTo<T>(T value, T comparator) where T : IComparable
            => value.Equals(comparator);

        public static bool IsGreaterThan<T>(T value, T comparator) where T : IComparable
            => value.CompareTo(comparator) > 0;

        public static bool IsLessThan<T>(T value, T comparator) where T : IComparable
            => value.CompareTo(comparator) < 0;

        public static bool IsGreaterThanOrEqualTo<T>(T value, T comparator) where T : IComparable
            => value.CompareTo(comparator) >= 0;

        public static bool IsLessThanOrEqualTo<T>(T value, T comparator) where T : IComparable
            => value.CompareTo(comparator) <= 0;

        // Range Validation
        public static bool IsBetween<T>(T value, T minValue, T maxValue) where T : IComparable
            => value.CompareTo(minValue) >= 0 && value.CompareTo(maxValue) <= 0;

        // Pattern Matching
        public static bool IsNumbersOnly(string value)
        {
            const string pattern = "^[0-9]+$";
            return Regex.IsMatch(value, pattern);
        }

        public static bool IsLettersOnly(string value)
        {
            const string pattern = "^[A-Za-z]+$";
            return Regex.IsMatch(value, pattern);
        }

        public static bool IsAlphaNumeric(string value)
        {
            const string pattern = "^[A-Za-z0-9א-ת]+$";
            return Regex.IsMatch(value, pattern);
        }

        public static bool IsValidEmail(string value)
        {
            const string pattern =
                "^([a-zA-Z0-9_\\-\\.]+)@((\\[[0-9]{1,3}" +
                "\\.[0-9]{1,3}\\.[0-9]{1,3}\\.)|(([a-zA-Z0-9\\-]+\\.)+))" +
                "([a-zA-Z]{2,4}|[0-9]{1,3})(\\]?)$";
            return Regex.IsMatch(value, pattern);
        }
    }
}
