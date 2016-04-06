using System;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    partial class TemplateItem
    {
        public abstract class Builder<TElement, TItem, TBuilder> : IDisposable
            where TElement : UIElement, new()
            where TItem : TemplateItem
            where TBuilder : Builder<TElement, TItem, TBuilder>
        {
            internal Builder(TemplateItemBuilderFactory builderFactory, TItem item)
            {
                _builderFactory = builderFactory;
                _item = item;
            }

            public void Dispose()
            {
                _item = null;
            }

            private TemplateItemBuilderFactory _builderFactory;
            public TemplateBuilder End()
            {
                Item.AutoSizeMeasureOrder = _builderFactory.AutoSizeMeasureOrder;
                AddItem(_builderFactory.Template, _builderFactory.GridRange, Item);
                return _builderFactory.TemplateBuilder;
            }

            internal abstract void AddItem(Template template, GridRange gridRange, TItem item);

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

            public TBuilder Cleanup(Action<TElement> cleanupAction)
            {
                Item.InitCleanupAction(cleanupAction);
                return This;
            }

            public TBuilder Behaviors(params IBehavior<TElement>[] behaviors)
            {
                Item.InitBehaviors(behaviors);
                return This;
            }
        }
    }
}
