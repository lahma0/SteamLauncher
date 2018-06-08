using System.Text;

namespace SteamLauncher.Tools
{
    public static class CustomExtensions
    {
        /// <summary>
        /// Hackish workaround to get around the fact that the 'UnmanagedFunctionPointer' CharSet decorator does not support CharSet.UTF8.
        /// This function converts a normal string into a UTF-8 byte array which is then turned back into a normal string using 'Default' 
        /// encoding (ANSI). The end result of this is that when the CLR marshals the string, decoding it into a byte array using ANSI, to 
        /// pass it to a native function, it will actually be a valid UTF-8 string that Steam will read correctly rather than an ANSI string 
        /// which will mangle any non-ASCII special characters. Note: Once converted with this method, the .NET string object will appear 
        /// mangled because its being displayed using the wrong encoding.
        /// </summary>
        /// <param name="str">The string to convert to UTF-8.</param>
        /// <returns>A string that when decoded into bytes using ANSI encoding, can be turned back into a valid string using UTF-8 encoding.</returns>
        public static string ToUtf8(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return Encoding.Default.GetString(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// Inserts double quotes around the provided string.
        /// </summary>
        /// <param name="str">The string to put inside double quotation marks.</param>
        /// <returns>Double quoted string if input is not null and length is greater than 0; otherwise, returns an empty string.</returns>
        public static string InDblQuotes(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return "";

            if (str.StartsWith("\"") && str.EndsWith("\""))
                return str;

            return $"\"{str}\"";
        }
    }
}
