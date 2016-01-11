using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ScalarUnit : TemplateUnit
    {
        internal static ScalarUnit Create<T>()
            where T : UIElement, new()
        {
            return new ScalarUnit(() => new T());
        }

        private ScalarUnit(Func<UIElement> constructor)
            : base(constructor)
        {
        }

        public FlowMode FlowMode { get; private set; }

        private void InitFlowMode(FlowMode value)
        {
            FlowMode = value;
        }

        public sealed class Builder<T> : TemplateUnit.Builder<T, ScalarUnit, Builder<T>>
            where T : UIElement, new()
        {
            internal Builder(GridRangeConfig rangeConfig)
                : base(rangeConfig, ScalarUnit.Create<T>())
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override DataViewBuilder End(GridRangeConfig rangeConfig, ScalarUnit unit)
            {
                return rangeConfig.End(unit);
            }

            public Builder<T> FlowMode(FlowMode value)
            {
                Unit.InitFlowMode(value);
                return this;
            }
        }
    }
}
