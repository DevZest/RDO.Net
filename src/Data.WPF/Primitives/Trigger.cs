using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class Trigger
    {
        protected Trigger(Binding binding)
        {
            Debug.Assert(binding != null);
            Binding = binding;
        }

        public Binding Binding { get; private set; }

        public Template Template
        {
            get { return Binding.Template; }
        }

        public abstract TriggerEvent Event { get; }
    }

    internal sealed class Trigger<T> : Trigger
        where T : UIElement, new()
    {
        public Trigger(Binding binding, TriggerEvent<T> triggerEvent, Action<T> triggerAction)
            : base(binding)
        {
            Debug.Assert(triggerEvent != null && triggerAction != null);
            triggerEvent.Trigger = this;
            _event = triggerEvent;
            _action = triggerAction;
        }

        public Trigger(Binding binding, TriggerEvent<T> triggerEvent, Action<T, int, IReadOnlyList<RowPresenter>> triggerAction)
            : this(binding, triggerEvent, x =>
            {
                var blockView = x.GetBlockView();
                triggerAction(x, blockView.Ordinal, blockView);
            })
        {
        }

        public Trigger(Binding binding, TriggerEvent<T> triggerEvent, Action<T, RowPresenter> triggerAction)
            : this(binding, triggerEvent, x =>
            {
                var rowPresenter = x.GetRowPresenter();
                triggerAction(x, rowPresenter);
            })
        {
        }

        private readonly TriggerEvent<T> _event;
        private readonly Action<T> _action;

        public override TriggerEvent Event
        {
            get { return _event; }
        }

        public void Attach(T element)
        {
            _event.Attach(element);
        }

        public void Detach(T element)
        {
            _event.Detach(element);
        }

        public void ExecuteAction(T element)
        {
            Template.ExecutingTrigger = this;
            try
            {
                _action(element);
            }
            finally
            {
                Template.ExecutingTrigger = null;
            }
        }
    }
}
