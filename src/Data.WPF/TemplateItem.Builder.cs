using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    partial class TemplateItem
    {
        public abstract class Builder<TElement, TItem, TBuilder> : IDisposable
            where TElement : UIElement, new()
            where TItem : TemplateItem
            where TBuilder : Builder<TElement, TItem, TBuilder>
        {
            internal Builder(GridRangeConfig rangeConfig, TItem item)
            {
                _rangeConfig = rangeConfig;
                _item = item;
            }

            public void Dispose()
            {
                _item = null;
            }

            private GridRangeConfig _rangeConfig;
            public DataViewBuilder End()
            {
                return End(_rangeConfig, Item);
            }

            internal abstract DataViewBuilder End(GridRangeConfig rangeConfig, TItem item);

            internal abstract TBuilder This { get; }

            private TItem _item;
            internal TItem Item
            {
                get
                {
                    if (_item == null)
                        throw new ObjectDisposedException(GetType().FullName);

                    return _item;
                }
            }

            public TBuilder Initialize(Action<TElement> initializer)
            {
                Item.InitInitializer(initializer);
                return This;
            }

            public TBuilder Cleanup(Action<TElement> cleanup)
            {
                Item.InitCleanup(cleanup);
                return This;
            }

            public TBuilder Behaviors(params IBehavior<TElement>[] behaviors)
            {
                Item.InitBehaviors(behaviors);
                return This;
            }

            public TBuilder Bind(Action<TElement> updateTarget)
            {
                Item.AddBinding(Binding.Bind(updateTarget));
                return This;
            }

            public TBuilder BindToSource(Action<TElement> updateSource, params BindingTrigger[] triggers)
            {
                Item.AddBinding(Binding.BindToSource(updateSource, triggers));
                return This;
            }

            public TBuilder BindTwoWay(Action<TElement> updateTarget, Action<TElement> updateSource, params BindingTrigger[] triggers)
            {
                Item.AddBinding(Binding.BindTwoWay(updateTarget, updateSource, triggers));
                return This;
            }
        }
    }
}
