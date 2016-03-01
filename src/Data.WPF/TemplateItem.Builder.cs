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
            public DataPresenterBuilder End()
            {
                return End(_rangeConfig, Item);
            }

            internal abstract DataPresenterBuilder End(GridRangeConfig rangeConfig, TItem item);

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

            public TBuilder Bind(Action<BindingSource, TElement> updateTarget)
            {
                Item.AddBinding(Binding.Bind(Item, updateTarget));
                return This;
            }

            public TBuilder BindToSource(Action<TElement, BindingSource> updateSource, params BindingTrigger[] triggers)
            {
                Item.AddBinding(Binding.BindToSource(Item, updateSource, triggers));
                return This;
            }

            public TBuilder BindTwoWay(Action<BindingSource, TElement> updateTarget, Action<TElement, BindingSource> updateSource, params BindingTrigger[] triggers)
            {
                Item.AddBinding(Binding.BindTwoWay(Item, updateTarget, updateSource, triggers));
                return This;
            }
        }
    }
}
