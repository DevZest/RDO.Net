using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public class RowItem : TemplateItem, IConcatList<RowItem>
    {
        #region IConcatList<RowItem>

        int IReadOnlyCollection<RowItem>.Count
        {
            get { return 1; }
        }

        RowItem IReadOnlyList<RowItem>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        void IConcatList<RowItem>.Sort(Comparison<RowItem> comparision)
        {
        }

        IEnumerator<RowItem> IEnumerable<RowItem>.GetEnumerator()
        {
            yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        #endregion

        private struct Binding
        {
            public Binding(BindingTrigger trigger, Action<RowPresenter, UIElement> action)
            {
                Trigger = trigger;
                Action = action;
            }

            public readonly BindingTrigger Trigger;
            public readonly Action<RowPresenter, UIElement> Action;

            public void Update(UIElement element, BindingTrigger trigger)
            {
                if (trigger == Trigger)
                    Action(element.GetRowPresenter(), element);
            }
        }

        public sealed class Builder<T> : TemplateItem.Builder<T, RowItem, Builder<T>>
            where T : UIElement, new()
        {
            internal Builder(TemplateBuilder templateBuilder)
                : base(templateBuilder, RowItem.Create<T>())
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override void AddItem(Template template, GridRange gridRange, RowItem item)
            {
                template.AddRowItem(gridRange, item);
            }

            public Builder<T> OnMount(Action<T, RowPresenter> onMount)
            {
                if (onMount == null)
                    throw new ArgumentNullException(nameof(onMount));
                TemplateItem.OnMount(onMount);
                return This;
            }

            public Builder<T> OnUnmount(Action<T, RowPresenter> onUnmount)
            {
                if (onUnmount == null)
                    throw new ArgumentNullException(nameof(onUnmount));
                TemplateItem.OnUnmount(onUnmount);
                return This;
            }

            public Builder<T> OnRefresh(Action<T, RowPresenter> onRefresh)
            {
                if (onRefresh == null)
                    throw new ArgumentNullException(nameof(onRefresh));
                TemplateItem.OnRefresh(onRefresh);
                return This;
            }

            public Builder<T> Bind(BindingTrigger trigger, Action<RowPresenter, T> action)
            {
                if (trigger == null)
                    throw new ArgumentNullException(nameof(trigger));
                if (action == null)
                    throw new ArgumentNullException(nameof(action));
                TemplateItem.Bind(trigger, action);
                return This;
            }
        }

        internal static RowItem Create<T>()
            where T : UIElement, new()
        {
            return new RowItem(() => new T());
        }

        internal RowItem(Func<UIElement> constructor)
            : base(constructor)
        {
        }

        internal UIElement Mount(RowPresenter rowPresenter, Action<UIElement> initializer)
        {
            Debug.Assert(rowPresenter != null);
            return base.Mount(x => Initialize(x, rowPresenter), initializer);
        }

        protected virtual void Initialize(UIElement element, RowPresenter rowPresenter)
        {
            element.SetRowPresenter(rowPresenter);
        }

        protected override void SetBindings(UIElement element)
        {
            foreach (var binding in _bindings)
                binding.Trigger.Attach(element);
        }

        protected override void Cleanup(UIElement element)
        {
            foreach (var binding in _bindings)
                binding.Trigger.Detach(element);
            element.SetRowPresenter(null);
        }

        protected sealed override void OnMount(UIElement element)
        {
            if (_onMount != null)
                _onMount(element, element.GetRowPresenter());
        }

        private Action<UIElement, RowPresenter> _onMount;
        private void OnMount<T>(Action<T, RowPresenter> onMount)
            where T : UIElement
        {
            Debug.Assert(onMount != null);
            _onMount = (element, rowPresenter) => onMount((T)element, rowPresenter);
        }

        protected sealed override void OnUnmount(UIElement element)
        {
            if (_onMount != null)
                _onMount(element, element.GetRowPresenter());
        }

        private Action<UIElement, RowPresenter> _onUnmount;
        private void OnUnmount<T>(Action<T, RowPresenter> onUnmount)
            where T : UIElement
        {
            Debug.Assert(onUnmount != null);
            _onUnmount = (element, rowPresenter) => onUnmount((T)element, rowPresenter);
        }

        internal sealed override void Refresh(UIElement element)
        {
            if (_onRefresh != null)
                _onRefresh(element, element.GetRowPresenter());
        }

        private Action<UIElement, RowPresenter> _onRefresh;
        private void OnRefresh<T>(Action<T, RowPresenter> onRefresh)
            where T : UIElement
        {
            Debug.Assert(onRefresh != null);
            _onRefresh = (element, rowPresenter) => onRefresh((T)element, rowPresenter);
        }

        private IList<Binding> _bindings = Array<Binding>.Empty;
        private void Bind<T>(BindingTrigger trigger, Action<RowPresenter, T> action)
            where T : UIElement
        {
            if (_bindings == Array<Binding>.Empty)
                _bindings = new List<Binding>();

            _bindings.Add(new Binding(trigger, (rowPresenter, element) => action(rowPresenter, (T)element)));
        }

        internal void UpdateBinding(UIElement element, BindingTrigger trigger)
        {
            foreach (var binding in _bindings)
                binding.Update(element, trigger);
        }

        internal sealed override void VerifyRowRange(GridRange rowRange)
        {
            if (!rowRange.Contains(GridRange))
                throw new InvalidOperationException(Strings.RowItem_OutOfRowRange(Ordinal));
        }
    }
}
