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

        private void InitFlowMode(FlowMode value)
        {
            FlowMode = value;
        }

        public sealed class Builder<T> : GridEntry.Builder<T, ScalarEntry, Builder<T>>
            where T : UIElement, new()
        {
            internal Builder(DataSetPresenterBuilderRange builderRange)
                : base(builderRange, ScalarEntry.Create<T>())
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override DataSetPresenterBuilder End(DataSetPresenterBuilderRange builderRange, ScalarEntry entry)
            {
                return builderRange.ScalarEntry(entry);
            }

            public Builder<T> FlowMode(FlowMode value)
            {
                Entry.InitFlowMode(value);
                return this;
            }
        }
    }
}
