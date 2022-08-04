using SteamLauncher.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace SteamLauncher.UI.Utils
{
    public static class EnumHelper
    {
        public static string Description(this Enum value)
        {
            var attributes = value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Any())
                return (attributes.First() as DescriptionAttribute)?.Description;

            // Replace underscores with spaces
            var ti = CultureInfo.CurrentCulture.TextInfo;
            var output = ti.ToTitleCase(ti.ToLower(value.ToString().Replace("_", " ")));

            // Insert spaces between camel casing
            return output.SplitCamelCase();
        }

        public static IEnumerable<Tuple<Enum, string>> GetAllValuesAndDescriptions(Type t)
        {
            if (!t.IsEnum)
                throw new ArgumentException($"{nameof(t)} must be an enum type");

            return Enum.GetValues(t).Cast<Enum>().Select((e) => new Tuple<Enum, string>(e, e.Description())).ToList();
        }
    }
}
