using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class ElementManager : RowManager
    {
        internal ElementManager(Template template, DataSet dataSet)
            : base(template, dataSet)
        {
            BlockViews = new BlockViewCollection(this);
        }

        internal BlockViewCollection BlockViews { get; private set; }

        public IReadOnlyList<IBlockPresenter> Blocks
        {
            get { return BlockViews; }
        }

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
            Debug.Assert(ElementCollection == null && !BlockViews.IsInitialized);

            ElementCollection = ElementCollectionFactory.Create(elementsPanel);

            var dataItems = Template.DataItems;
            for (int i = 0; i < dataItems.Count; i++)
                InsertDataElementsAfter(dataItems[i], Elements.Count - 1, 1);
            BlockViewStartIndex = Template.DataItemsSplit;
        }

        internal void RefreshElements()
        {
            if (Elements.Count == 0)
                return;

            RefreshDataElements();
            RefreshBlocks();

            _isDirty = false;
        }

        private void RefreshDataElements()
        {
            var dataItems = Template.DataItems;
            foreach (var dataItem in dataItems)
            {
                for (int i = 0; i < dataItem.BlockDimensions; i++)
                {
                    var element = dataItem[i];
                    dataItem.UpdateTarget(element);
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
            var dataItems = Template.DataItems;
            for (int i = 0; i < dataItems.Count; i++)
            {
                var dataItem = dataItems[i];
                dataItem.CumulativeBlockDimensionsDelta = 0;
                int count = dataItem.IsMultidimensional ? BlockDimensions : 1;
                RemoveDataElementsAfter(dataItem, -1, count);
            }
            Debug.Assert(Elements.Count == 0);
            _blockDimensions = 1;
            ElementCollection = null;
        }

        private int InsertDataElementsAfter(DataItem dataItem, int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var element = dataItem.Generate();
                ElementCollection.Insert(index + i + 1, element);
                dataItem.Initialize(element);
            }
            return index + count;
        }

        private void RemoveDataElementsAfter(DataItem dataItem, int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var element = Elements[index + 1];
                Debug.Assert(element.GetTemplateItem() == dataItem);
                dataItem.Cleanup(element);
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
            var dataItems = Template.DataItems;
            for (int i = 0; i < dataItems.Count; i++)
            {
                index++;
                var dataItem = dataItems[i];

                var prevCumulativeBlockDimensionsDelta = i == 0 ? 0 : dataItems[i - 1].CumulativeBlockDimensionsDelta;
                if (!dataItem.IsMultidimensional)
                {
                    dataItem.CumulativeBlockDimensionsDelta = prevCumulativeBlockDimensionsDelta + (BlockDimensions - 1);
                    continue;
                }
                dataItem.CumulativeBlockDimensionsDelta = prevCumulativeBlockDimensionsDelta;

                if (i < Template.DataItemsSplit)
                    delta += blockDimensionsDelta;

                if (blockDimensionsDelta > 0)
                    index = InsertDataElementsAfter(dataItem, index, blockDimensionsDelta);
                else
                    RemoveDataElementsAfter(dataItem, index, -blockDimensionsDelta);
            }

            BlockViewStartIndex += delta;
        }

        private bool _isDirty;
        protected sealed override void Invalidate(RowPresenter row)
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
