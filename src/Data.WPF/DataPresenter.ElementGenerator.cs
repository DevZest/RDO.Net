using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    partial class DataPresenter
    {
        private sealed class ElementGenerator
        {
            Func<RowView> _rowViewConstructor = () => new RowView();
            public void InitRowViewConstructor(Func<RowView> rowViewConstructor)
            {
                _rowViewConstructor = rowViewConstructor;
            }

            List<RowView> _cachedRowViews;
            private RowView GenerateRowView()
            {
                if (_cachedRowViews == null || _cachedRowViews.Count == 0)
                    return _rowViewConstructor();

                var last = _cachedRowViews.Count - 1;
                var result = _cachedRowViews[last];
                _cachedRowViews.RemoveAt(last);
                return result;
            }

            private void RecycleRowView(RowView rowView)
            {
                Debug.Assert(rowView != null);

                if (_cachedRowViews == null)
                    _cachedRowViews = new List<RowView>();
                _cachedRowViews.Add(rowView);
            }

        }
    }
}
