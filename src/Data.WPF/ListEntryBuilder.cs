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

        public ListEntryBuilder<T> Bind<TValue>(DependencyProperty property, Column<T> column)
        {
            throw new NotImplementedException();
        }

        public ListEntryBuilder<T> Bind<TValue>(DependencyProperty property, Column<T> column, BindingMode mode, UpdateSourceTrigger updateSourceTrigger)
        {
            throw new NotImplementedException();
        }
    }
}
