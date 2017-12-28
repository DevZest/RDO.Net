using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    partial class BindingFactory
    {
        private static string GetInvalidInputErrorMessage(string value, Type type)
        {
            return string.IsNullOrEmpty(value) ? DiagnosticMessages.BindingFactory_InvalidInput(type) : value;
        }

        public static RowBinding<TextBox> BindToTextBox(this Column<string> source)
        {
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source);
            }).WithInput(TextBox.TextProperty, TextBox.LostFocusEvent, source, v => string.IsNullOrEmpty(v.Text) ? null : v.Text);
        }

        public static RowBinding<TextBox> BindToTextBox(this Column<Int16?> source, string flushErrorDescription = null)
        {
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source).ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Int16 result;
                return Int16.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int16));
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

        public static RowBinding<TextBox> BindToTextBox(this Column<Int32?> source, string flushErrorDescription = null)
        {
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source).ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Int32 result;
                return Int32.TryParse(v.Text, out result) ? null
                : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int32));
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

        public static RowBinding<TextBox> BindToTextBox(this Column<Int64?> source, string flushErrorDescription = null)
        {
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source).ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Int64 result;
                return Int64.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int64));
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

        public static RowBinding<TextBox> BindToTextBox(this Column<Single?> source, string flushErrorDescription = null)
        {
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source).ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Single result;
                return Single.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Single));
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

        public static RowBinding<TextBox> BindToTextBox(this Column<Double?> source, string flushErrorDescription = null)
        {
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source).ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Double result;
                return Double.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Double));
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

        public static RowBinding<TextBox> BindToTextBox(this Column<Decimal?> source, string flushErrorDescription = null)
        {
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source).ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Decimal result;
                return Decimal.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Double));
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

        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<String> source)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value;
            }).WithInput(TextBox.TextProperty, TextBox.LostFocusEvent, source, v => v.Text);
        }

        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<Int16?> source, string flushErrorDescription = null)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Int16 result;
                return Int16.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int16));
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

        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<Int32?> source, string flushErrorDescription = null)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Int32 result;
                return Int32.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int32));
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

        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<Int64?> source, string flushErrorDescription = null)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Int64 result;
                return Int64.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int64));
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

        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<Single?> source, string flushErrorDescription = null)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Single result;
                return Single.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Single));
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

        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<Double?> source, string flushErrorDescription = null)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Double result;
                return Double.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Double));
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

        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<Int16> source, string flushErrorDescription = null)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushValidator(v =>
            {
                Int16 result;
                return Int16.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int16));
            })
            .WithFlush(source, v =>
            {
                return Int16.Parse(v.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<Int32> source, string flushErrorDescription = null)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushValidator(v =>
            {
                Int32 result;
                return Int32.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int32));
            })
            .WithFlush(source, v =>
            {
                return Int32.Parse(v.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<Int64> source, string flushErrorDescription = null)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushValidator(v =>
            {
                Int64 result;
                return Int64.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int64));
            })
            .WithFlush(source, v =>
            {
                return Int64.Parse(v.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<Single> source, string flushErrorDescription = null)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushValidator(v =>
            {
                Single result;
                return Single.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Single));
            })
            .WithFlush(source, v =>
            {
                return Single.Parse(v.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> BindToTextBox(this Scalar<Double> source, string flushErrorDescription = null)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
            .WithFlushValidator(v =>
            {
                Double result;
                return Double.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Double));
            })
            .WithFlush(source, v =>
            {
                return Double.Parse(v.Text);
            })
            .EndInput();
        }
    }
}
