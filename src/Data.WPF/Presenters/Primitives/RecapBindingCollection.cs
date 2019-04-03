using System.Collections.Generic;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>Collection container for <see cref="ScalarBinding"/> and <see cref="BlockBinding"/>.</summary>
    /// <typeparam name="T"><see cref="ScalarBinding"/> or <see cref="BlockBinding"/>.</typeparam>
    internal sealed class RecapBindingCollection<T> : BindingCollection<T>
        where T : Binding, IConcatList<T>
    {
        private IReadOnlyList<T> _preAutoSizeBindings;
        internal IReadOnlyList<T> PreAutoSizeBindings
        {
            get { return _preAutoSizeBindings ?? (_preAutoSizeBindings = FilterAutoSizeBindings(x => x.IsAutoSize && x.AutoSizeOrder <= 0).Seal()); }
        }

        private IReadOnlyList<T> _postAutoSizeBindings;
        internal IReadOnlyList<T> PostAutoSizeBindings
        {
            get { return _postAutoSizeBindings ?? (_postAutoSizeBindings = FilterAutoSizeBindings(x => x.IsAutoSize && x.AutoSizeOrder > 0).Seal()); }
        }

        internal override void InvalidateAutoHeightBindings()
        {
            _preAutoSizeBindings = _postAutoSizeBindings = null;
            base.InvalidateAutoHeightBindings();
        }

        internal override void InvalidateAutoWidthBindings()
        {
            _preAutoSizeBindings = _postAutoSizeBindings = null;
            base.InvalidateAutoWidthBindings();
        }
    }
}
