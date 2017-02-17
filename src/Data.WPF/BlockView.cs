using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Collections;

namespace DevZest.Data.Windows
{
    [TemplatePart(Name = "PART_Panel", Type = typeof(BlockViewPanel))]
    public class BlockView : ContainerView, IReadOnlyList<RowPresenter>
    {
        static BlockView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BlockView), new FrameworkPropertyMetadata(typeof(BlockView)));
        }

        public BlockView()
        {
        }

        internal sealed override void Setup(ElementManager elementManager, int ordinal)
        {
            _elementManager = elementManager;
            _ordinal = ordinal;
            SetupElements();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template == null)
                return;

            var panel = Template.FindName("PART_Panel", this) as BlockViewPanel;
            if (panel != null)
                Setup(panel);
        }

        internal void Setup(FrameworkElement elementsPanel)
        {
            if (ElementCollection != null)
            {
                if (ElementCollection.Parent == elementsPanel)
                    return;

                CleanupElements();
            }

            ElementCollection = ElementCollectionFactory.Create(elementsPanel);
            SetupElements();
        }

        private void SetupElements()
        {
            if (ElementManager == null || ElementCollection == null)
                return;

            AddElements();
            OnSetup();
        }

        internal sealed override void Cleanup()
        {
            CleanupElements();
            _elementManager = null;
        }

        private void CleanupElements()
        {
            OnCleanup();
            ClearElements();
        }

        private ElementManager _elementManager;
        internal sealed override ElementManager ElementManager
        {
            get { return _elementManager; }
        }

        public int Dimensions
        {
            get { return ElementManager == null ? 1 : ElementManager.BlockDimensions; }
        }

        private int GetDimensions()
        {
            return ElementManager == null ? 1 : ElementManager.BlockDimensions;
        }

        private int _ordinal;
        public sealed override int ContainerOrdinal
        {
            get { return _ordinal; }
        }

        public int Count
        {
            get
            {
                if (ElementManager == null)
                    return 0;

                var blockDimensions = ElementManager.BlockDimensions;
                var nextBlockFirstRowOrdinal = (ContainerOrdinal + 1) * blockDimensions;
                var rowCount = ElementManager.Rows.Count;
                return nextBlockFirstRowOrdinal <= rowCount ? blockDimensions : blockDimensions - (nextBlockFirstRowOrdinal - rowCount);
            }
        }

        public RowPresenter this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return ElementManager.Rows[ContainerOrdinal * ElementManager.BlockDimensions + index];
            }
        }

        public IEnumerator<RowPresenter> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private RecapBindingCollection<BlockBinding> BlockBindings
        {
            get { return ElementManager.Template.InternalBlockBindings; }
        }

        private int BlockBindingsSplit
        {
            get { return ElementManager.Template.BlockBindingsSplit; }
        }

        private IElementCollection ElementCollection { get; set; }

        internal IReadOnlyList<UIElement> Elements
        {
            get { return ElementCollection; }
        }

        private void AddElements()
        {
            var blockBindings = BlockBindings;

            blockBindings.BeginSetup();

            for (int i = 0; i < BlockBindingsSplit; i++)
                AddElement(blockBindings[i]);

            for (int i = 0; i < ElementManager.BlockDimensions; i++)
            {
                var success = AddRowView(i);
                if (!success)   // Exceeded the total count of the rows
                    break;
            }

            for (int i = BlockBindingsSplit; i < BlockBindings.Count; i++)
                AddElement(blockBindings[i]);

            blockBindings.EndSetup();
        }

        private void AddElement(BlockBinding blockBinding)
        {
            var element = blockBinding.Setup(this);
            ElementCollection.Add(element);
        }

        private bool AddRowView(int offset)
        {
            var rows = ElementManager.Rows;
            var rowIndex = ContainerOrdinal * ElementManager.BlockDimensions + offset;
            if (rowIndex >= rows.Count)
                return false;
            var row = rows[rowIndex];
            var rowView = ElementManager.Setup(this, row);
            ElementCollection.Insert(BlockBindingsSplit + offset, rowView);
            return true;
        }

        private void ClearElements()
        {
            if (ElementCollection == null)
                return;

            int blockDimensions = Elements.Count - BlockBindings.Count;

            var blockBindings = BlockBindings;
            for (int i = BlockBindings.Count - 1; i >= BlockBindingsSplit; i--)
                RemoveLastElement(blockBindings[i]);

            for (int i = blockDimensions - 1; i >= 0; i--)
                RemoveRowViewAt(Elements.Count - 1);

            for (int i = BlockBindingsSplit - 1; i >= 0 ; i--)
                RemoveLastElement(blockBindings[i]);
        }

        private void RemoveLastElement(BlockBinding blockBinding)
        {
            var lastIndex = Elements.Count - 1;
            var element = Elements[lastIndex];
            blockBinding.Cleanup(element);
            RemoveAt(lastIndex);
        }

        private void RemoveRowViewAt(int index)
        {
            var rowView = (RowView)Elements[index];
            ElementManager.Cleanup(rowView.RowPresenter);
            RemoveAt(index);
        }

        private void RemoveAt(int index)
        {
            ElementCollection.RemoveAt(index);
        }

        internal sealed override void Refresh()
        {
            if (Elements == null)
                return;

            var blockBindings = BlockBindings;
            int blockDimensions = Elements.Count - blockBindings.Count;
            var index = 0;

            for (int i = 0; i < BlockBindingsSplit; i++)
                Refresh(blockBindings[i], index++);

            for (int i = 0; i < blockDimensions; i++)
                ((RowView)Elements[index++]).Refresh();

            for (int i = BlockBindingsSplit; i < BlockBindings.Count; i++)
                Refresh(blockBindings[i], index++);

            OnRefresh();
        }

        private void Refresh(BlockBinding blockBinding, int index)
        {
            var element = Elements[index];
            blockBinding.Refresh(element);
        }

        protected virtual void OnRefresh()
        {
        }

        private bool Contains(RowPresenter rowPresenter)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i] == rowPresenter)
                    return true;
            }

            return false;
        }

        internal sealed override void ReloadCurrentRow(RowPresenter oldValue)
        {
            if (Elements == null)
                return;

            Debug.Assert(ElementManager.CurrentContainerView == this && ElementManager.CurrentContainerViewPosition == CurrentContainerViewPosition.Alone);

            var currentRowView = RemoveAllRowViewsExcept(oldValue);
            currentRowView.ReloadCurrentRow(oldValue);
            FillMissingRowViews(currentRowView);
            var newValue = ElementManager.CurrentRow;
            _ordinal = newValue.Index / ElementManager.BlockDimensions;
            Refresh();
        }

        internal sealed override bool ShouldReloadCurrentRow
        {
            get
            {
                if (Elements == null)
                    return false;

                var startRowIndex = ContainerOrdinal * ElementManager.BlockDimensions;
                var startIndex = BlockBindingsSplit;
                int blockDimensions = Elements.Count - BlockBindings.Count;
                for (int i = 0; i < blockDimensions; i++)
                {
                    var index = startIndex + i;
                    var rowView = (RowView)Elements[index];
                    if (rowView.RowPresenter.Index != startRowIndex + i)
                        return true;
                }
                return false;
            }
        }

        private RowView RemoveAllRowViewsExcept(RowPresenter row)
        {
            OnCleanup();

            RowView result = null;
            var startIndex = BlockBindingsSplit;
            int blockDimensions = Elements.Count - BlockBindings.Count;
            for (int i = blockDimensions - 1; i >= 0; i--)
            {
                var index = startIndex + i;
                var rowView = (RowView)Elements[index];
                if (rowView.RowPresenter == row)
                    result = rowView;
                else
                    RemoveRowViewAt(index);
            }
            Debug.Assert(result != null);
            return result;
        }

        private void FillMissingRowViews(RowView currentRowView)
        {
            var currentRowIndex = currentRowView.RowPresenter.Index;
            var blockDimensions = ElementManager.BlockDimensions;
            var offset = currentRowIndex % blockDimensions;

            for (int i = 0; i < offset; i++)
                AddRowView(i);

            for (int i = offset + 1; i < blockDimensions; i++)
            {
                var success = AddRowView(i);
                if (!success)   // Exceeded the total count of the rows
                    break;
            }

            OnSetup();
        }

        internal UIElement this[BlockBinding blockBinding]
        {
            get
            {
                if (Elements == null)
                    return null;

                var index = blockBinding.Ordinal;
                if (index >= BlockBindingsSplit)
                    index += Count;
                return Elements[index];
            }
        }
    }
}
