using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ScalarGridItem : GridItem
    {
        public static ScalarGridItem Create<T>(Action<T> initializer, Func<UIElement> constructor = null)
            where T : UIElement, new()
        {
            if (initializer == null)
                throw new ArgumentNullException(nameof(initializer));

            if (constructor == null)
                constructor = () => new T();

            return new ScalarGridItem(constructor, x => initializer((T)x));
        }

        private ScalarGridItem(Func<UIElement> constructor, Action<UIElement> initializer)
            : base(constructor)
        {
            Debug.Assert(initializer != null);
            _initializer = initializer;

        }

        private FlowMode _flowMode;
        public FlowMode FlowMode
        {
            get { return _flowMode; }
            set
            {
                VerifyNotSealed();
                _flowMode = value;
            }
        }

        Action<UIElement> _initializer;
        internal override void OnMounted(UIElement uiElement)
        {
            _initializer(uiElement);
        }
    }
}
