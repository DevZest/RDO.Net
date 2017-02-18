using DevZest.Data.Windows.Primitives;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevZest.Data.Windows
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

        public static RowBinding<TextBox> TextBox(this _String source, UpdateSourceTrigger updateSourceTrigger)
        {
            var trigger = System.Windows.Controls.TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new RowBinding<TextBox>(onRefresh: (e, r) =>
            {
                e.Text = r.GetValue(source);
            }).WithInput(trigger, source, e => string.IsNullOrEmpty(e.Text) ? null : e.Text);
        }

        public static RowBinding<TextBox> TextBox(this _Int16 source, UpdateSourceTrigger updateSourceTrigger, string inputErrorId = null, string inputErrorDescription = null)
        {
            var flushTrigger = System.Windows.Controls.TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            var inputValidationTrigger = new PropertyChangedTrigger<TextBox>(System.Windows.Controls.TextBox.TextProperty);
            return new RowBinding<TextBox>(onRefresh: (e, r) =>
            {
                e.Text = r.GetValue(source).ToString();
            }).BeginInput(flushTrigger)
            .WithInputValidator(e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return InputError.Empty;
                Int16 result;
                return Int16.TryParse(e.Text, out result) ? InputError.Empty
                : new InputError(inputErrorId, GetInvalidInputErrorMessage(inputErrorDescription, typeof(Int16)));
            }, inputValidationTrigger)
            .WithFlush(source, e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                else
                    return Int16.Parse(e.Text);
            })
            .EndInput();
        }

        public static RowBinding<TextBox> TextBox(this _Int32 source, UpdateSourceTrigger updateSourceTrigger, string inputErrorId = null, string inputErrorDescription = null)
        {
            var flushTrigger = System.Windows.Controls.TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            var inputValidationTrigger = new PropertyChangedTrigger<TextBox>(System.Windows.Controls.TextBox.TextProperty);
            return new RowBinding<TextBox>(onRefresh: (e, r) =>
            {
                e.Text = r.GetValue(source).ToString();
            }).BeginInput(flushTrigger)
            .WithInputValidator(e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return InputError.Empty;
                Int32 result;
                return Int32.TryParse(e.Text, out result) ? InputError.Empty
                : new InputError(inputErrorId, GetInvalidInputErrorMessage(inputErrorDescription, typeof(Int32)));
            }, inputValidationTrigger)
            .WithFlush(source, e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                else
                    return Int32.Parse(e.Text);
            })
            .EndInput();
        }

        public static RowBinding<TextBox> TextBox(this _Int64 source, UpdateSourceTrigger updateSourceTrigger, string inputErrorId = null, string inputErrorDescription = null)
        {
            var flushTrigger = System.Windows.Controls.TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            var inputValidationTrigger = new PropertyChangedTrigger<TextBox>(System.Windows.Controls.TextBox.TextProperty);
            return new RowBinding<TextBox>(onRefresh: (e, r) =>
            {
                e.Text = r.GetValue(source).ToString();
            }).BeginInput(flushTrigger)
            .WithInputValidator(e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return InputError.Empty;
                Int64 result;
                return Int64.TryParse(e.Text, out result) ? InputError.Empty
                : new InputError(inputErrorId, GetInvalidInputErrorMessage(inputErrorDescription, typeof(Int64)));
            }, inputValidationTrigger)
            .WithFlush(source, e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                else
                    return Int64.Parse(e.Text);
            })
            .EndInput();
        }

        public static RowBinding<TextBox> TextBox(this _Single source, UpdateSourceTrigger updateSourceTrigger, string inputErrorId = null, string inputErrorDescription = null)
        {
            var flushTrigger = System.Windows.Controls.TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            var inputValidationTrigger = new PropertyChangedTrigger<TextBox>(System.Windows.Controls.TextBox.TextProperty);
            return new RowBinding<TextBox>(onRefresh: (e, r) =>
            {
                e.Text = r.GetValue(source).ToString();
            }).BeginInput(flushTrigger)
            .WithInputValidator(e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return InputError.Empty;
                Single result;
                return Single.TryParse(e.Text, out result) ? InputError.Empty
                : new InputError(inputErrorId, GetInvalidInputErrorMessage(inputErrorDescription, typeof(Single)));
            }, inputValidationTrigger)
            .WithFlush(source, e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                else
                    return Single.Parse(e.Text);
            })
            .EndInput();
        }

        public static RowBinding<TextBox> TextBox(this _Double source, UpdateSourceTrigger updateSourceTrigger, string inputErrorId = null, string inputErrorDescription = null)
        {
            var flushTrigger = System.Windows.Controls.TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            var inputValidationTrigger = new PropertyChangedTrigger<TextBox>(System.Windows.Controls.TextBox.TextProperty);
            return new RowBinding<TextBox>(onRefresh: (e, r) =>
            {
                e.Text = r.GetValue(source).ToString();
            }).BeginInput(flushTrigger)
            .WithInputValidator(e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return InputError.Empty;
                Double result;
                return Double.TryParse(e.Text, out result) ? InputError.Empty
                : new InputError(inputErrorId, GetInvalidInputErrorMessage(inputErrorDescription, typeof(Double)));
            }, inputValidationTrigger)
            .WithFlush(source, e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                else
                    return Double.Parse(e.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> TextBox(this Scalar<String> source, UpdateSourceTrigger updateSourceTrigger)
        {
            var trigger = System.Windows.Controls.TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value;
            }).WithInput(trigger, source, e => e.Text);
        }

        public static ScalarBinding<TextBox> TextBox(this Scalar<Int16?> source, UpdateSourceTrigger updateSourceTrigger, string inputErrorId = null, string inputErrorDescription = null)
        {
            var flushTrigger = System.Windows.Controls.TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            var inputValidationTrigger = new PropertyChangedTrigger<TextBox>(System.Windows.Controls.TextBox.TextProperty);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
            .WithInputValidator(e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return InputError.Empty;
                Int16 result;
                return Int16.TryParse(e.Text, out result) ? InputError.Empty
                : new InputError(inputErrorId, GetInvalidInputErrorMessage(inputErrorDescription, typeof(Int16)));
            }, inputValidationTrigger)
            .WithFlush(source, e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                else
                    return Int16.Parse(e.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> TextBox(this Scalar<Int32?> source, UpdateSourceTrigger updateSourceTrigger, string inputErrorId = null, string inputErrorDescription = null)
        {
            var flushTrigger = System.Windows.Controls.TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            var inputValidationTrigger = new PropertyChangedTrigger<TextBox>(System.Windows.Controls.TextBox.TextProperty);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
            .WithInputValidator(e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return InputError.Empty;
                Int32 result;
                return Int32.TryParse(e.Text, out result) ? InputError.Empty
                : new InputError(inputErrorId, GetInvalidInputErrorMessage(inputErrorDescription, typeof(Int32)));
            }, inputValidationTrigger)
            .WithFlush(source, e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                else
                    return Int32.Parse(e.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> TextBox(this Scalar<Int64?> source, UpdateSourceTrigger updateSourceTrigger, string inputErrorId = null, string inputErrorDescription = null)
        {
            var flushTrigger = System.Windows.Controls.TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            var inputValidationTrigger = new PropertyChangedTrigger<TextBox>(System.Windows.Controls.TextBox.TextProperty);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
            .WithInputValidator(e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return InputError.Empty;
                Int64 result;
                return Int64.TryParse(e.Text, out result) ? InputError.Empty
                : new InputError(inputErrorId, GetInvalidInputErrorMessage(inputErrorDescription, typeof(Int64)));
            }, inputValidationTrigger)
            .WithFlush(source, e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                else
                    return Int64.Parse(e.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> TextBox(this Scalar<Single?> source, UpdateSourceTrigger updateSourceTrigger, string inputErrorId = null, string inputErrorDescription = null)
        {
            var flushTrigger = System.Windows.Controls.TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            var inputValidationTrigger = new PropertyChangedTrigger<TextBox>(System.Windows.Controls.TextBox.TextProperty);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
            .WithInputValidator(e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return InputError.Empty;
                Single result;
                return Single.TryParse(e.Text, out result) ? InputError.Empty
                : new InputError(inputErrorId, GetInvalidInputErrorMessage(inputErrorDescription, typeof(Single)));
            }, inputValidationTrigger)
            .WithFlush(source, e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                else
                    return Single.Parse(e.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> TextBox(this Scalar<Double?> source, UpdateSourceTrigger updateSourceTrigger, string inputErrorId = null, string inputErrorDescription = null)
        {
            var flushTrigger = System.Windows.Controls.TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            var inputValidationTrigger = new PropertyChangedTrigger<TextBox>(System.Windows.Controls.TextBox.TextProperty);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
            .WithInputValidator(e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return InputError.Empty;
                Double result;
                return Double.TryParse(e.Text, out result) ? InputError.Empty
                : new InputError(inputErrorId, GetInvalidInputErrorMessage(inputErrorDescription, typeof(Double)));
            }, inputValidationTrigger)
            .WithFlush(source, e =>
            {
                if (string.IsNullOrEmpty(e.Text))
                    return null;
                else
                    return Double.Parse(e.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> TextBox(this Scalar<Int16> source, UpdateSourceTrigger updateSourceTrigger, string inputErrorId = null, string inputErrorDescription = null)
        {
            var flushTrigger = System.Windows.Controls.TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            var inputValidationTrigger = new PropertyChangedTrigger<TextBox>(System.Windows.Controls.TextBox.TextProperty);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
            .WithInputValidator(e =>
            {
                Int16 result;
                return Int16.TryParse(e.Text, out result) ? InputError.Empty
                : new InputError(inputErrorId, GetInvalidInputErrorMessage(inputErrorDescription, typeof(Int16)));
            }, inputValidationTrigger)
            .WithFlush(source, e =>
            {
                return Int16.Parse(e.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> TextBox(this Scalar<Int32> source, UpdateSourceTrigger updateSourceTrigger, string inputErrorId = null, string inputErrorDescription = null)
        {
            var flushTrigger = System.Windows.Controls.TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            var inputValidationTrigger = new PropertyChangedTrigger<TextBox>(System.Windows.Controls.TextBox.TextProperty);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
            .WithInputValidator(e =>
            {
                Int32 result;
                return Int32.TryParse(e.Text, out result) ? InputError.Empty
                : new InputError(inputErrorId, GetInvalidInputErrorMessage(inputErrorDescription, typeof(Int32)));
            }, inputValidationTrigger)
            .WithFlush(source, e =>
            {
                return Int32.Parse(e.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> TextBox(this Scalar<Int64> source, UpdateSourceTrigger updateSourceTrigger, string inputErrorId = null, string inputErrorDescription = null)
        {
            var flushTrigger = System.Windows.Controls.TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            var inputValidationTrigger = new PropertyChangedTrigger<TextBox>(System.Windows.Controls.TextBox.TextProperty);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
            .WithInputValidator(e =>
            {
                Int64 result;
                return Int64.TryParse(e.Text, out result) ? InputError.Empty
                : new InputError(inputErrorId, GetInvalidInputErrorMessage(inputErrorDescription, typeof(Int64)));
            }, inputValidationTrigger)
            .WithFlush(source, e =>
            {
                return Int64.Parse(e.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> TextBox(this Scalar<Single> source, UpdateSourceTrigger updateSourceTrigger, string inputErrorId = null, string inputErrorDescription = null)
        {
            var flushTrigger = System.Windows.Controls.TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            var inputValidationTrigger = new PropertyChangedTrigger<TextBox>(System.Windows.Controls.TextBox.TextProperty);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
            .WithInputValidator(e =>
            {
                Single result;
                return Single.TryParse(e.Text, out result) ? InputError.Empty
                : new InputError(inputErrorId, GetInvalidInputErrorMessage(inputErrorDescription, typeof(Single)));
            }, inputValidationTrigger)
            .WithFlush(source, e =>
            {
                return Single.Parse(e.Text);
            })
            .EndInput();
        }

        public static ScalarBinding<TextBox> TextBox(this Scalar<Double> source, UpdateSourceTrigger updateSourceTrigger, string inputErrorId = null, string inputErrorDescription = null)
        {
            var flushTrigger = System.Windows.Controls.TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            var inputValidationTrigger = new PropertyChangedTrigger<TextBox>(System.Windows.Controls.TextBox.TextProperty);
            return new ScalarBinding<TextBox>(onRefresh: e =>
            {
                e.Text = source.Value.ToString();
            }).BeginInput(flushTrigger)
            .WithInputValidator(e =>
            {
                Double result;
                return Double.TryParse(e.Text, out result) ? InputError.Empty
                : new InputError(inputErrorId, GetInvalidInputErrorMessage(inputErrorDescription, typeof(Double)));
            }, inputValidationTrigger)
            .WithFlush(source, e =>
            {
                return Double.Parse(e.Text);
            })
            .EndInput();
        }
    }
}
