using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal static class GridLengthParser
    {
        internal struct Result
        {
            public Result(GridLength length, double minLength, double maxLength)
            {
                Length = length;
                MinLength = minLength;
                MaxLength = maxLength;
            }

            public readonly GridLength Length;
            public readonly double MinLength;
            public readonly double MaxLength;
        }

        private static GridLengthConverter s_gridLengthConverter = new GridLengthConverter();

        internal static Result Parse(string value)
        {
            if (value == null || string.IsNullOrEmpty(value))
                throw new FormatException(Strings.GridLengthParser_InvalidInput(value));

            GridLength? length = null;
            double? minLength = null;
            double? maxLength = null;

            var splits = value.Split(';');
            var splitCount = splits.Length;
            if (string.IsNullOrEmpty(splits[splits.Length - 1].Trim()))
                splitCount--;

            if (splitCount < 1 || splitCount > 3)
                throw new FormatException(Strings.GridLengthParser_InvalidInput(value));

            for (int i = 0; i < splitCount; i++)
            {
                var result = ParseNameGridLengthPair(splits[i]);
                if (!result.HasValue)
                    throw new FormatException(Strings.GridLengthParser_InvalidInput(value));

                var pair = result.GetValueOrDefault();
                var name = pair.Name;
                var gridLength = pair.GridLength;
                if (string.IsNullOrEmpty(name))
                {
                    if (length.HasValue)
                        throw new FormatException(Strings.GridLengthParser_InvalidInput(value));
                    length = gridLength;
                }
                else if (name == "min")
                {
                    if (minLength.HasValue)
                        throw new FormatException(Strings.GridLengthParser_InvalidInput(value));
                    minLength = gridLength.Value;
                }
                else
                {
                    Debug.Assert(name == "max");
                    if (maxLength.HasValue)
                        throw new FormatException(Strings.GridLengthParser_InvalidInput(value));
                    maxLength = gridLength.Value;
                }
            }

            if (!length.HasValue)
                throw new FormatException(Strings.GridLengthParser_InvalidInput(value));
            double min = minLength.HasValue ? minLength.GetValueOrDefault() : 0.0;
            double max = maxLength.HasValue ? maxLength.GetValueOrDefault() : double.PositiveInfinity;
            if (min > max)
                throw new FormatException(Strings.GridLengthParser_InvalidInput(value));
            return new Result(length.GetValueOrDefault(), min, max);
        }

        private struct NameGridLengthPair
        {
            public NameGridLengthPair(string name, GridLength gridLength)
            {
                Name = name;
                GridLength = gridLength;
            }

            public readonly string Name;
            public readonly GridLength GridLength;
        }

        private static NameGridLengthPair? ParseNameGridLengthPair(string value)
        {
            Debug.Assert(value != null);
            if (string.IsNullOrEmpty(value.Trim()))
                return null;

            var splits = value.Split(':');
            if (splits.Length < 1 || splits.Length > 2)
                return null;

            string name;
            GridLength gridLength;

            if (splits.Length == 1)
                name = string.Empty;
            else
            {
                name = splits[0].Trim().ToLowerInvariant();
                if (name != "min" && name != "max")
                    return null;
            }

            try
            {
                gridLength = (GridLength)s_gridLengthConverter.ConvertFromInvariantString(splits[splits.Length - 1]);
            }
            catch (FormatException)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(name) && !gridLength.IsAbsolute)
                return null;
            return new NameGridLengthPair(name, gridLength);
        }
    }
}
