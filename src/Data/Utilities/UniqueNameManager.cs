using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace DevZest.Data.Utilities
{
    public static class UniqueNameManager
    {
        public static string GetUniqueName(this Dictionary<string, int> suffixes, string name)
        {
            Check.NotNull(suffixes, nameof(suffixes));
            Check.NotEmpty(name, nameof(name));

            int suffix;
            if (suffixes.TryGetValue(name, out suffix))
                suffix++;
            suffixes[name] = suffix;
            string result;
            if (suffix == 0)
                result = name;
            else
            {
                var format = char.IsNumber(name[name.Length - 1]) ? "{0}_{1}" : "{0}{1}";
                result = string.Format(CultureInfo.InvariantCulture, format, name, suffix);
            }
            if (!suffixes.ContainsKey(result))
                suffixes[result] = 0;
            return result;
        }
    }
}
