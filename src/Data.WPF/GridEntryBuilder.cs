using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class GridEntryBuilder<TElement, TEntry, TBuilder> : IDisposable
        where TElement : UIElement, new()
        where TEntry : GridEntry
        where TBuilder : GridEntryBuilder<TElement, TEntry, TBuilder>
    {
        internal GridEntryBuilder(DataSetPresenterBuilderRange builderRange, TEntry entry)
        {
            _builderRange = builderRange;
            _Entry = entry;
        }

        public void Dispose()
        {
            _Entry = null;
        }

        private DataSetPresenterBuilderRange _builderRange;
        public DataSetPresenterBuilder End()
        {
            return End(_builderRange, Entry);
        }

        internal abstract DataSetPresenterBuilder End(DataSetPresenterBuilderRange builderRange, TEntry entry);

        internal abstract TBuilder This { get; }
               
        private TEntry _Entry;
        internal TEntry Entry
        {
            get
            {
                if (_Entry == null)
                    throw new ObjectDisposedException(GetType().FullName);

                return _Entry;
            }
        }

        public TBuilder Initialize(Action<TElement> initializer)
        {
            Entry.InitInitializer(initializer);
            return This;
        }

        public TBuilder Cleanup(Action<TElement> cleanup)
        {
            Entry.InitCleanup(cleanup);
            return This;
        }

        public TBuilder Behaviors(params IBehavior<TElement>[] behaviors)
        {
            Entry.InitBehaviors(behaviors);
            return This;
        }

        public TBuilder Bind(Action<TElement> updateTarget)
        {
            Entry.AddBinding(Binding.Bind(updateTarget));
            return This;
        }

        public TBuilder BindToSource(Action<TElement> updateSource, params BindingTrigger[] triggers)
        {
            Entry.AddBinding(Binding.BindToSource(updateSource, triggers));
            return This;
        }

        public TBuilder BindTwoWay(Action<TElement> updateTarget, Action<TElement> updateSource, params BindingTrigger[] triggers)
        {
            Entry.AddBinding(Binding.BindTwoWay(updateTarget, updateSource, triggers));
            return This;
        }
    }
}
