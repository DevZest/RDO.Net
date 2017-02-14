using DevZest.Data.Windows.Utilities;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevZest.Data.Windows
{
    public static class BindingFactory
    {
        public static ScalarBinding<ColumnHeader> ColumnHeader(this Column source)
        {
            return new ScalarBinding<ColumnHeader>(
                onRefresh: e =>
                {
                    e.Column = source;
                });
        }

        public static RowBinding<RowHeader> RowHeader(this Model source)
        {
            return new RowBinding<RowHeader>(
                onRefresh: (e, r) =>
                {
                    e.IsCurrent = r.IsCurrent;
                    e.IsSelected = r.IsSelected;
                    e.IsEditing = r.IsEditing;
                });
        }

        public static ScalarBinding<TextBlock> TextBlock<T>(this Scalar<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<TextBlock>(
                onRefresh: e =>
                {
                    e.Text = source.Value.ToString();
                });
        }

        public static ScalarBinding<TextBlock> TextBlock<T>(this Scalar<T> source, string format)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            return new ScalarBinding<TextBlock>(
                onRefresh: e =>
                {
                    e.Text = string.Format(format, source.Value);
                });
        }

        public static ScalarBinding<TextBlock> TextBlock<T>(this Scalar<T> source, IFormatProvider formatProvider, string format)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (formatProvider == null)
                throw new ArgumentNullException(nameof(formatProvider));
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            return new ScalarBinding<TextBlock>(
                onRefresh: e =>
                {
                    e.Text = string.Format(formatProvider, format, source.Value);
                });
        }

        public static RowBinding<TextBlock> TextBlock<T>(this Column<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<TextBlock>(
                onRefresh: (e, r) =>
                {
                    e.Text = r.GetValue(source).ToString();
                });
        }

        public static RowBinding<TextBlock> TextBlock<T>(this Column<T> source, string format)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            return new RowBinding<TextBlock>(
                onRefresh: (e, r) =>
                {
                    e.Text = string.Format(format, r.GetValue(source));
                });
        }

        public static RowBinding<TextBlock> TextBlock<T>(this Column<T> source, IFormatProvider formatProvider, string format)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (formatProvider == null)
                throw new ArgumentNullException(nameof(formatProvider));
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            return new RowBinding<TextBlock>(
                onRefresh: (e, r) =>
                {
                    e.Text = string.Format(formatProvider, format, r.GetValue(source));
                });
        }

        public static RowBinding<TextBox> TextBox(this _String source, UpdateSourceTrigger updateSourceTrigger)
        {
            var trigger = System.Windows.Controls.TextBox.TextProperty.GetUpdateSourceTrigger<TextBox>(updateSourceTrigger);
            return new RowBinding<TextBox>(onRefresh: (e, r) =>
            {
                e.Text = r.GetValue(source);
            }).WithInput(trigger, source, e => e.Text);
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

        private static string GetInvalidInputErrorMessage(string value, Type type)
        {
            return string.IsNullOrEmpty(value) ? Strings.BindingFactory_InvalidInput(type) : value;
        }

        public static RowBinding<ValidationView> ValidationView<T>(this RowInput<T> source)
            where T : UIElement, new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<ValidationView>(
                onRefresh: (e, r) =>
                {
                    e.AsyncValidators = source.AsyncValidators;
                },
                onSetup: (e, r) =>
                {
                    e.Errors = source.GetErrors(r);
                    e.Warnings = source.GetErrors(r);
                    e.RefreshStatus();
                },
                onCleanup: (e, r) =>
                {
                    e.AsyncValidators = AsyncValidatorGroup.Empty;
                    e.Errors = e.Warnings = AbstractValidationMessageGroup.Empty;
                });
        }

        public static RowBinding<ValidationView> ValidationView(this Model source)
        {
            return new RowBinding<ValidationView>(
                onRefresh: (e, r) =>
                {
                    e.AsyncValidators = r.AsyncValidators;
                },
                onSetup: (e, r) =>
                {
                    e.RefreshStatus();
                },
                onCleanup: (e, r) =>
                {
                    e.AsyncValidators = AsyncValidatorGroup.Empty;
                });
        }

        public static ScalarBinding<ValidationView> ValidationView(this DataPresenter source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<ValidationView>(
                onRefresh: e =>
                {
                    e.AsyncValidators = source.AsyncValidators;
                },
                onSetup: e =>
                {
                    e.RefreshStatus();
                },
                onCleanup: e =>
                {
                    e.AsyncValidators = AsyncValidatorGroup.Empty;
                });
        }
    }
}
