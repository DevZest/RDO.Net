using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public class ListEntry : GridEntry
    {
        internal static ListEntry Create<T>()
            where T : UIElement, new()
        {
            return new ListEntry(() => new T());
        }

        internal ListEntry(Func<UIElement> constructor)
            : base(constructor)
        {
        }

        private List<DataBinding> _dataBindings = new List<DataBinding>();

        internal void AddDataBinding(DataBinding dataBinding)
        {
            Debug.Assert(dataBinding != null);
            _dataBindings.Add(dataBinding);
        }

        internal void Refresh(UIElement uiElement)
        {
            var dataRowPresenter = uiElement.GetDataRowPresenter();
            Debug.Assert(dataRowPresenter != null);

            foreach (var dataBinding in _dataBindings)
                dataBinding.UpdateTarget(dataRowPresenter, uiElement);
        }

        internal override void OnInitialize(UIElement element)
        {
            base.OnInitialize(element);
            foreach (var dataBinding in _dataBindings)
                dataBinding.Attach(element);

            Refresh(element);
        }

        internal override void OnCleanup(UIElement element)
        {
            base.OnCleanup(element);
            foreach (var dataBinding in _dataBindings)
                dataBinding.Detach(element);
        }
    }
}
