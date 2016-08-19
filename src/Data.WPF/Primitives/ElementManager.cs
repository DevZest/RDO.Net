using DevZest.Data.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class ElementManager : RowManager
    {
        internal ElementManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy)
            : base(template, dataSet, where, orderBy)
        {
            BlockViews = new BlockViewCollection(this);
        }

        internal BlockViewCollection BlockViews { get; private set; }

        List<RowView> _cachedRowViews;

        internal RowView Realize(RowPresenter row)
        {
            Debug.Assert(row != null);

            if (row.View != null)
                return row.View;

            var rowView = CachedList.GetOrCreate(ref _cachedRowViews, Template.RowViewConstructor);
            row.View = rowView;
            rowView.Initialize(row);
            if (Template.RowViewInitializer != null)
                Template.RowViewInitializer(rowView);
            return rowView;
        }

        internal void Virtualize(RowPresenter row)
        {
            Debug.Assert(row != null && row.View != null);

            if (row.IsEditing)
                return;

            var rowView = row.View;
            if (Template.RowViewCleanupAction != null)
                Template.RowViewCleanupAction(rowView);
            row.View = null;
            rowView.Cleanup();
            CachedList.Recycle(ref _cachedRowViews, rowView);
        }

        public sealed override RowPresenter EditingRow
        {
            internal set
            {
                var oldValue = base.EditingRow;
                if (oldValue == value)
                    return;

                if (oldValue != null && BlockViews.Contains(oldValue))
                    Virtualize(oldValue);

                if (value != null)
                    Realize(value);

                base.EditingRow = value;
            }
        }

        internal IElementCollection ElementCollection { get; private set; }

        public IReadOnlyList<UIElement> Elements
        {
            get { return ElementCollection; }
        }

        internal int BlockViewStartIndex { get; private set; }

        internal void SetElementsPanel(FrameworkElement elementsPanel)
        {
            Debug.Assert(elementsPanel != null);

            if (ElementCollection != null)
                ClearElements();
            InitializeElements(elementsPanel);
        }

        internal void InitializeElements(FrameworkElement elementsPanel)
        {
            Debug.Assert(ElementCollection == null);

            ElementCollection = ElementCollectionFactory.Create(elementsPanel);

            var scalarItems = Template.ScalarItems;
            for (int i = 0; i < scalarItems.Count; i++)
                InsertDataElementsAfter(scalarItems[i], Elements.Count - 1, 1);
            BlockViewStartIndex = Template.ScalarItemsSplit;
        }

        private void RefreshElements()
        {
            if (Elements.Count == 0)
                return;

            RefreshDataElements();
            RefreshBlocks();

            _isDirty = false;
        }

        private void RefreshDataElements()
        {
            var scalarItems = Template.ScalarItems;
            foreach (var scalarItem in scalarItems)
            {
                for (int i = 0; i < scalarItem.BlockDimensions; i++)
                {
                    var element = scalarItem[i];
                    scalarItem.Refresh(element);
                }
            }
        }

        private void RefreshBlocks()
        {
            var count = BlockViews.Count;
            for (int i = 0; i < count; i++)
            {
                var blockView = BlockViews[i];
                blockView.RefreshElements();
            }
        }

        internal void ClearElements()
        {
            if (ElementCollection == null)
                return;

            BlockViews.VirtualizeAll();
            var scalarItems = Template.ScalarItems;
            for (int i = 0; i < scalarItems.Count; i++)
            {
                var scalarItem = scalarItems[i];
                scalarItem.CumulativeBlockDimensionsDelta = 0;
                int count = scalarItem.IsMultidimensional ? BlockDimensions : 1;
                RemoveDataElementsAfter(scalarItem, -1, count);
            }
            Debug.Assert(Elements.Count == 0);
            _blockDimensions = 1;
            ElementCollection = null;
        }

        private int InsertDataElementsAfter(ScalarItem scalarItem, int index, int count)
        {
            for (int i = 0; i < count; i++)
                scalarItem.Mount(x => ElementCollection.Insert(index + i + 1, x));
            return index + count;
        }

        private void RemoveDataElementsAfter(ScalarItem scalarItem, int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var element = Elements[index + 1];
                Debug.Assert(element.GetTemplateItem() == scalarItem);
                scalarItem.Unmount(element);
                ElementCollection.RemoveAt(index + 1);
            }
        }

        private int _blockDimensions = 1;
        internal int BlockDimensions
        {
            get { return _blockDimensions; }
            set
            {
                Debug.Assert(value >= 1);

                if (_blockDimensions == value)
                    return;

                var delta = value - _blockDimensions;
                _blockDimensions = value;
                OnBlockDimensionsChanged(delta);
            }
        }

        private void OnBlockDimensionsChanged(int blockDimensionsDelta)
        {
            Debug.Assert(blockDimensionsDelta != 0);

            BlockViews.VirtualizeAll();

            var index = -1;
            var delta = 0;
            var scalarItems = Template.ScalarItems;
            for (int i = 0; i < scalarItems.Count; i++)
            {
                index++;
                var scalarItem = scalarItems[i];

                var prevCumulativeBlockDimensionsDelta = i == 0 ? 0 : scalarItems[i - 1].CumulativeBlockDimensionsDelta;
                if (!scalarItem.IsMultidimensional)
                {
                    scalarItem.CumulativeBlockDimensionsDelta = prevCumulativeBlockDimensionsDelta + (BlockDimensions - 1);
                    continue;
                }
                scalarItem.CumulativeBlockDimensionsDelta = prevCumulativeBlockDimensionsDelta;

                if (i < Template.ScalarItemsSplit)
                    delta += blockDimensionsDelta;

                if (blockDimensionsDelta > 0)
                    index = InsertDataElementsAfter(scalarItem, index, blockDimensionsDelta);
                else
                    RemoveDataElementsAfter(scalarItem, index, -blockDimensionsDelta);
            }

            BlockViewStartIndex += delta;
        }

        private bool _isDirty;
        internal sealed override void Invalidate(RowPresenter row)
        {
            if (_isDirty || ElementCollection == null)
                return;

            if (row == null || (BlockViews.Contains(row) && row.Elements != null))
            {
                _isDirty = true;
                BeginRefreshElements();
            }
        }

        private void BeginRefreshElements()
        {
            Debug.Assert(ElementCollection != null && _isDirty);

            var panel = ElementCollection.Parent;
            if (panel == null)
                RefreshElements();
            else
            {
                panel.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    RefreshElements();
                }));
            }
        }
    }
}
