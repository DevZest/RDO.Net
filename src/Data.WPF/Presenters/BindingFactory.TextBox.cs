using DevZest.Data.Presenters.Primitives;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevZest.Data.Presenters
{
    partial class BindingFactory
    {
        private static string GetInvalidInputErrorMessage(string value, Type type)
        {
            return string.IsNullOrEmpty(value) ? Strings.BindingFactory_InvalidInput(type) : value;
        }

        private static UpdateSourceTrigger GetDefaultUpdateSourceTrigger(this DependencyProperty property, Type type)
        {
            var metaData = property.GetMetadata(type) as FrameworkPropertyMetadata;
            return metaData == null ? UpdateSourceTrigger.PropertyChanged : metaData.DefaultUpdateSourceTrigger;
        }

        private static Trigger<T> GetUpdateSourceTrigger<T>(this DependencyProperty property, UpdateSourceTrigger updateSourceTrigger)
            where T : UIElement, new()
        {
            switch (updateSourceTrigger)
            {
                case UpdateSourceTrigger.Default:
                    return property.GetUpdateSourceTrigger<T>(property.GetDefaultUpdateSourceTrigger(typeof(T)));
                case UpdateSourceTrigger.Explicit:
                    return new ExplicitTrigger<T>();
                case UpdateSourceTrigger.LostFocus:
                    return new LostFocusTrigger<T>();
                case UpdateSourceTrigger.PropertyChanged:
                    return new PropertyChangedTrigger<T>(property);
            }
            return null;
        }

        public static RowBinding<TextBox> AsTextBox(this _String source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source);
            }).WithInput(flushTrigger, source, v => string.IsNullOrEmpty(v.Text) ? null : v.Text);
        }

        public static RowBinding<TextBox> AsTextBox(this _Int16 source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source).ToString();
            }).BeginInput(flushTrigger)
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

        public static RowBinding<TextBox> AsTextBox(this _Int32 source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source).ToString();
            }).BeginInput(flushTrigger)
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

        public static RowBinding<TextBox> AsTextBox(this _Int64 source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source).ToString();
            }).BeginInput(flushTrigger)
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

        public static RowBinding<TextBox> AsTextBox(this _Single source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source).ToString();
            }).BeginInput(flushTrigger)
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

        public static RowBinding<TextBox> AsTextBox(this _Double source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source).ToString();
            }).BeginInput(flushTrigger)
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

        public static RowBinding<TextBox> AsTextBox(this _Decimal source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new RowBinding<TextBox>(onRefresh: (v, p) =>
            {
                v.Text = p.GetValue(source).ToString();
            }).BeginInput(flushTrigger)
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

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<String> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged)
        {
            var trigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value;
            }).WithInput(trigger, source, v => v.Text);
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Int16?> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
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

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Int32?> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
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

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Int64?> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
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

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Single?> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
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

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Double?> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
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

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Int16> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
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

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Int32> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
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

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Int64> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
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

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Single> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
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

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Double> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: v =>
            {
                v.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
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
