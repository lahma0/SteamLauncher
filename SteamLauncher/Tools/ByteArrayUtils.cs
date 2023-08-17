using System;
using System.Text;

namespace SteamLauncher.Tools
{
    public static class ByteArrayUtils
    {
        /// <summary>
        /// Tests if <paramref name="self"/> contains the string <paramref name="matchString"/>.
        /// </summary>
        /// <param name="self">The byte array that is to be searched.</param>
        /// <param name="matchString">The string to search for.</param>
        /// <param name="stringEncoding">The encoding of <paramref name="matchString"/>.</param>
        /// <returns>True if the string is found; otherwise, false.</returns>
        public static bool Contains(this byte[] self, string matchString, Encoding stringEncoding = null)
        {
            if (stringEncoding == null)
                stringEncoding = Encoding.UTF8;

            return CountOccurrences(self, matchString, stringEncoding) > 0;
        }

        /// <summary>
        /// Find the index of the 1st occurrence of <paramref name="matchString"/> in <paramref name="self"/>. 
        /// Returns -1 if no occurrence is found.
        /// </summary>
        /// <param name="self">The byte array that is to be searched.</param>
        /// <param name="matchString">The string to search for.</param>
        /// <param name="stringEncoding">The encoding of <paramref name="matchString"/> (used to convert it to 
        /// bytes; these bytes are what is actually searched for).</param>
        /// <returns>Index of 1st match or -1 if no match is found.</returns>
        public static int IndexOf(this byte[] self, string matchString, Encoding stringEncoding = null)
        {
            if (stringEncoding == null)
                stringEncoding = Encoding.UTF8;

            return IndexOf(self, stringEncoding.GetBytes(matchString));
        }

        /// <summary>
        /// Find the index of the 1st occurrence of <paramref name="matchBytes"/> in <paramref name="self"/>.
        /// Return -1 if no occurrence is found.
        /// </summary>
        /// <param name="self">The byte array that is to be searched.</param>
        /// <param name="matchBytes">The byte array to search for.</param>
        /// <returns>Index of 1st match or -1 if no match is found.</returns>
        public static int IndexOf(this byte[] self, byte[] matchBytes)
        {
            if (self.Length < matchBytes.Length)
                return -1;

            var y = 0;
            while (y < self.Length)
            {
                if (!IsMatch(self, y, matchBytes))
                {
                    y += 1;
                    continue;
                }

                return y;
            }

            return -1;
        }

        /// <summary>
        /// Tests if <paramref name="self"/> contains the byte sequence <paramref name="matchBytes"/>.
        /// </summary>
        /// <param name="self">The byte array that is to be searched.</param>
        /// <param name="matchBytes">The byte array to search for.</param>
        /// <returns>True if the byte array is found; otherwise, false.</returns>
        public static bool Contains(this byte[] self, byte[] matchBytes)
        {
            return CountOccurrences(self, matchBytes) > 0;
        }

        /// <summary>
        /// Counts the number of times <paramref name="matchString"/> occurs within <paramref name="self"/>.
        /// </summary>
        /// <param name="self">The byte array that is to be searched.</param>
        /// <param name="matchString">The string to search for.</param>
        /// <param name="stringEncoding">The encoding of <paramref name="matchString"/>.</param>
        /// <param name="allowOverlapping">Defines if overlapping occurrences should be counted. If true, the string 
        /// 'CC' would count as being found twice within the string 'A-BB-CCC-DDDD'.</param>
        /// <returns>The number of instances found.</returns>
        public static int CountOccurrences(this byte[] self,
                                           string matchString,
                                           Encoding stringEncoding = null,
                                           bool allowOverlapping = false)
        {
            if (stringEncoding == null)
                stringEncoding = Encoding.UTF8;

            return CountOccurrences(self, stringEncoding.GetBytes(matchString), allowOverlapping);
        }

        /// <summary>
        /// Counts the number of times <paramref name="matchBytes"/> occurs within <paramref name="self"/>.
        /// </summary>
        /// <param name="self">The byte array that is to be searched.</param>
        /// <param name="matchBytes">The byte array to search for.</param>
        /// <param name="allowOverlapping">Defines if overlapping occurrences should be counted. If true, the bytes 
        /// '[0x11, 0x11]' would count as being found twice within the bytes '[0x00, 0x11, 0x11, 0x11, 0x00]'.</param>
        /// <returns>The number of instances found.</returns>
        public static int CountOccurrences(this byte[] self, byte[] matchBytes, bool allowOverlapping = false)
        {
            if (self.Length < matchBytes.Length)
                return 0;

            var count = 0;

            if (allowOverlapping)
            {
                for (var i = 0; i < self.Length; i++)
                {
                    if (!IsMatch(self, i, matchBytes))
                        continue;

                    count += 1;
                }

                return count;
            }

            var y = 0;
            while (y < self.Length)
            {
                if (!IsMatch(self, y, matchBytes))
                {
                    y += 1;
                    continue;
                }

                count += 1;
                y += matchBytes.Length;
            }

            return count;
        }

        /// <summary>
        /// Checks if <paramref name="array"/> contains <paramref name="matchBytes"/> at <paramref name="position"/>.
        /// </summary>
        /// <param name="array">The byte array to look for the value within.</param>
        /// <param name="position">The index within the array to look for <paramref name="matchBytes"/>.</param>
        /// <param name="matchBytes">The byte array to search for.</param>
        /// <returns>True if the bytes were found; otherwise, false.</returns>
        private static bool IsMatch(byte[] array, int position, byte[] matchBytes)
        {
            if ((array.Length - position) < matchBytes.Length)
                return false;

            for (var i = 0; i < matchBytes.Length; i++)
            {
                if (array[position + i] != matchBytes[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Converts byte array to hex string.
        /// </summary>
        /// <param name="data">Byte array to convert to hex string.</param>
        /// <returns>Byte array represented as a hex string.</returns>
        public static string ToHex(this byte[] data)
        {
            return ToHex(data, "", "");
        }

        /// <summary>
        /// /// Converts byte array to hex string with the given prefix and an instance of separator between each byte.
        /// </summary>
        /// <param name="data">Byte array to convert to hex string.</param>
        /// <param name="prefix">A string to prefix the hex string with.</param>
        /// <param name="separator">A string to use as a separator between each byte.</param>
        /// <returns>Byte array represented as a hex string.</returns>
        public static string ToHex(this byte[] data, string prefix, string separator)
        {
            char[] lookup = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
            int i = 0, p = prefix.Length, l = data.Length;
            int s = (data.Length - 1) * separator.Length;
            char[] c = new char[l * 2 + p + s];
            byte d;
            for (; i < p; ++i) c[i] = prefix[i];
            i = -1;
            --l;
            --p;
            while (i < l)
            {
                d = data[++i];
                c[++p] = lookup[d >> 4];
                c[++p] = lookup[d & 0xF];
                if (i < l)
                    for (int y = 0; y < separator.Length; y++)
                        c[++p] = separator[y];
            }
            string output = new string(c, 0, c.Length);
            return output;
        }

        /// <summary>
        /// Converts hex string to byte array.
        /// </summary>
        /// <param name="str">Hex string representing a sequence of bytes to convert to a byte array.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] FromHex(this string str)
        {
            return FromHex(str, 0, 0, 0);
        }

        /// <summary>
        /// Converts hex string to byte array.
        /// </summary>
        /// <param name="str">Hex string representing a sequence of bytes to convert to a byte array.</param>
        /// <param name="offset">Offset to begin converting from.</param>
        /// <param name="step">Skip this many characters on every iteration (allows for ignoring a hex 
        /// string where each byte is separated by a space or some other characters).</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] FromHex(this string str, int offset, int step)
        {
            return FromHex(str, offset, step, 0);
        }

        /// <summary>
        /// Converts hex string to byte array.
        /// </summary>
        /// <param name="str">Hex string representing a sequence of bytes to convert to a byte array.</param>
        /// <param name="offset">Offset to begin converting from.</param>
        /// <param name="step">Skip this many characters on every iteration (allows for ignoring a hex 
        /// string where each byte is separated by a space or some other characters).</param>
        /// <param name="tail">Set to the number of characters each byte has attached to it that you would 
        /// like to ignore. For example, if every byte has a suffix of 'h' (41h 42h 43h 44h), you would set 
        /// this value to 1.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] FromHex(this string str, int offset, int step, int tail)
        {
            byte[] b = new byte[(str.Length - offset - tail + step) / (2 + step)];
            byte c1, c2;
            int l = str.Length - tail;
            int s = step + 1;
            for (int y = 0, x = offset; x < l; ++y, x += s)
            {
                c1 = (byte)str[x];
                if (c1 > 0x60) c1 -= 0x57;
                else if (c1 > 0x40) c1 -= 0x37;
                else c1 -= 0x30;
                c2 = (byte)str[++x];
                if (c2 > 0x60) c2 -= 0x57;
                else if (c2 > 0x40) c2 -= 0x37;
                else c2 -= 0x30;
                b[y] = (byte)((c1 << 4) + c2);
            }
            return b;
        }

        /// <summary>
        /// Finds the last index of a pattern in a byte array.
        /// </summary>
        /// <param name="data">The byte array to search for the pattern in.</param>
        /// <param name="pattern">The pattern to look for in the byte array.</param>
        /// <returns>The index of the last instance of the pattern found; if the pattern is not found, -1.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static long LastIndexOf(this byte[] data, byte[] pattern)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            if (pattern.LongLength > data.LongLength) return -1;

            var cycles = data.LongLength - pattern.LongLength + 1;
            for (var dataIndex = cycles; dataIndex > 0; dataIndex--)
            {
                if (data[dataIndex] != pattern[0]) continue;
                long patternIndex;
                for (patternIndex = pattern.Length - 1; patternIndex >= 1; patternIndex--) if (data[dataIndex + patternIndex] != pattern[patternIndex]) break;
                if (patternIndex == 0) return dataIndex;
            }
            return -1;
        }

        /// <summary>
        /// Finds the last index of a pattern in a byte array.
        /// </summary>
        /// <param name="data">The byte array to search for the pattern in.</param>
        /// <param name="pattern">The pattern to look for in the byte array.</param>
        /// <param name="startIndex">The index to start the search at.</param>
        /// <returns>The index of the first instance of the pattern found; if the pattern is not found, -1.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static long IndexOf(this byte[] data, byte[] pattern, long startIndex)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            if (pattern.LongLength > data.LongLength) return -1;

            var cycles = data.LongLength - pattern.LongLength + 1;
            for (var dataIndex = startIndex; dataIndex < cycles; dataIndex++)
            {
                if (data[dataIndex] != pattern[0]) continue;
                long patternIndex;
                for (patternIndex = pattern.Length - 1; patternIndex >= 1; patternIndex--) if (data[dataIndex + patternIndex] != pattern[patternIndex]) break;
                if (patternIndex == 0) return dataIndex;
            }
            return -1;
        }
    }
}
