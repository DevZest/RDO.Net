using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;

namespace DevZest.Data.Windows
{
    public sealed class ListEntryBuilder<T> : IDisposable
        where T : UIElement, new()
    {
        internal ListEntryBuilder(ListEntry listEntry)
        {
            Debug.Assert(listEntry != null);
            _listEntry = listEntry;
        }

        public void Dispose()
        {
            _listEntry = null;
        }

        private ListEntry _listEntry;
        private ListEntry ListEntry
        {
            get
            {
                if (_listEntry == null)
                    throw new ObjectDisposedException(GetType().FullName);

                return _listEntry;
            }
        }

        public ListEntryBuilder<T> WithInitializer(Action<T> initializer)
        {
            ListEntry.InitInitializer(initializer);
            return this;
        }

        public ListEntryBuilder<T> WithCleanup(Action<T> cleanup)
        {
            ListEntry.InitCleanup(cleanup);
            return this;
        }

        public ListEntryBuilder<T> WithBehaviors(params IBehavior<T>[] behaviors)
        {
            ListEntry.InitBehaviors(behaviors);
            return this;
        }

        public ListEntryBuilder<T> Bind<TValue>(DependencyProperty property, Column<TValue> column)
        {
            ListEntry.AddDataBinding(DataBinding.Create<TValue>(typeof(T), property, column));
            return this;
        }

        public ListEntryBuilder<T> Bind<TValue>(DependencyProperty property, Func<DataRowPresenter, TValue> valueGetter)
        {
            ListEntry.AddDataBinding(DataBinding.Create(typeof(T), property, valueGetter));
            return this;
        }

        public ListEntryBuilder<T> BindTwoWay<TValue>(DependencyProperty property, Column<TValue> column,
            UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.Default)
        {
            ListEntry.AddDataBinding(DataBinding.CreateTwoWay<TValue>(typeof(T), property, column, updateSourceTrigger));
            return this;
        }

        public ListEntryBuilder<T> BindTwoWay<TValue>(DependencyProperty property, Func<DataRowPresenter, TValue> valueGetter,
            Action<DataRowPresenter, TValue> valueSetter, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.Default)
        {
            ListEntry.AddDataBinding(DataBinding.CreateTwoWay(typeof(T), property, valueGetter, valueSetter, updateSourceTrigger));
            return this;
        }
    }
}
