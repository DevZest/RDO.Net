using System;
using System.Windows;
using System.Windows.Data;

namespace DevZest.Data.Windows
{
    internal sealed class DataBinding
    {
        private DataBinding(Action<DataRowPresenter, UIElement> updateTarget, Action<UIElement, DataRowPresenter> updateSource)
        {
            _updateTarget = updateTarget;
            _updateSource = updateSource;
        }

        Action<DataRowPresenter, UIElement> _updateTarget;
        public void UpdateTarget(DataRowPresenter dataRowPresenter, UIElement element)
        {
            _updateTarget(dataRowPresenter, element);
        }

        Action<UIElement, DataRowPresenter> _updateSource;
        public void UpdateSource(UIElement element, DataRowPresenter dataRowPresenter)
        {
            if (_updateSource != null)
                _updateSource(element, dataRowPresenter);
        }

        public void Attach(UIElement element)
        {
            throw new NotImplementedException();
        }

        public void Detach(UIElement element)
        {
            throw new NotImplementedException();
        }
    }
}
