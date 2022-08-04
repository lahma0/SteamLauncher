using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SteamLauncher.Tools
{
    public static class DateTimeHelper
    {
        public static string GetIso8601UtcTimestamp()
        {
            return DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture);
        }

        public static DateTime Iso8601UtcTimestampToLocalDateTime(string iso8601String)
        {
            return DateTime.ParseExact(iso8601String, "s", CultureInfo.InvariantCulture).ToLocalTime();
        }

        public static string TimeElapsedSinceNowString(DateTime sinceDateTime)
        {
            var timeSpan = DateTime.Now.Subtract(sinceDateTime);
            return $"{timeSpan.Days}d {timeSpan.Hours}h {timeSpan.Minutes}m";
        }

        /// <summary>
        /// Converts a Unix timestamp in seconds to a local DateTime object.
        /// </summary>
        /// <param name="unixTimeSeconds">A Unix timestamp in seconds.</param>
        /// <returns>A DateTime object representing the Unix timestamp in local time.</returns>
        public static DateTime ConvertUnixTimeSecondsToLocalDateTime(long unixTimeSeconds)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).LocalDateTime;
        }

        /// <summary>
        /// Converts a Unix timestamp string in seconds to a local DateTime object.
        /// </summary>
        /// <param name="unixTimeSeconds">A Unix timestamp string in seconds.</param>
        /// <returns>A DateTime object representing the Unix timestamp in local time.</returns>
        public static DateTime ConvertUnixTimeSecondsToLocalDateTime(string unixTimeSeconds)
        {
            bool success = long.TryParse(unixTimeSeconds, out var result);
            if (!success)
                throw new ArgumentException("The provided Unix timestamp string could not be parsed as a number.", nameof(unixTimeSeconds));

            return DateTimeOffset.FromUnixTimeSeconds(result).LocalDateTime;
        }

        /// <summary>
        /// Gets the current Unix timestamp in seconds as a 32-bit int. Note: Unix timestamps are always UTC.
        /// </summary>
        /// <returns>A 32-bit int representing the current Unix timestamps in seconds.</returns>
        public static int GetCurrentUnixTimeSecondsAsInt()
        {
            // Unix timestamps are always  UTC so 'DateTimeOffset.[Now|UtcNow].ToUnixTimeSeconds()' are equivalent.
            return Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds());
        }

        /// <summary>
        /// Gets the current Unix timestamp in seconds as a 64-bit long. Note: Unix timestamps are always UTC.
        /// </summary>
        /// <returns>A 64-bit long representing the current Unix timestamps in seconds.</returns>
        public static long GetCurrentUnixTimeSeconds()
        {
            // Unix timestamps are always  UTC so 'DateTimeOffset.[Now|UtcNow].ToUnixTimeSeconds()' are equivalent.
            return DateTimeOffset.Now.ToUnixTimeSeconds();
        }
    }
}
