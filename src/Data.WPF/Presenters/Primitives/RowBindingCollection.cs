namespace DevZest.Data.Presenters.Primitives
{
    internal sealed class RowBindingCollection : BindingCollection<RowBinding>
    {
        private IConcatList<RowBinding> _autoSizeItems;

        internal IConcatList<RowBinding> AutoSizeItems
        {
            get { return _autoSizeItems ?? (_autoSizeItems = FilterAutoSizeBindings(x => x.IsAutoSize).Seal()); }
        }

        internal override void InvalidateAutoHeightBindings()
        {
            _autoSizeItems = null;
            base.InvalidateAutoHeightBindings();
        }

        internal override void InvalidateAutoWidthBindings()
        {
            _autoSizeItems = null;
            base.InvalidateAutoWidthBindings();
        }
    }
}
