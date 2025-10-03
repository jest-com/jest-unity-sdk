using System;
using System.Globalization;

namespace JestSDK
{
    /// <summary>
    /// Provides extension methods for DateTime conversions between C# and JavaScript formats.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts a DateTime to an ISO 8601 formatted string compatible with JavaScript.
        /// </summary>
        /// <param name="dateTime">The DateTime to convert.</param>
        /// <returns>An ISO 8601 formatted string representation of the DateTime.</returns>
        public static string ToJsString(this DateTime dateTime)
        {
            return dateTime.ToString("o"); // Returns ISO 8601 format
        }

        /// <summary>
        /// Converts an ISO 8601 formatted string to a DateTime object.
        /// </summary>
        /// <param name="isoString">The ISO 8601 formatted string to parse.</param>
        /// <returns>A DateTime object representing the parsed string.</returns>
        public static DateTime FromJsString(string isoString)
        {
            return DateTime.Parse(isoString, null, DateTimeStyles.RoundtripKind);
        }
    }
}
