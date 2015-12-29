using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ScalarGridItem : GridItem
    {
        internal static ScalarGridItem Create<T>(Action<T> initializer, FlowMode flowMode)
            where T : UIElement, new()
        {
            Debug.Assert(initializer != null);

            return new ScalarGridItem(() => new T(), x => initializer((T)x), flowMode);
        }

        private ScalarGridItem(Func<UIElement> constructor, Action<UIElement> initializer, FlowMode flowMode)
            : base(constructor)
        {
            Debug.Assert(initializer != null);
            _initializer = initializer;
            FlowMode = flowMode;
        }

        public FlowMode FlowMode { get; private set; }

        Action<UIElement> _initializer;
        internal override void OnMounted(UIElement uiElement)
        {
            _initializer(uiElement);
        }
    }
}
