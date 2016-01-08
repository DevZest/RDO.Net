using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    partial class TemplateUnit
    {
        public abstract class Builder<TElement, TUnit, TBuilder> : IDisposable
            where TElement : UIElement, new()
            where TUnit : TemplateUnit
            where TBuilder : Builder<TElement, TUnit, TBuilder>
        {
            internal Builder(GridRangeConfig rangeConfig, TUnit unit)
            {
                _rangeConfig = rangeConfig;
                _unit = unit;
            }

            public void Dispose()
            {
                _unit = null;
            }

            private GridRangeConfig _rangeConfig;
            public DataSetPresenterBuilder End()
            {
                return End(_rangeConfig, Unit);
            }

            internal abstract DataSetPresenterBuilder End(GridRangeConfig rangeConfig, TUnit unit);

            internal abstract TBuilder This { get; }

            private TUnit _unit;
            internal TUnit Unit
            {
                get
                {
                    if (_unit == null)
                        throw new ObjectDisposedException(GetType().FullName);

                    return _unit;
                }
            }

            public TBuilder Initialize(Action<TElement> initializer)
            {
                Unit.InitInitializer(initializer);
                return This;
            }

            public TBuilder Cleanup(Action<TElement> cleanup)
            {
                Unit.InitCleanup(cleanup);
                return This;
            }

            public TBuilder Behaviors(params IBehavior<TElement>[] behaviors)
            {
                Unit.InitBehaviors(behaviors);
                return This;
            }

            public TBuilder Bind(Action<TElement> updateTarget)
            {
                Unit.AddBinding(Binding.Bind(updateTarget));
                return This;
            }

            public TBuilder BindToSource(Action<TElement> updateSource, params BindingTrigger[] triggers)
            {
                Unit.AddBinding(Binding.BindToSource(updateSource, triggers));
                return This;
            }

            public TBuilder BindTwoWay(Action<TElement> updateTarget, Action<TElement> updateSource, params BindingTrigger[] triggers)
            {
                Unit.AddBinding(Binding.BindTwoWay(updateTarget, updateSource, triggers));
                return This;
            }
        }
    }
}
