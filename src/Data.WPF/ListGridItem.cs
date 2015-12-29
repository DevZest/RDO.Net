using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public class ListGridItem : GridItem
    {
        public static ListGridItem Create<T>(Action<DataRowPresenter, UIElement> refresh, Func<T> constructor = null)
            where T : UIElement, new()
        {
            if (constructor == null)
                constructor = () => new T();

            return new ListGridItem(constructor, (p, e) => refresh(p, (T)e));
        }

        internal ListGridItem(Func<UIElement> constructor, Action<DataRowPresenter, UIElement> refresh)
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
