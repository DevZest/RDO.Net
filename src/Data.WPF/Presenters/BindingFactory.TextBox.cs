using DevZest.Data.Presenters.Primitives;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevZest.Data.Presenters
{
    partial class BindingFactory
    {
        private static string GetInvalidInputErrorMessage(string value, Type type)
        {
            return string.IsNullOrEmpty(value) ? DiagnosticMessages.BindingFactory_InvalidInput(type) : value;
        }

        /// <summary>
        /// Binds string column to <see cref="TextBox"/>.
        /// </summary>
        /// <param name="source">The source string column.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<TextBox> BindToTextBox(this Column<string> source)
        {
            return new RowBinding<TextBox>(onSetup: (v, p) => v.Setup(), onCleanup: (v, p) => v.Cleanup(), onRefresh: (v, p) =>
            {
                if (!v.GetIsEditing())
                    v.Text = p.GetValue(source);
            }).WithInput(TextBox.TextProperty, TextBox.LostFocusEvent, source, v => string.IsNullOrEmpty(v.Text) ? null : v.Text);
        }

        /// <summary>
        /// Binds nullable <see cref="Int16"/> column to <see cref="TextBox"/>.
        /// </summary>
        /// <param name="source">The source nullable <see cref="Int16"/> column.</param>
        /// <param name="flushErrorMessage">The conversion error message when flushing data from binding to source model.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<TextBox> BindToTextBox(this Column<Int16?> source, string flushErrorMessage = null)
        {
            return new RowBinding<TextBox>(onSetup: (v, p) => v.Setup(), onCleanup: (v, p) => v.Cleanup(), onRefresh: (v, p) =>
            {
                if (!v.GetIsEditing())
                    v.Text = p.GetValue(source).ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushingValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Int16 result;
                return Int16.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorMessage, typeof(Int16));
            })
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Int16.Parse(v.Text);
            })
            .EndInput();
        }

        /// <summary>
        /// Binds nullable <see cref="Int32"/> column to <see cref="TextBox"/>.
        /// </summary>
        /// <param name="source">The source nullable <see cref="Int32"/> column.</param>
        /// <param name="flushErrorMessage">The conversion error message when flushing data from binding to source model.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<TextBox> BindToTextBox(this Column<Int32?> source, string flushErrorMessage = null)
        {
            return new RowBinding<TextBox>(onSetup: (v, p) => v.Setup(), onCleanup: (v, p) => v.Cleanup(), onRefresh: (v, p) =>
            {
                if (!v.GetIsEditing())
                    v.Text = p.GetValue(source).ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushingValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Int32 result;
                return Int32.TryParse(v.Text, out result) ? null
                : GetInvalidInputErrorMessage(flushErrorMessage, typeof(Int32));
            })
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Int32.Parse(v.Text);
            })
            .EndInput();
        }

        /// <summary>
        /// Binds nullable <see cref="Int64"/> column to <see cref="TextBox"/>.
        /// </summary>
        /// <param name="source">The source nullable <see cref="Int64"/> column.</param>
        /// <param name="flushErrorMessage">The conversion error message when flushing data from binding to source model.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<TextBox> BindToTextBox(this Column<Int64?> source, string flushErrorMessage = null)
        {
            return new RowBinding<TextBox>(onSetup: (v, p) => v.Setup(), onCleanup: (v, p) => v.Cleanup(), onRefresh: (v, p) =>
            {
                if (!v.GetIsEditing())
                    v.Text = p.GetValue(source).ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushingValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Int64 result;
                return Int64.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorMessage, typeof(Int64));
            })
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Int64.Parse(v.Text);
            })
            .EndInput();
        }

        /// <summary>
        /// Binds nullable <see cref="Single"/> column to <see cref="TextBox"/>.
        /// </summary>
        /// <param name="source">The source nullable <see cref="Single"/> column.</param>
        /// <param name="flushErrorMessage">The conversion error message when flushing data from binding to source model.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<TextBox> BindToTextBox(this Column<Single?> source, string flushErrorMessage = null)
        {
            return new RowBinding<TextBox>(onSetup: (v, p) => v.Setup(), onCleanup: (v, p) => v.Cleanup(), onRefresh: (v, p) =>
            {
                if (!v.GetIsEditing())
                    v.Text = p.GetValue(source).ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushingValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Single result;
                return Single.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorMessage, typeof(Single));
            })
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Single.Parse(v.Text);
            })
            .EndInput();
        }

        /// <summary>
        /// Binds nullable <see cref="Double"/> column to <see cref="TextBox"/>.
        /// </summary>
        /// <param name="source">The source nullable <see cref="Double"/> column.</param>
        /// <param name="flushErrorMessage">The conversion error message when flushing data from binding to source model.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<TextBox> BindToTextBox(this Column<Double?> source, string flushErrorMessage = null)
        {
            return new RowBinding<TextBox>(onSetup: (v, p) => v.Setup(), onCleanup: (v, p) => v.Cleanup(), onRefresh: (v, p) =>
            {
                if (!v.GetIsEditing())
                    v.Text = p.GetValue(source).ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushingValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Double result;
                return Double.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorMessage, typeof(Double));
            })
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Double.Parse(v.Text);
            })
            .EndInput();
        }

        /// <summary>
        /// Binds nullable <see cref="Decimal"/> column to <see cref="TextBox"/>.
        /// </summary>
        /// <param name="source">The source nullable <see cref="Decimal"/> column.</param>
        /// <param name="flushErrorMessage">The conversion error message when flushing data from binding to source model.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<TextBox> BindToTextBox(this Column<Decimal?> source, string flushErrorMessage = null)
        {
            return new RowBinding<TextBox>(onSetup: (v, p) => v.Setup(), onCleanup: (v, p) => v.Cleanup(), onRefresh: (v, p) =>
            {
                if (!v.GetIsEditing())
                    v.Text = p.GetValue(source).ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushingValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Decimal result;
                return Decimal.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorMessage, typeof(Double));
            })
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Decimal.Parse(v.Text);
            })
            .EndInput();
        }

        /// <summary>
        /// Binds string scalar data to <see cref="TextBox"/>.
        /// </summary>
        /// <param name="source">The source string scalar data.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<String> source)
        {
            return new ScalarBinding<TextBox>(onSetup: v => v.Setup(), onCleanup: v => v.Cleanup(), onRefresh: v =>
            {
                if (!v.GetIsEditing())
                    v.Text = source.GetValue();
            }).WithInput(TextBox.TextProperty, TextBox.LostFocusEvent, source, v => v.Text);
        }

        /// <summary>
        /// Binds nullable <see cref="Int16"/> scalar data to <see cref="TextBox"/>.
        /// </summary>
        /// <param name="source">The source nullabled <see cref="Int16"/> scalar data.</param>
        /// <param name="flushErrorMessage">The conversion error message when flushing data from binding to source model.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<Int16?> source, string flushErrorMessage = null)
        {
            return new ScalarBinding<TextBox>(onSetup: v => v.Setup(), onCleanup: v => v.Cleanup(), onRefresh: v =>
            {
                if (!v.GetIsEditing())
                    v.Text = source.GetValue().ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushingValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Int16 result;
                return Int16.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorMessage, typeof(Int16));
            })
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Int16.Parse(v.Text);
            })
            .EndInput();
        }

        /// <summary>
        /// Binds nullable <see cref="Int32"/> scalar data to <see cref="TextBox"/>.
        /// </summary>
        /// <param name="source">The source nullabled <see cref="Int32"/> scalar data.</param>
        /// <param name="flushErrorMessage">The conversion error message when flushing data from binding to source model.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<Int32?> source, string flushErrorMessage = null)
        {
            return new ScalarBinding<TextBox>(onSetup: v => v.Setup(), onCleanup: v => v.Cleanup(), onRefresh: v =>
            {
                if (!v.GetIsEditing())
                    v.Text = source.GetValue().ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushingValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Int32 result;
                return Int32.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorMessage, typeof(Int32));
            })
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Int32.Parse(v.Text);
            })
            .EndInput();
        }

        /// <summary>
        /// Binds nullable <see cref="Int64"/> scalar data to <see cref="TextBox"/>.
        /// </summary>
        /// <param name="source">The source nullabled <see cref="Int64"/> scalar data.</param>
        /// <param name="flushErrorMessage">The conversion error message when flushing data from binding to source model.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<Int64?> source, string flushErrorMessage = null)
        {
            return new ScalarBinding<TextBox>(onSetup: v => v.Setup(), onCleanup: v => v.Cleanup(), onRefresh: v =>
            {
                if (!v.GetIsEditing())
                    v.Text = source.GetValue().ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushingValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Int64 result;
                return Int64.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorMessage, typeof(Int64));
            })
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Int64.Parse(v.Text);
            })
            .EndInput();
        }

        /// <summary>
        /// Binds nullable <see cref="Single"/> scalar data to <see cref="TextBox"/>.
        /// </summary>
        /// <param name="source">The source nullabled <see cref="Single"/> scalar data.</param>
        /// <param name="flushErrorMessage">The conversion error message when flushing data from binding to source model.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<Single?> source, string flushErrorMessage = null)
        {
            return new ScalarBinding<TextBox>(onSetup: v => v.Setup(), onCleanup: v => v.Cleanup(), onRefresh: v =>
            {
                if (!v.GetIsEditing())
                    v.Text = source.GetValue().ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushingValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Single result;
                return Single.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorMessage, typeof(Single));
            })
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Single.Parse(v.Text);
            })
            .EndInput();
        }

        /// <summary>
        /// Binds nullable <see cref="Double"/> scalar data to <see cref="TextBox"/>.
        /// </summary>
        /// <param name="source">The source nullabled <see cref="Double"/> scalar data.</param>
        /// <param name="flushErrorMessage">The conversion error message when flushing data from binding to source model.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<Double?> source, string flushErrorMessage = null)
        {
            return new ScalarBinding<TextBox>(onSetup: v => v.Setup(), onCleanup: v => v.Cleanup(), onRefresh: v =>
            {
                if (!v.GetIsEditing())
                    v.Text = source.GetValue().ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushingValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Double result;
                return Double.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorMessage, typeof(Double));
            })
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Double.Parse(v.Text);
            })
            .EndInput();
        }

        /// <summary>
        /// Binds <see cref="Int16"/> scalar data to <see cref="TextBox"/>.
        /// </summary>
        /// <param name="source">The source <see cref="Int16"/> scalar data.</param>
        /// <param name="flushErrorMessage">The conversion error message when flushing data from binding to source model.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<Int16> source, string flushErrorMessage = null)
        {
            return new ScalarBinding<TextBox>(onSetup: v => v.Setup(), onCleanup: v => v.Cleanup(), onRefresh: v =>
            {
                if (!v.GetIsEditing())
                    v.Text = source.GetValue().ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushingValidator(v =>
            {
                Int16 result;
                return Int16.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorMessage, typeof(Int16));
            })
            .WithFlush(source, v =>
            {
                return Int16.Parse(v.Text);
            })
            .EndInput();
        }

        /// <summary>
        /// Binds <see cref="Int32"/> scalar data to <see cref="TextBox"/>.
        /// </summary>
        /// <param name="source">The source <see cref="Int32"/> scalar data.</param>
        /// <param name="flushErrorMessage">The conversion error message when flushing data from binding to source model.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<Int32> source, string flushErrorMessage = null)
        {
            return new ScalarBinding<TextBox>(onSetup: v => v.Setup(), onCleanup: v => v.Cleanup(), onRefresh: v =>
            {
                if (!v.GetIsEditing())
                    v.Text = source.GetValue().ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushingValidator(v =>
            {
                Int32 result;
                return Int32.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorMessage, typeof(Int32));
            })
            .WithFlush(source, v =>
            {
                return Int32.Parse(v.Text);
            })
            .EndInput();
        }

        /// <summary>
        /// Binds <see cref="Int64"/> scalar data to <see cref="TextBox"/>.
        /// </summary>
        /// <param name="source">The source <see cref="Int64"/> scalar data.</param>
        /// <param name="flushErrorMessage">The conversion error message when flushing data from binding to source model.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<Int64> source, string flushErrorMessage = null)
        {
            return new ScalarBinding<TextBox>(onSetup: v => v.Setup(), onCleanup: v => v.Cleanup(), onRefresh: v =>
            {
                if (!v.GetIsEditing())
                    v.Text = source.GetValue().ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushingValidator(v =>
            {
                Int64 result;
                return Int64.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorMessage, typeof(Int64));
            })
            .WithFlush(source, v =>
            {
                return Int64.Parse(v.Text);
            })
            .EndInput();
        }

        /// <summary>
        /// Binds <see cref="Single"/> scalar data to <see cref="TextBox"/>.
        /// </summary>
        /// <param name="source">The source <see cref="Single"/> scalar data.</param>
        /// <param name="flushErrorMessage">The conversion error message when flushing data from binding to source model.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<Single> source, string flushErrorMessage = null)
        {
            return new ScalarBinding<TextBox>(onSetup: v => v.Setup(), onCleanup: v => v.Cleanup(), onRefresh: v =>
            {
                if (!v.GetIsEditing())
                    v.Text = source.GetValue().ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushingValidator(v =>
            {
                Single result;
                return Single.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorMessage, typeof(Single));
            })
            .WithFlush(source, v =>
            {
                return Single.Parse(v.Text);
            })
            .EndInput();
        }

        /// <summary>
        /// Binds <see cref="Double"/> scalar data to <see cref="TextBox"/>.
        /// </summary>
        /// <param name="source">The source <see cref="Double"/> scalar data.</param>
        /// <param name="flushErrorMessage">The conversion error message when flushing data from binding to source model.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<Double> source, string flushErrorMessage = null)
        {
            return new ScalarBinding<TextBox>(onSetup: v => v.Setup(), onCleanup: v => v.Cleanup(), onRefresh: v =>
            {
                if (!v.GetIsEditing())
                    v.Text = source.GetValue().ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushingValidator(v =>
            {
                Double result;
                return Double.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorMessage, typeof(Double));
            })
            .WithFlush(source, v =>
            {
                return Double.Parse(v.Text);
            })
            .EndInput();
        }

        /// <summary>
        /// Binds column to <see cref="TextBox"/> with specified value converter.
        /// </summary>
        /// <param name="source">The source column.</param>
        /// <param name="converter">The value converter.</param>
        /// <param name="flushErrorMessage">The conversion error message when flushing data from binding to source model.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<TextBox> BindToTextBox(this Column source, IValueConverter converter, string flushErrorMessage = null)
        {
            return BindToTextBox(source, converter, CultureInfo.CurrentCulture, flushErrorMessage);
        }

        /// <summary>
        /// Binds column to <see cref="TextBox"/> with specified value converter and culture info.
        /// </summary>
        /// <param name="source">The source column.</param>
        /// <param name="converter">The value converter.</param>
        /// <param name="cultureInfo">The culture info.</param>
        /// <param name="flushErrorMessage">The conversion error message when flushing data from binding to source model.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<TextBox> BindToTextBox(this Column source, IValueConverter converter, CultureInfo cultureInfo, string flushErrorMessage = null)
        {
            source.VerifyNotNull(nameof(source));
            converter.VerifyNotNull(nameof(converter));
            cultureInfo.VerifyNotNull(nameof(cultureInfo));

            return new RowBinding<TextBox>(onSetup: (v, p) => v.Setup(), onCleanup: (v, p) => v.Cleanup(), onRefresh: (v, p) =>
            {
                if (!v.GetIsEditing())
                    v.Text = Convert(p[source], converter, cultureInfo);
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushingValidator(v =>
            {
                try
                {
                    var result = converter.ConvertBack(v.Text, source.DataType, null, cultureInfo);
                    if (result == DependencyProperty.UnsetValue)
                        return GetInvalidInputErrorMessage(flushErrorMessage, source.DataType);
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
                return null;
            })
            .WithFlush(source, (p, v) =>
            {
                var value = converter.ConvertBack(v.Text, source.DataType, null, cultureInfo);
                var oldValue = p[source];
                if (Equals(oldValue, value))
                    return false;
                p[source] = value;
                return true;
            })
            .EndInput();
        }

        private static string Convert(object value, IValueConverter converter, CultureInfo cultureInfo)
        {
            Debug.Assert(converter != null);
            try
            {
                var result = converter.Convert(value, typeof(string), null, cultureInfo);
                if (result == DependencyProperty.UnsetValue)
                    result = value?.ToString();
                return result as string;
            }
            catch (Exception)
            {
                return value?.ToString();
            }
        }
    }
}
