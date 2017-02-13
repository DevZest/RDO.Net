using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class RowBinding<T> : RowBinding
        where T : UIElement, new()
    {
        public RowBinding(Action<T, RowPresenter> onRefresh)
        {
            _onRefresh = onRefresh;
        }

        public RowBinding(Action<T, RowPresenter> onRefresh, Action<T, RowPresenter> onSetup, Action<T, RowPresenter> onCleanup)
            : this(onRefresh)
        {
            _onSetup = onSetup;
            _onCleanup = onCleanup;
        }

        public RowInput<T> Input { get; private set; }

        internal sealed override void FlushInput(UIElement element)
        {
            if (Input != null)
                Input.Flush((T)element);
        }

        private IColumnSet Columns
        {
            get { return Input == null ? ColumnSet.Empty : Input.Columns; }
        }

        List<T> _cachedElements;

        private T Create()
        {
            var result = new T();
            OnCreated(result);
            return result;
        }

        public T SettingUpElement { get; private set; }

        internal sealed override UIElement GetSettingUpElement()
        {
            return SettingUpElement;
        }

        internal sealed override void BeginSetup(UIElement value)
        {
            SettingUpElement = (T)value;
        }

        internal sealed override void BeginSetup()
        {
            SettingUpElement = CachedList.GetOrCreate(ref _cachedElements, Create);
        }

        internal sealed override UIElement Setup(RowPresenter rowPresenter)
        {
            Debug.Assert(SettingUpElement != null);
            SettingUpElement.SetRowPresenter(rowPresenter);
            Setup(SettingUpElement, rowPresenter);
            Refresh(SettingUpElement, rowPresenter);
            if (Input != null)
                Input.Attach(SettingUpElement);
            return SettingUpElement;
        }

        internal sealed override void EndSetup()
        {
            SettingUpElement = null;
        }

        private Action<T, RowPresenter> _onSetup;
        private void Setup(T element, RowPresenter rowPresenter)
        {
            if (_onSetup != null)
                _onSetup(element, rowPresenter);
        }

        private Action<T, RowPresenter> _onRefresh;
        internal void Refresh(T element, RowPresenter rowPresenter)
        {
            if (_onRefresh != null)
                _onRefresh(element, rowPresenter);
        }

        private Action<T, RowPresenter> _onCleanup;
        private void Cleanup(T element, RowPresenter rowPresenter)
        {
            if (_onCleanup != null)
                _onCleanup(element, rowPresenter);
        }

        internal sealed override void Refresh(UIElement element)
        {
            var rowPresenter = element.GetRowPresenter();
            var e = (T)element;
            if (Input != null)
                Input.Refresh(e, rowPresenter);
            else
                Refresh(e, rowPresenter);
        }

        internal sealed override void Cleanup(UIElement element, bool recycle)
        {
            var rowPresenter = element.GetRowPresenter();
            var e = (T)element;
            if (Input != null)
                Input.Detach(e);
            Cleanup(e, rowPresenter);
            e.SetRowPresenter(null);
            if (recycle)
                CachedList.Recycle(ref _cachedElements, e);
        }

        public RowInput<T> BeginInput(Trigger<T> flushTrigger)
        {
            VerifyNotSealed();
            if (Input != null)
                throw new InvalidOperationException(Strings.TwoWayBinding_InputAlreadyExists);

            return Input = new RowInput<T>(this, flushTrigger);
        }

        public RowBinding<T> WithInput<TData>(Trigger<T> flushTrigger, Column<TData> column, Func<T, TData> getValue)
        {
            return BeginInput(flushTrigger).WithFlush(column, getValue).EndInput();
        }

        public new T this[RowPresenter rowPresenter]
        {
            get { return (T)base[rowPresenter]; }
        }
    }
}
