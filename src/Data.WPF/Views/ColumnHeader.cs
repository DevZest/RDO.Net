using DevZest.Data.Presenters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace DevZest.Data.Views
{
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateNormal)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateMouseOver)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StatePressed)]
    [TemplateVisualState(GroupName = VisualStates.GroupSort, Name = VisualStates.StateUnsorted)]
    [TemplateVisualState(GroupName = VisualStates.GroupSort, Name = VisualStates.StateSortAscending)]
    [TemplateVisualState(GroupName = VisualStates.GroupSort, Name = VisualStates.StateSortDescending)]
    public class ColumnHeader : ButtonBase
    {
        private interface IColumnSource
        {
            Column Column { get; }
            IColumnComparer GetColumnComparer(SortDirection direction);
        }

        private sealed class ColumnSource<T> : IColumnSource
        {
            public ColumnSource(Column<T> column, IComparer<T> comparer)
            {
                _column = column;
                _comparer = comparer;
            }

            private readonly Column<T> _column;
            private readonly IComparer<T> _comparer;

            public Column Column
            {
                get { return _column; }
            }

            public IColumnComparer GetColumnComparer(SortDirection direction)
            {
                return DataRow.OrderBy(_column, direction, _comparer);
            }
        }

        private sealed class ColumnSource : IColumnSource
        {
            public ColumnSource(Column column)
            {
                _column = column;
            }

            private readonly Column _column;

            public Column Column
            {
                get { return _column; }
            }

            public IColumnComparer GetColumnComparer(SortDirection direction)
            {
                return DataRow.OrderBy(_column, direction);
            }
        }

        public static readonly RoutedUICommand ShowSortWindowCommand = new RoutedUICommand(UIText.ColumnHeader_ShowSortWindowCommandText, nameof(ShowSortWindowCommand), typeof(ColumnHeader));

        public static readonly DependencyProperty SeparatorBrushProperty = DependencyProperty.Register(nameof(SeparatorBrush), typeof(Brush),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SeparatorVisibilityProperty = DependencyProperty.Register("SeparatorVisibility", typeof(Visibility),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(Visibility.Visible));

        static ColumnHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColumnHeader), new FrameworkPropertyMetadata(typeof(ColumnHeader)));
        }

        private IColumnSource _columnSource;

        public Column Column
        {
            get { return _columnSource?.Column; }
        }

        public void Setup(Column column, object title)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            _columnSource = new ColumnSource(column);
            Content = title;
        }

        public void Setup<T>(Column<T> column, IComparer<T> comparer, object title)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            _columnSource = new ColumnSource<T>(column, comparer);
            Content = title;
        }

        public IColumnComparer GetColumnComparer(SortDirection direction)
        {
            return _columnSource?.GetColumnComparer(direction);
        }

        public static IReadOnlyList<ColumnHeader> GetColumnHeaders(DataPresenter dataPresenter)
        {
            Debug.Assert(dataPresenter != null);

            List<ColumnHeader> result = null;
            var bindings = dataPresenter.Template.ScalarBindings;
            for (int i = 0; i < bindings.Count; i++)
            {
                var columnHeader = bindings[i][0] as ColumnHeader;
                if (columnHeader != null && columnHeader.Column != null)
                {
                    if (result == null)
                        result = new List<ColumnHeader>();
                    result.Add(columnHeader);
                }
            }

            if (result == null)
                return Array<ColumnHeader>.Empty;
            else
                return result;
        }

        public Brush SeparatorBrush
        {
            get { return (Brush)GetValue(SeparatorBrushProperty); }
            set { SetValue(SeparatorBrushProperty, value); }
        }

        public Visibility SeparatorVisibility
        {
            get { return (Visibility)GetValue(SeparatorVisibilityProperty); }
            set { SetValue(SeparatorVisibilityProperty, value); }
        }
    }
}
