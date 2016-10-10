using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public sealed class RowItem : TemplateItem, IConcatList<RowItem>
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

        private struct Trigger
        {
            public Trigger(TriggerEvent triggerEvent, Action<RowPresenter, UIElement> action)
            {
                Event = triggerEvent;
                Action = action;
            }

            public readonly TriggerEvent Event;
            public readonly Action<RowPresenter, UIElement> Action;

            public void Execute(UIElement element, TriggerEvent triggerEvent)
            {
                if (triggerEvent == Event)
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

            public Builder<T> OnSetup(Action<T, RowPresenter> onSetup)
            {
                if (onSetup == null)
                    throw new ArgumentNullException(nameof(onSetup));
                TemplateItem.InitOnSetup(onSetup);
                return This;
            }

            public Builder<T> OnCleanup(Action<T, RowPresenter> onCleanup)
            {
                if (onCleanup == null)
                    throw new ArgumentNullException(nameof(onCleanup));
                TemplateItem.InitOnCleanup(onCleanup);
                return This;
            }

            public Builder<T> OnRefresh(Action<T, RowPresenter> onRefresh)
            {
                if (onRefresh == null)
                    throw new ArgumentNullException(nameof(onRefresh));
                TemplateItem.InitOnRefresh(onRefresh);
                return This;
            }

            public Builder<T> AddTrigger(TriggerEvent triggerEvent, Action<RowPresenter, T> triggerAction)
            {
                if (triggerEvent == null)
                    throw new ArgumentNullException(nameof(triggerEvent));
                if (triggerAction == null)
                    throw new ArgumentNullException(nameof(triggerAction));
                TemplateItem.AddTrigger(triggerEvent, triggerAction);
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

        private Action<UIElement, RowPresenter> _onSetup;
        private void InitOnSetup<T>(Action<T, RowPresenter> onSetup)
            where T : UIElement
        {
            Debug.Assert(onSetup != null);
            _onSetup = (element, rowPresenter) => onSetup((T)element, rowPresenter);
        }

        private void OnSetup(UIElement element, RowPresenter rowPresenter)
        {
            Debug.Assert(rowPresenter != null);
            element.SetRowPresenter(rowPresenter);
            if (_onSetup != null)
                _onSetup(element, rowPresenter);
            foreach (var trigger in _triggers)
                trigger.Event.Attach(element);
        }

        internal UIElement Setup(RowPresenter rowPresenter)
        {
            return Setup(x => OnSetup(x, rowPresenter));
        }

        private Action<UIElement, RowPresenter> _onCleanup;
        private void InitOnCleanup<T>(Action<T, RowPresenter> onCleanup)
            where T : UIElement
        {
            Debug.Assert(onCleanup != null);
            _onCleanup = (element, rowPresenter) => onCleanup((T)element, rowPresenter);
        }

        protected override void OnCleanup(UIElement element)
        {
            foreach (var trigger in _triggers)
                trigger.Event.Detach(element);
            if (_onCleanup != null)
                _onCleanup(element, element.GetRowPresenter());
            element.SetRowPresenter(null);
        }

        internal sealed override void Refresh(UIElement element)
        {
            if (_onRefresh != null)
            {
                var rowPresenter = element.GetRowPresenter();
                if (!rowPresenter.IsEditing)
                    _onRefresh(element, rowPresenter);
            }
        }

        private Action<UIElement, RowPresenter> _onRefresh;
        private void InitOnRefresh<T>(Action<T, RowPresenter> onRefresh)
            where T : UIElement
        {
            Debug.Assert(onRefresh != null);
            _onRefresh = (element, rowPresenter) => onRefresh((T)element, rowPresenter);
        }

        private IList<Trigger> _triggers = Array<Trigger>.Empty;
        private void AddTrigger<T>(TriggerEvent trigger, Action<RowPresenter, T> action)
            where T : UIElement
        {
            if (_triggers == Array<Trigger>.Empty)
                _triggers = new List<Trigger>();

            _triggers.Add(new Trigger(trigger, (rowPresenter, element) => action(rowPresenter, (T)element)));
        }

        internal void ExecuteTrigger(UIElement element, TriggerEvent triggerEvent)
        {
            foreach (var trigger in _triggers)
                trigger.Execute(element, triggerEvent);
        }

        internal sealed override void VerifyRowRange(GridRange rowRange)
        {
            if (!rowRange.Contains(GridRange))
                throw new InvalidOperationException(Strings.RowItem_OutOfRowRange(Ordinal));
        }
    }
}
