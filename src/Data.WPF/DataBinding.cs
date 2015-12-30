using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;

namespace DevZest.Data.Windows
{
    internal abstract class DataBinding
    {
        internal static DataBinding Create<T>(Type type, DependencyProperty property, Column<T> column)
        {
            return new ColumnBinding<T>(type, property, column, null);
        }

        internal static DataBinding CreateTwoWay<T>(Type type, DependencyProperty property, Column<T> column, UpdateSourceTrigger updateSourceTrigger)
        {
            return new ColumnBinding<T>(type, property, column, updateSourceTrigger);
        }

        internal static DataBinding Create<T>(Type type, DependencyProperty property, Func<DataRowPresenter, T> valueGetter)
        {
            return new ExpressionBinding<T>(type, property, valueGetter, null, null);
        }

        internal static DataBinding CreateTwoWay<T>(Type type, DependencyProperty property, Func<DataRowPresenter, T> valueGetter,
            Action<DataRowPresenter, T> valueSetter, UpdateSourceTrigger updateSourceTrigger)
        {
            return new ExpressionBinding<T>(type, property, valueGetter, valueSetter, updateSourceTrigger);
        }

        private sealed class ColumnBinding<T> : DependencyPropertyBinding<T>
        {
            public ColumnBinding(Type type, DependencyProperty property, Column<T> column, UpdateSourceTrigger? updateSourceTrigger)
                : base(type, property, updateSourceTrigger)
            {
                _column = column;
            }

            Column<T> _column;

            protected override T GetValue(DataRowPresenter dataRowPresenter)
            {
                var dataRow = dataRowPresenter.DataRow;
                return dataRow == null ? default(T) : _column[dataRow];
            }

            protected override void SetValue(DataRowPresenter dataRowPresenter, T value)
            {
                _column[dataRowPresenter.DataRow] = value;
            }
        }

        private sealed class ExpressionBinding<T> : DependencyPropertyBinding<T>
        {
            public ExpressionBinding(Type type, DependencyProperty property, Func<DataRowPresenter, T> valueGetter, Action<DataRowPresenter, T> valueSetter,
                UpdateSourceTrigger? updateSourceTrigger)
                : base(type, property, updateSourceTrigger)
            {
                _valueGetter = valueGetter;
                _valueSetter = valueSetter;
            }

            private Func<DataRowPresenter, T> _valueGetter;
            protected override T GetValue(DataRowPresenter dataRowPresenter)
            {
                return _valueGetter(dataRowPresenter);
            }

            private Action<DataRowPresenter, T> _valueSetter;
            protected override void SetValue(DataRowPresenter dataRowPresenter, T value)
            {
                _valueSetter(dataRowPresenter, value);
            }
        }

        private abstract class DependencyPropertyBinding<T> : DataBinding
        {
            protected DependencyPropertyBinding(Type type, DependencyProperty property, UpdateSourceTrigger? updateSourceTrigger)
            {
                Debug.Assert(property != null && property.PropertyType.IsAssignableFrom(typeof(T)));
                _property = property;
                if (_updateSourceTrigger.HasValue)
                {
                    var triggerValue = updateSourceTrigger.GetValueOrDefault();
                    if (triggerValue == UpdateSourceTrigger.Default)
                        triggerValue = ((FrameworkPropertyMetadata)property.GetMetadata(type)).DefaultUpdateSourceTrigger;
                    _updateSourceTrigger = triggerValue;
                }
            }

            private DependencyProperty _property;

            private UpdateSourceTrigger? _updateSourceTrigger;

            private bool IsTwoWay
            {
                get { return _updateSourceTrigger.HasValue; }
            }

            private UpdateSourceTrigger UpdateSourceTrigger
            {
                get { return _updateSourceTrigger.Value; }
            }

            private void OnLostFocus(object sender, RoutedEventArgs e)
            {
                UpdateSource((UIElement)sender);
            }

            private void OnPropertyChanged(object sender, EventArgs e)
            {
                UpdateSource((UIElement)sender);
            }

            private void UpdateSource(UIElement target)
            {
                var source = target.GetDataRowPresenter();
                UpdateSource(target, source);
            }

            public sealed override void Attach(UIElement element)
            {
                if (!IsTwoWay)
                    return;

                if (UpdateSourceTrigger == UpdateSourceTrigger.PropertyChanged)
                {
                    var dpd = DependencyPropertyDescriptor.FromProperty(_property, element.GetType());
                    Debug.Assert(dpd != null);
                    dpd.AddValueChanged(element, OnPropertyChanged);
                }
                else if (UpdateSourceTrigger == UpdateSourceTrigger.LostFocus)
                {
                    element.LostFocus += OnLostFocus;
                }
            }

            public sealed override void Detach(UIElement element)
            {
                if (!IsTwoWay)
                    return;
                if (UpdateSourceTrigger == UpdateSourceTrigger.PropertyChanged)
                {
                    var dpd = DependencyPropertyDescriptor.FromProperty(_property, element.GetType());
                    Debug.Assert(dpd != null);
                    dpd.RemoveValueChanged(element, OnPropertyChanged);
                }
                else if (UpdateSourceTrigger == UpdateSourceTrigger.LostFocus)
                {
                    element.LostFocus -= OnLostFocus;
                }
            }

            public sealed override void UpdateTarget(DataRowPresenter source, UIElement target)
            {
                target.SetValue(_property, GetValue(source));
            }

            protected abstract T GetValue(DataRowPresenter dataRowPresenter);

            public sealed override void UpdateSource(UIElement target, DataRowPresenter source)
            {
                if (!IsTwoWay)
                    return;

                var dataRow = source.DataRow;
                if (dataRow == null)
                    return;
                SetValue(source, (T)target.GetValue(_property));
            }

            protected abstract void SetValue(DataRowPresenter dataRowPresenter, T value);
        }

        private DataBinding()
        {
        }

        public abstract void UpdateTarget(DataRowPresenter source, UIElement target);

        public abstract void UpdateSource(UIElement target,  DataRowPresenter source);

        public abstract void Attach(UIElement element);

        public abstract void Detach(UIElement element);
    }
}
