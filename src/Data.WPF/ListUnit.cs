using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public class ListUnit : TemplateUnit
    {
        internal static ListUnit Create<T>()
            where T : UIElement, new()
        {
            return new ListUnit(() => new T());
        }

        internal ListUnit(Func<UIElement> constructor)
            : base(constructor)
        {
        }

        public sealed class Builder<T> : TemplateUnit.Builder<T, ListUnit, Builder<T>>
            where T : UIElement, new()
        {
            internal Builder(GridRangeConfig rangeConfig)
                : base(rangeConfig, ListUnit.Create<T>())
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override DataSetPresenterBuilder End(GridRangeConfig rangeConfig, ListUnit unit)
            {
                return rangeConfig.End(unit);
            }
        }
    }
}
