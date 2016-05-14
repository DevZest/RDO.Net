using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public sealed class SubviewItem : RowItem, IConcatList<SubviewItem>
    {
        public sealed new class Builder<T> : TemplateItem.Builder<T, SubviewItem, Builder<T>>
            where T : DataView, new()
        {
            internal Builder(TemplateBuilder templateBuilder, Func<RowPresenter, DataPresenter> dataPresenterConstructor)
                : base(templateBuilder, SubviewItem.Create<T>(dataPresenterConstructor))
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override void AddItem(Template template, GridRange gridRange, SubviewItem item)
            {
                template.AddSubviewItem(gridRange, item);
            }
        }

        internal static SubviewItem Create<T>(Func<RowPresenter, DataPresenter> dataPresenterConstructor)
            where T : DataView, new()
        {
            return new SubviewItem(() => new T(), dataPresenterConstructor);
        }

        private SubviewItem(Func<UIElement> constructor, Func<RowPresenter, DataPresenter> dataPresenterConstructor)
                : base(constructor)
        {
            Debug.Assert(dataPresenterConstructor != null);
            DataPresenterConstructor = dataPresenterConstructor;
        }

        internal int Index { get; private set; }

        internal void Seal(Template owner, GridRange gridRange, int ordinal, int index)
        {
            base.Construct(owner, gridRange, ordinal);
            Index = index;
        }

        internal Func<RowPresenter, DataPresenter> DataPresenterConstructor { get; private set; }

        internal sealed override void Initialize(UIElement element)
        {
            base.Initialize(element);
            var dataView = (DataView)element;
            var parentRow = dataView.GetRowPresenter();
            dataView.Initialize(this[parentRow]);
        }

        internal sealed override void Cleanup(UIElement element)
        {
            base.Cleanup(element);
            var dataView = (DataView)element;
            dataView.Cleanup();
        }

        bool IConcatList<SubviewItem>.IsReadOnly
        {
            get { return true; }
        }

        int IReadOnlyCollection<SubviewItem>.Count
        {
            get { return 1; }
        }

        SubviewItem IReadOnlyList<SubviewItem>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        IEnumerator<SubviewItem> IEnumerable<SubviewItem>.GetEnumerator()
        {
            yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        private ConditionalWeakTable<RowPresenter, DataPresenter> _dataPresenters = new ConditionalWeakTable<RowPresenter, DataPresenter>();

        internal DataPresenter this[RowPresenter row]
        {
            get
            {
                Debug.Assert(row != null && row.Template == Template);
                DataPresenter result;
                if (!_dataPresenters.TryGetValue(row, out result))
                {
                    result = DataPresenterConstructor(row);
                    _dataPresenters.Add(row, result);
                }
                return result;
            }
        }
    }
}
