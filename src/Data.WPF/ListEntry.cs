using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public class ListEntry : GridEntry
    {
        internal static ListEntry Create<T>(Action<DataRowPresenter, T> refresh, Func<T> constructor = null)
            where T : UIElement, new()
        {
            if (constructor == null)
                constructor = () => new T();

            return new ListEntry(constructor, (p, e) => refresh(p, (T)e));
        }

        internal ListEntry(Func<UIElement> constructor, Action<DataRowPresenter, UIElement> refresh)
            : base(constructor)
        {
            _refresh = refresh;
        }

        Action<DataRowPresenter, UIElement> _refresh;
        internal override void Refresh(UIElement uiElement)
        {
            if (_refresh == null)
                return;

            var dataRowPresenter = uiElement.GetDataRowPresenter();
            Debug.Assert(dataRowPresenter != null);
            _refresh(dataRowPresenter, uiElement);
        }

        internal override void OnMounted(UIElement uiElement)
        {
            Refresh(uiElement);
        }
    }
}
