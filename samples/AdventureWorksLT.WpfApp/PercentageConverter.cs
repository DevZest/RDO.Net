using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DevZest.Samples.AdventureWorksLT
{
    public sealed class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == null || targetType != typeof(string))
                return DependencyProperty.UnsetValue;

            if (value == null)
                return null;

            if (value is decimal?)
                return string.Format("{0:P}", value);

            return DependencyProperty.UnsetValue;
        }

        private bool TryConvertBack(string value, CultureInfo culture, out decimal? result)
        {
            result = new decimal?();
            if (string.IsNullOrEmpty(value))
                return true;

            if (TryConvertBack(value, culture, out decimal decimalResult))
            {
                result = decimalResult;
                return true;
            }

            return false;
        }

        private bool TryConvertBack(string value, CultureInfo culture, out decimal result)
        {
            result = default(decimal);
            if (string.IsNullOrEmpty(value))
                return false;

            try
            {
                var text = value.Trim();
                if (!culture.IsNeutralCulture && text.Length > 0 && culture.NumberFormat != null)
                {
                    switch (culture.NumberFormat.PercentPositivePattern)
                    {
                        case 0:
                        case 1:
                            if (text.Length - 1 == text.LastIndexOf(culture.NumberFormat.PercentSymbol, StringComparison.CurrentCultureIgnoreCase))
                                text = text.Substring(0, text.Length - 1);
                            break;
                        case 2:
                            if (text.IndexOf(culture.NumberFormat.PercentSymbol, StringComparison.CurrentCultureIgnoreCase) == 0)
                                text = text.Substring(1);
                            break;
                    }
                }
                result = System.Convert.ToDecimal(text, culture);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            catch (ArgumentNullException)
            {
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text;
            if (value == null)
                text = null;
            else if (value is string)
                text = (string)value;
            else
                return DependencyProperty.UnsetValue;

            if (targetType == typeof(decimal?))
                return TryConvertBack(text, culture, out decimal? result) ? result / 100 : DependencyProperty.UnsetValue;
            else
                return DependencyProperty.UnsetValue;
        }
    }
}
