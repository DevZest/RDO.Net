using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ScalarEntry : GridEntry
    {
        internal static ScalarEntry Create<T>(Action<T> initializer, FlowMode flowMode, Action<T> cleanup, IBehavior<T>[] behaviors)
            where T : UIElement, new()
        {
            Debug.Assert(initializer != null);

            var result = new ScalarEntry(() => new T(), x => initializer((T)x), flowMode, x => cleanup((T)x));
            result.InitBehaviors(behaviors);
            return result;
        }

        private ScalarEntry(Func<UIElement> constructor, Action<UIElement> initializer, FlowMode flowMode, Action<UIElement> cleanup)
            : base(constructor)
        {
            Debug.Assert(initializer != null);
            InitInitializer(initializer);
            FlowMode = flowMode;
            InitCleanup(cleanup);
        }

        public FlowMode FlowMode { get; private set; }
    }
}
