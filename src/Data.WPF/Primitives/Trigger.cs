using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal class Trigger<T>
        where T : UIElement, new()
    {
        public Trigger(TriggerEvent<T> triggerEvent, Action<T> triggerAction)
        {
            Debug.Assert(triggerEvent != null && triggerAction != null);
            triggerEvent.Trigger = this;
            _event = triggerEvent;
            _action = triggerAction;
        }

        public Trigger(TriggerEvent<T> triggerEvent, Action<T, int, IReadOnlyList<RowPresenter>> triggerAction)
            : this(triggerEvent, x =>
            {
                var blockView = x.GetBlockView();
                triggerAction(x, blockView.Ordinal, blockView);
            })
        {
        }

        public Trigger(TriggerEvent<T> triggerEvent, Action<T, RowPresenter> triggerAction)
            : this(triggerEvent, x =>
            {
                var rowPresenter = x.GetRowPresenter();
                triggerAction(x, rowPresenter);
            })
        {
        }

        private readonly TriggerEvent<T> _event;
        private readonly Action<T> _action;

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
            var binding = element.GetBinding();
            var dataPresenter = binding.Template.DataPresenter;
            if (dataPresenter == null)
                _action(element);
            else
            {
                Debug.Assert(dataPresenter.ExecutingEvent == null);
                dataPresenter.ExecutingEvent = _event;
                try
                {
                    _action(element);
                }
                finally
                {
                    dataPresenter.ExecutingEvent = null;
                }
            }
        }
    }
}
