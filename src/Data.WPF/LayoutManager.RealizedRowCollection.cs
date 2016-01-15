using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    partial class LayoutManager
    {
        private class RealizedRowCollection : IReadOnlyList<RowView>
        {
            public RealizedRowCollection(LayoutManager layoutManager)
            {
                Debug.Assert(layoutManager != null);
                _layoutManager = layoutManager;
            }

            private LayoutManager _layoutManager;

            private GridTemplate Template
            {
                get { return _layoutManager.Template; }
            }

            private IReadOnlyList<UIElement> Elements
            {
                get { return _layoutManager.Elements; }
            }

            private IElementCollection Children
            {
                get { return _layoutManager.Children; }
            }

            public int Count
            {
                get { return Elements.Count - Template.ScalarUnits.Count; }
            }

            public RowView this[int index]
            {
                get
                {
                    Debug.Assert(index >= 0 && index < Count);
                    var rowForm = (RowForm)Elements[index + Template.NumberOfScallarUnitsBeforeRow];
                    return rowForm.View;
                }
            }

            public IEnumerator<RowView> GetEnumerator()
            {
                for (int i = 0; i < Count; i++)
                    yield return this[i];
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            Func<RowForm> _rowFormConstructor = () => new RowForm();
            public void InitRowFormConstructor(Func<RowForm> rowFormConstructor)
            {
                _rowFormConstructor = rowFormConstructor;
            }

            List<RowForm> _cachedRowForms;
            private RowForm GenerateRowForm()
            {
                if (_cachedRowForms == null || _cachedRowForms.Count == 0)
                    return _rowFormConstructor();

                var last = _cachedRowForms.Count - 1;
                var result = _cachedRowForms[last];
                _cachedRowForms.RemoveAt(last);
                return result;
            }

            private void RecycleRowForm(RowForm rowForm)
            {
                Debug.Assert(rowForm != null);

                if (_cachedRowForms == null)
                    _cachedRowForms = new List<RowForm>();
                _cachedRowForms.Add(rowForm);
            }

            public void Add(RowView row)
            {
                Debug.Assert(row != null && row.Form == null);

                int index;
                if (Count == 0 || row.Index == this[0].Index - 1)
                    index = Template.NumberOfScallarUnitsBeforeRow;
                else
                {
                    Debug.Assert(row.Index == this[Count - 1].Index + 1);
                    index = Template.NumberOfScallarUnitsBeforeRow + Count;
                }

                var rowForm = GenerateRowForm();
                rowForm.Initialize(row);
                row.Form = rowForm;
                Children.Insert(index, rowForm);
            }

            public void RemoveRange(int startIndex, int count)
            {
                for (int i = startIndex; i < count; i++)
                {
                    var row = this[i];
                    var rowForm = this[i].Form;
                    rowForm.Cleanup();
                    row.Form = null;
                }

                int startChildrenIndex = startIndex + Template.NumberOfScallarUnitsBeforeRow;
                Children.RemoveRange(startChildrenIndex, count);
            }

            public void RemoveAll()
            {
                RemoveRange(0, Count);
            }
        }
    }
}
