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
            return new RowBinding<TextBox>(onRefresh: (e, r) =>
            {
                e.Text = r.GetValue(source);
            }).WithInput(flushTrigger, source, e => string.IsNullOrEmpty(e.Text) ? null : e.Text);
        }

        public static RowBinding<TextBox> AsTextBox(this _Int16 source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new RowBinding<TextBox>(onRefresh: (e, r) =>
            {
                e.Text = r.GetValue(source).ToString();
            }).BeginInput(flushTrigger)
            .WithFlushValidator(e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                Int16 result;
                return Int16.TryParse(e.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int16));
            }, flushErrorId)
            .WithFlush(source, e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                else
                    return Int16.Parse(e.Text);
            })
            .EndInput();
        }

        public static RowBinding<TextBox> AsTextBox(this _Int32 source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new RowBinding<TextBox>(onRefresh: (e, r) =>
            {
                e.Text = r.GetValue(source).ToString();
            }).BeginInput(flushTrigger)
            .WithFlushValidator(e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                Int32 result;
                return Int32.TryParse(e.Text, out result) ? null
                : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int32));
            }, flushErrorId)
            .WithFlush(source, e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                else
                    return Int32.Parse(e.Text);
            })
            .EndInput();
        }

        public static RowBinding<TextBox> AsTextBox(this _Int64 source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new RowBinding<TextBox>(onRefresh: (e, r) =>
            {
                e.Text = r.GetValue(source).ToString();
            }).BeginInput(flushTrigger)
            .WithFlushValidator(e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                Int64 result;
                return Int64.TryParse(e.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int64));
            }, flushErrorId)
            .WithFlush(source, e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                else
                    return Int64.Parse(e.Text);
            })
            .EndInput();
        }

        public static RowBinding<TextBox> AsTextBox(this _Single source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new RowBinding<TextBox>(onRefresh: (e, r) =>
            {
                e.Text = r.GetValue(source).ToString();
            }).BeginInput(flushTrigger)
            .WithFlushValidator(e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                Single result;
                return Single.TryParse(e.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Single));
            }, flushErrorId)
            .WithFlush(source, e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                else
                    return Single.Parse(e.Text);
            })
            .EndInput();
        }

        public static RowBinding<TextBox> AsTextBox(this _Double source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new RowBinding<TextBox>(onRefresh: (e, r) =>
            {
                e.Text = r.GetValue(source).ToString();
            }).BeginInput(flushTrigger)
            .WithFlushValidator(e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                Double result;
                return Double.TryParse(e.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Double));
            }, flushErrorId)
            .WithFlush(source, e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                else
                    return Double.Parse(e.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<String> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged)
        {
            var trigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value;
            }).WithInput(trigger, source, e => e.Text);
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Int16?> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
            .WithFlushValidator(e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                Int16 result;
                return Int16.TryParse(e.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int16));
            }, flushErrorDescription)
            .WithFlush(source, e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                else
                    return Int16.Parse(e.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Int32?> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
            .WithFlushValidator(e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                Int32 result;
                return Int32.TryParse(e.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int32));
            }, flushErrorId)
            .WithFlush(source, e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                else
                    return Int32.Parse(e.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Int64?> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
            .WithFlushValidator(e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                Int64 result;
                return Int64.TryParse(e.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int64));
            }, flushErrorId)
            .WithFlush(source, e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                else
                    return Int64.Parse(e.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Single?> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
            .WithFlushValidator(e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                Single result;
                return Single.TryParse(e.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Single));
            }, flushErrorId)
            .WithFlush(source, e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                else
                    return Single.Parse(e.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Double?> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
            .WithFlushValidator(e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                Double result;
                return Double.TryParse(e.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Double));
            }, flushErrorId)
            .WithFlush(source, e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                else
                    return Double.Parse(e.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Int16> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
            .WithFlushValidator(e =>
            {
                Int16 result;
                return Int16.TryParse(e.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int16));
            }, flushErrorId)
            .WithFlush(source, e =>
            {
                return Int16.Parse(e.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Int32> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
            .WithFlushValidator(e =>
            {
                Int32 result;
                return Int32.TryParse(e.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int32));
            }, flushErrorId)
            .WithFlush(source, e =>
            {
                return Int32.Parse(e.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Int64> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
            .WithFlushValidator(e =>
            {
                Int64 result;
                return Int64.TryParse(e.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int64));
            }, flushErrorId)
            .WithFlush(source, e =>
            {
                return Int64.Parse(e.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Single> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
            .WithFlushValidator(e =>
            {
                Single result;
                return Single.TryParse(e.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Single));
            }, flushErrorId)
            .WithFlush(source, e =>
            {
                return Single.Parse(e.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> AsTextBox(this Scalar<Double> source, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            string flushErrorId = null, string flushErrorDescription = null)
        {
            var flushTrigger = TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
            .WithFlushValidator(e =>
            {
                Double result;
                return Double.TryParse(e.Text, out result) ? null : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Double));
            }, flushErrorId)
            .WithFlush(source, e =>
            {
                return Double.Parse(e.Text);
            })
            .EndInput();
        }
    }
}
