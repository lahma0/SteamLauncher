using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace SteamLauncher.Tools
{
    public static class CustomExtensions
    {
        /// <summary>
        /// Checks if this IEnumerable <paramref name="instance"/> is null or empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The IEnumerable to check.</param>
        /// <returns>True if the <paramref name="instance"/> is null or empty; otherwise, false.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> instance)
        {
            return !(instance?.Any() ?? false);
        }

        /// <summary>
        /// Checks if this list instance contains all items in <paramref name="itemsList"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="itemsList"></param>
        /// <returns>True if this list <paramref name="instance"/> contains all items in <paramref name="itemsList"/>;
        /// otherwise, false.</returns>
        public static bool ContainsAllItems<T>(this IEnumerable<T> instance, IEnumerable<T> itemsList)
        {
            return !itemsList.Except(instance).Any();
        }

        /// <summary>
        /// Encloses this string <paramref name="instance"/> in double quotes.
        /// </summary>
        /// <param name="instance">The string to enclose in double quotation marks.</param>
        /// <param name="autoFixMultipleQuotes">Will ensure the string is enclosed in only 1 set of quotes.</param>
        /// <returns>Double quoted string if input is valid; otherwise, returns an empty string.</returns>
        public static string InDblQuotes(this string instance, bool autoFixMultipleQuotes = true)
        {
            if (string.IsNullOrEmpty(instance))
                return instance;

            return autoFixMultipleQuotes ? $"\"{instance.Trim('"')}\"" : $"\"{instance}\"";
        }

        /// <summary>
        /// Determines whether this string <paramref name="instance"/> starts with any one of the provided chars. If it
        /// does, it returns that char; otherwise it returns 0.
        /// </summary>
        /// <param name="instance">The string instance to check.</param>
        /// <param name="chars">The list/array of chars to check for.</param>
        /// <returns>If found, the provided char that the string starts with; otherwise, returns 0.</returns>
        public static char StartsWith(this string instance, IEnumerable<char> chars)
        {
            return chars.FirstOrDefault(instance.StartsWith);
        }

        /// <summary>
        /// Determines whether this string <paramref name="instance"/> ends with any one of the provided chars. If it
        /// does, it returns that char; otherwise it returns 0.
        /// </summary>
        /// <param name="instance">The string instance to check.</param>
        /// <param name="chars">The list/array of chars to check for.</param>
        /// <returns>If found, the provided char that the string ends with; otherwise, returns 0.</returns>
        public static char EndsWith(this string instance, IEnumerable<char> chars)
        {
            return chars.FirstOrDefault(instance.EndsWith);
        }

        /// <summary>
        /// Removes all starting and trailing instances of single and double quotes from this string
        /// <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">Input string to modify.</param>
        /// <returns>The modified string with all instances of single and double quotes removed from the beginning and
        /// end of this string <paramref name="instance"/>. If no instances are found, <paramref name="instance"/> is
        /// returned unchanged.</returns>
        public static string TrimQuotes(this string instance)
        {
            return instance.Trim('"', '\'');
        }

        /// <summary>
        /// Removes all starting and trailing instances of double quotes from this string <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">Input string to modify.</param>
        /// <returns>The modified string with all instances of double quotes removed from the beginning and end of this
        /// string <paramref name="instance"/>. If no instances are found, <paramref name="instance"/> is returned
        /// unchanged.</returns>
        public static string TrimDblQuotes(this string instance)
        {
            return instance.Trim('"');
        }

        /// <summary>
        /// Removes all starting and trailing instances of single quotes from this string <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">Input string to modify.</param>
        /// <returns>The modified string with all instances of single quotes removed from the beginning and end of this
        /// string <paramref name="instance"/>. If no instances are found, <paramref name="instance"/> is returned
        /// unchanged.</returns>
        public static string TrimSingleQuotes(this string instance)
        {
            return instance.Trim('\'');
        }

        /// <summary>
        /// Removes <paramref name="suffixToRemove"/> from the end of this string <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">Input string to modify.</param>
        /// <param name="suffixToRemove">String to remove.</param>
        /// <param name="comparison">StringComparison type to use.</param>
        /// <returns>Modified <paramref name="instance"/> string if it ended with <paramref name="suffixToRemove"/>;
        /// otherwise, <paramref name="instance"/> is returned unmodified.</returns>
        public static string TrimStrEnd(this string instance,
                                        string suffixToRemove,
                                        StringComparison comparison = StringComparison.Ordinal)
        {
            while (instance.EndsWith(suffixToRemove, comparison))
                instance = instance.Remove(instance.LastIndexOf(suffixToRemove, comparison));

            return instance;
        }

        /// <summary>
        /// Removes <paramref name="prefixToRemove"/> from the beginning of <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">Input string to modify.</param>
        /// <param name="prefixToRemove">String to remove.</param>
        /// <param name="comparison">StringComparison type to use.</param>
        /// <returns>Modified <paramref name="instance"/> string if it began with <paramref name="prefixToRemove"/>;
        /// otherwise, <paramref name="instance"/> is returned unmodified.</returns>
        public static string TrimStrStart(this string instance,
                                          string prefixToRemove,
                                          StringComparison comparison = StringComparison.Ordinal)
        {
            while (instance.StartsWith(prefixToRemove, comparison))
                instance = instance.Remove(0, prefixToRemove.Length);

            return instance;
        }

        /// <summary>
        /// Gets the relative path from the DirectoryInfo/FileInfo object <paramref name="from"/> to this
        /// DirectoryInfo/FileInfo <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">This FileSystemInfo instance to find the relative path to.</param>
        /// <param name="from">The FileSystemInfo object to find the relative path from.</param>
        /// <returns></returns>
        public static string GetRelativePathFrom(this FileSystemInfo instance, FileSystemInfo from)
        {
            return from.GetRelativePathTo(instance);
        }

        /// <summary>
        /// Gets the relative path to DirectoryInfo/FileInfo object <paramref name="to"/> from this
        /// DirectoryInfo/FileInfo <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">This FileSystemInfo instance to find the relative path from.</param>
        /// <param name="to">The FileSystemInfo object to find the relative path to.</param>
        /// <returns></returns>
        public static string GetRelativePathTo(this FileSystemInfo instance, FileSystemInfo to)
        {
            string GetPath(FileSystemInfo fsi)
            {
                return (fsi is DirectoryInfo d) ? d.FullName.TrimEnd('\\') + "\\" : fsi.FullName;
            }

            string fromPath = GetPath(instance);
            string toPath = GetPath(to);

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Returns a list of processes running from the path specified by <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">The executable to look for when enumerating running processes.</param>
        /// <returns>A list of Process instances that have a MainModule path matching <paramref name="instance"/>.</returns>
        public static IEnumerable<Process> GetActiveProcesses(this FileInfo instance)
        {
            return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(instance.Name));
        }

        /// <summary>
        /// A much more compact extension method that duplicates the functionality of string.Equals(string1, string2,
        /// StringComparison.CurrentCultureIgnoreCase). If <paramref name="useOrdinalComparison"/> is true,
        /// 'OrdinalIgnoreCase' is used instead of the default, 'CurrentCultureIgnoreCase'.
        /// </summary>
        /// <param name="instance">The string instance to compare against.</param>
        /// <param name="value2">The string value to compare to this instance.</param>
        /// <param name="useOrdinalComparison">Use Ordinal comparison instead of CurrentCulture.</param>
        /// <returns></returns>
        public static bool EqualsIgnoreCase(this string instance, string value2, bool useOrdinalComparison = false)
        {
            return string.Equals(instance, value2,
                                 useOrdinalComparison
                                     ? StringComparison.OrdinalIgnoreCase
                                     : StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// A much more compact extension method that duplicates the functionality of string1.Contains(string2,
        /// StringComparison.CurrentCultureIgnoreCase). If <paramref name="useOrdinalComparison"/> is true,
        /// 'OrdinalIgnoreCase' is used instead of the default, 'CurrentCultureIgnoreCase'.
        /// </summary>
        /// <param name="instance">This string instance which will be searched.</param>
        /// <param name="containsValue">The value to search for.</param>
        /// <param name="useOrdinalComparison">Use Ordinal comparison instead of CurrentCulture.</param>
        /// <returns></returns>
        public static bool ContainsIgnoreCase(this string instance,
                                              string containsValue,
                                              bool useOrdinalComparison = false)
        {
            return instance.Contains(containsValue,
                                   useOrdinalComparison ? StringComparison.OrdinalIgnoreCase
                                       : StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Serialize class instance to an XML string.
        /// </summary>
        /// <typeparam name="T">Class type.</typeparam>
        /// <param name="value">The class instance.</param>
        /// <returns>A string containing the serialized class instance.</returns>
        public static string Serialize<T>(this T value)
        {
            if (value == null)
                return string.Empty;

            var ns = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            //ns.Add("", "");

            var xmlWriterSettings = new XmlWriterSettings()
            {
                OmitXmlDeclaration = false,
                Indent = true
            };

            using (var ms = new MemoryStream())
            using (var writer = XmlWriter.Create(ms, xmlWriterSettings))
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, value, ns);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// Serialize class instance to a file.
        /// </summary>
        /// <typeparam name="T">Class type.</typeparam>
        /// <param name="value">The class instance.</param>
        /// <param name="file">FileInfo instance describing the file to write to.</param>
        public static void Serialize<T>(this T value, FileInfo file)
        {
            var serializedStr = Serialize<T>(value);

            File.WriteAllText(file.FullName, serializedStr);
        }

        /// <summary>
        /// Deserialize a class instance from an XML string. 
        /// </summary>
        /// <remarks>Makes use of 'XmlCallbackSerializer' which allows for a callback function to be triggered after
        /// deserialization is complete. The deserialized class must be marked as '[Serializable]' and must implement a
        /// function such as 'internal void OnDeserialized(StreamingContext context)'. This function must be marked with
        /// the attribute '[OnDeserialized]'.</remarks>
        /// <typeparam name="T">Class type.</typeparam>
        /// <param name="value">The XML string containing the deserialized class instance.</param>
        /// <returns>A new class instance built from the serialized XML string.</returns>
        public static T Deserialize<T>(this string value)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(value)))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(ms);
            }
        }

        /// <summary>
        /// Deserialize a class instance from an XML file.
        /// </summary>
        /// <remarks>Makes use of 'XmlCallbackSerializer' which allows for a callback function to be triggered after
        /// deserialization is complete. The deserialized class must be marked as '[Serializable]' and must implement a
        /// function such as 'internal void OnDeserialized(StreamingContext context)'. This function must be marked with
        /// the attribute '[OnDeserialized]'.</remarks>
        /// <typeparam name="T">Class type.</typeparam>
        /// <param name="file">FileInfo instance describing the file to read from.</param>
        /// <returns></returns>
        public static T Deserialize<T>(this FileInfo file)
        {
            using (var stream = File.OpenRead(file.FullName))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(stream);
            }
        }

        /// <summary>
        /// Adds the method <see cref="List{T}.AddRange"/> to the <see cref="Collection{T}"/> type and its
        /// derived types (ex: <see cref="ObservableCollection{T}"/>).
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <param name="collection">The <see cref="Collection{T}"/> object to add items to.</param>
        /// <param name="itemsToAddToCollection">The items to add to the <see cref="Collection{T}"/>.</param>
        public static void AddRange<T>(this Collection<T> collection, IEnumerable<T> itemsToAddToCollection)
        {
            foreach (var x in itemsToAddToCollection)
            {
                collection.Add(x);
            }
        }

        /// <summary>
        /// Converts strings in camel-case format to a string with spaces. Ex: 'AddItemTo' -> 'Add Item To'.
        /// </summary>
        /// <param name="str">The camel-case input string.</param>
        /// <returns>A string with camel-casing split by spaces.</returns>
        public static string SplitCamelCase(this string str)
        {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }

        ///// <summary>
        ///// Hackish workaround to get around the fact that the 'UnmanagedFunctionPointer' CharSet decorator does not
        ///// support CharSet.UTF8. This function converts a normal string into a UTF-8 byte array which is then turned
        ///// back into a normal string using 'Default' encoding (ANSI). The end result of this is that when the CLR
        ///// marshals the string, decoding it into a byte array using ANSI, to pass it to a native function, it will
        ///// actually be a valid UTF-8 string that Steam will read correctly rather than an ANSI string which will
        ///// mangle any non-ASCII special characters. Note: Once converted with this method, the .NET string object will
        ///// appear mangled because its being displayed using the wrong encoding.
        ///// </summary>
        ///// <param name="instance">The string to convert to UTF-8.</param>
        ///// <returns>A string that when decoded into bytes using ANSI encoding, can be turned back into a valid string
        ///// using UTF-8 encoding.</returns>
        //public static string ToUtf8(this string instance)
        //{
        //    if (string.IsNullOrEmpty(instance))
        //        return instance;

        //    return Encoding.Default.GetString(Encoding.UTF8.GetBytes(instance));
        //}

        //public static string[] ToUtf8Array(this IEnumerable<string> instance)
        //{
        //    if (instance == null)
        //        return new string[0];

        //    return instance.SkipWhile(string.IsNullOrEmpty).Select(s => s.ToUtf8()).ToArray();
        //}
    }
}
