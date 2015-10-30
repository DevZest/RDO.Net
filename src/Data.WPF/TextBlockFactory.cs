using DevZest.Data.Primitives;
using System.Windows.Controls;
using System;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    public static class TextBlockFactory
    {
        private sealed class ColumnValueGridItem<TValue, T> : SetGridItem<T>
            where T : TextBlock, new()
        {
            public ColumnValueGridItem(Column<TValue> column, Func<TValue, string> converter, Action<T> initializer)
                : base(column.GetParentModel(), initializer)
            {
                Debug.Assert(column != null);
                Debug.Assert(converter != null);
                _column = column;
                _converter = converter;
            }

            private Column<TValue> _column;
            private Func<TValue, string> _converter;

            protected override void Refresh(DataRowView dataRowView, T uiElement)
            {
                uiElement.Text = _converter(_column[dataRowView.DataRow]);
            }
        }
    }
}
