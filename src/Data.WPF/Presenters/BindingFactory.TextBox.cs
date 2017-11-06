using System;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    partial class BindingFactory
    {
        private static string GetInvalidInputErrorMessage(string value, Type type)
        {
            return string.IsNullOrEmpty(value) ? Strings.BindingFactory_InvalidInput(type) : value;
        }

        public static RowBinding<TextBox> AsTextBox(this _String source)
        {
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source);
            }).WithInput(TextBox.TextProperty, source, v => string.IsNullOrEmpty(v.Text) ? null : v.Text);
        }

        public static RowBinding<TextBox> AsTextBox(this _Int16 source, string flushErrorId = null, string flushErrorDescription = null)
        {
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source).ToString();
            }).BeginInput(TextBox.TextProperty)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Int16 result;
                return Int16.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int16));
            }, flushErrorId)
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Int16.Parse(v.Text);
            })
            .EndInput();
        }

        public static RowBinding<TextBox> AsTextBox(this _Int32 source, string flushErrorId = null, string flushErrorDescription = null)
        {
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source).ToString();
            }).BeginInput(TextBox.TextProperty)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Int32 result;
                return Int32.TryParse(v.Text, out result) ? null
                : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int32));
            }, flushErrorId)
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Int32.Parse(v.Text);
            })
            .EndInput();
        }

        public static RowBinding<TextBox> AsTextBox(this _Int64 source, string flushErrorId = null, string flushErrorDescription = null)
        {
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source).ToString();
            }).BeginInput(TextBox.TextProperty)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Int64 result;
                return Int64.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int64));
            }, flushErrorId)
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Int64.Parse(v.Text);
            })
            .EndInput();
        }

        public static RowBinding<TextBox> AsTextBox(this _Single source, string flushErrorId = null, string flushErrorDescription = null)
        {
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source).ToString();
            }).BeginInput(TextBox.TextProperty)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Single result;
                return Single.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Single));
            }, flushErrorId)
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Single.Parse(v.Text);
            })
            .EndInput();
        }

        public static RowBinding<TextBox> AsTextBox(this _Double source, string flushErrorId = null, string flushErrorDescription = null)
        {
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source).ToString();
            }).BeginInput(TextBox.TextProperty)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Double result;
                return Double.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Double));
            }, flushErrorId)
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Double.Parse(v.Text);
            })
            .EndInput();
        }

        public static RowBinding<TextBox> AsTextBox(this _Decimal source, string flushErrorId = null, string flushErrorDescription = null)
        {
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source).ToString();
            }).BeginInput(TextBox.TextProperty)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Decimal result;
                return Decimal.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Double));
            }, flushErrorId)
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Decimal.Parse(v.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<String> source)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value;
            }).WithInput(TextBox.TextProperty, source, v => v.Text);
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Int16?> source, string flushErrorId = null, string flushErrorDescription = null)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(TextBox.TextProperty)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Int16 result;
                return Int16.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int16));
            }, flushErrorDescription)
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Int16.Parse(v.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Int32?> source, string flushErrorId = null, string flushErrorDescription = null)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(TextBox.TextProperty)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Int32 result;
                return Int32.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int32));
            }, flushErrorId)
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Int32.Parse(v.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Int64?> source, string flushErrorId = null, string flushErrorDescription = null)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(TextBox.TextProperty)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Int64 result;
                return Int64.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int64));
            }, flushErrorId)
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Int64.Parse(v.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Single?> source, string flushErrorId = null, string flushErrorDescription = null)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(TextBox.TextProperty)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Single result;
                return Single.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Single));
            }, flushErrorId)
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Single.Parse(v.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Double?> source, string flushErrorId = null, string flushErrorDescription = null)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(TextBox.TextProperty)
            .WithFlushValidator(v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                Double result;
                return Double.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Double));
            }, flushErrorId)
            .WithFlush(source, v =>
            {
                if (string.IsNullOrEmpty(v.Text))
                    return null;
                else
                    return Double.Parse(v.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Int16> source, string flushErrorId = null, string flushErrorDescription = null)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(TextBox.TextProperty)
            .WithFlushValidator(v =>
            {
                Int16 result;
                return Int16.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int16));
            }, flushErrorId)
            .WithFlush(source, v =>
            {
                return Int16.Parse(v.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Int32> source, string flushErrorId = null, string flushErrorDescription = null)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(TextBox.TextProperty)
            .WithFlushValidator(v =>
            {
                Int32 result;
                return Int32.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int32));
            }, flushErrorId)
            .WithFlush(source, v =>
            {
                return Int32.Parse(v.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Int64> source, string flushErrorId = null, string flushErrorDescription = null)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(TextBox.TextProperty)
            .WithFlushValidator(v =>
            {
                Int64 result;
                return Int64.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int64));
            }, flushErrorId)
            .WithFlush(source, v =>
            {
                return Int64.Parse(v.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Single> source, string flushErrorId = null, string flushErrorDescription = null)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(TextBox.TextProperty)
            .WithFlushValidator(v =>
            {
                Single result;
                return Single.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Single));
            }, flushErrorId)
            .WithFlush(source, v =>
            {
                return Single.Parse(v.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Double> source, string flushErrorId = null, string flushErrorDescription = null)
        {
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(TextBox.TextProperty)
            .WithFlushValidator(v =>
            {
                Double result;
                return Double.TryParse(v.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Double));
            }, flushErrorId)
            .WithFlush(source, v =>
            {
                return Double.Parse(v.Text);
            })
            .EndInput();
        }
    }
}
