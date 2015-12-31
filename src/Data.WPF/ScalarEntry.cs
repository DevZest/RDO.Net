using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ScalarEntry : GridEntry
    {
        internal static ScalarEntry Create<T>()
            where T : UIElement, new()
        {
            return new ScalarEntry(() => new T());
        }

        private ScalarEntry(Func<UIElement> constructor)
            : base(constructor)
        {
        }

        public FlowMode FlowMode { get; private set; }

        internal void InitFlowMode(FlowMode value)
        {
            FlowMode = value;
        }
    }
}
