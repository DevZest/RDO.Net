using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Views
{
    /// <summary>
    /// Interaction logic for PasteAppendWindow.xaml
    /// </summary>
    internal partial class PasteAppendWindow : Window
    {
        private sealed class Presenter : DataPresenter<TabularText>
        {
            private struct Indexer<T>
            {
                public Indexer(IReadOnlyList<T> values, int index)
                {
                    Values = values;
                    Index = index;
                }

                public readonly IReadOnlyList<T> Values;
                public readonly int Index;

                public T GetValue()
                {
                    return Values[Index];
                }
            }

            public Presenter(IReadOnlyList<Column> targetColumns, DataView dataView)
            {
                Debug.Assert(targetColumns != null && targetColumns.Count > 0);
                _columnSelection = InitColumnSelection(targetColumns);
                BindableIsFirstRowHeader = NewLinkedScalar<bool>(nameof(IsFirstRowHeader));

                var tabularText = TabularText.PasteFromClipboard();
                _hasData = tabularText.Count > 0;
                var textColumnsCount = tabularText._.TextColumns.Count;
                _headers = new string[textColumnsCount];
                InitHeaders(null);
                _bindableHeaders = new Func<string>[textColumnsCount];
                for (int i = 0; i < _bindableHeaders.Length; i++)
                    _bindableHeaders[i] = new Indexer<string>(_headers, i).GetValue;
                _columnMappings = new Scalar<Column>[textColumnsCount];
                for (int i = 0; i < _columnMappings.Length; i++)
                    _columnMappings[i] = NewScalar<Column>();
                _isFirstRowHeader = InitColumnMappings(tabularText, targetColumns);
                if (_isFirstRowHeader)
                {
                    InitHeaders(tabularText._.TextColumns);
                    tabularText.RemoveAt(0);
                }
                Show(dataView, tabularText);
            }

            private ColumnSelectionItem[] InitColumnSelection(IReadOnlyList<Column> targetColumns)
            {
                var result = new ColumnSelectionItem[targetColumns.Count + 1];
                for (int i = 0; i < targetColumns.Count; i++)
                    result[i] = new ColumnSelectionItem(targetColumns[i], targetColumns[i].DisplayName);
                result[result.Length - 1] = new ColumnSelectionItem(_ignore, "[Ignored]");
                return result;
            }

            private void InitHeaders(IReadOnlyList<Column<string>> columns)
            {
                for (int i = 0; i < _headers.Length; i++)
                    _headers[i] = columns == null ? "Column" + (i + 1) : columns[i][0];
            }

            private bool InitColumnMappings(DataSet<TabularText> tabularText, IReadOnlyList<Column> targetColumns)
            {
                if (InitColumnMappingsByFirstRowHeader(tabularText, targetColumns))
                    return true;

                var count = Math.Min(targetColumns.Count, tabularText._.TextColumns.Count);
                for (int i = 0; i < count; i++)
                    _columnMappings[i].SetValue(targetColumns[i]);
                return false;
            }

            private bool InitColumnMappingsByFirstRowHeader(DataSet<TabularText> tabularText, IReadOnlyList<Column> targetColumns)
            {
                if (tabularText.Count == 0)
                    return false;

                var columnsMatched = 0;
                var textColumns = tabularText._.TextColumns;
                for (int i = 0; i < _columnMappings.Length; i++)
                {
                    var header = textColumns[i][0];
                    foreach (var column in targetColumns)
                    {
                        if (!string.IsNullOrEmpty(header) && column.DisplayName == header)
                        {
                            _columnMappings[i].SetValue(column);
                            columnsMatched++;
                        }
                    }
                }

                return columnsMatched > 0;
            }

            private sealed class ColumnSelectionItem
            {
                public ColumnSelectionItem(Column column, string display)
                {
                    Column = column;
                    Display = display;
                }

                public Column Column { get; private set; }
                public string Display { get; private set; }
            }

            private readonly _String _ignore = new _String();
            private readonly ColumnSelectionItem[] _columnSelection;
            private readonly Scalar<Column>[] _columnMappings;
            private readonly string[] _headers;
            private readonly Func<string>[] _bindableHeaders;
            public readonly Scalar<bool> BindableIsFirstRowHeader;

            private bool _isFirstRowHeader;
            private bool IsFirstRowHeader
            {
                get { return _isFirstRowHeader; }
                set
                {
                    if (_isFirstRowHeader == value)
                        return;

                    SuspendInvalidateView();
                    _isFirstRowHeader = value;
                    if (_hasData)
                    {
                        if (value)
                        {
                            InitHeaders(_.TextColumns);
                            DataSet.RemoveAt(0);
                        }
                        else
                        {
                            DataSet.Insert(0, (_, dataRow) =>
                            {
                                for (int i = 0; i < _headers.Length; i++)
                                    _.TextColumns[i][dataRow] = _headers[i];
                            });
                            InitHeaders(null);
                        }
                    }
                    ResumeInvalidateView();
                }
            }

            private readonly bool _hasData;

            private bool AreHeadersVisible
            {
                get { return _hasData && IsFirstRowHeader; }
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var textColumns = _.TextColumns;

                builder.GridColumns(textColumns.Select(x => "100").ToArray())
                    .GridRows("Auto", "Auto", "Auto")
                    .Layout(Orientation.Vertical);

                for (int i = 0; i < _headers.Length; i++)
                    builder.AddBinding(i, 0, _bindableHeaders[i].BindToColumnHeader());

                for (int i = 0; i < _columnMappings.Length; i++)
                    builder.AddBinding(i, 1, _columnMappings[i].BindToComboBox(_columnSelection, nameof(ColumnSelectionItem.Column), nameof(ColumnSelectionItem.Display)));

                for (int i = 0; i < textColumns.Count; i++)
                    builder.AddBinding(i, 2, textColumns[i].BindToTextBlock().AddToGridCell());
            }
        }

        public PasteAppendWindow()
        {
            InitializeComponent();
        }

        private Presenter _presenter;
        public IReadOnlyList<ColumnValueBag> Show(IReadOnlyList<Column> columns)
        {
            _presenter = new Presenter(columns, _dataView);
            _presenter.Attach(_isFirstRowHeader, _presenter.BindableIsFirstRowHeader.BindToCheckBox());
            ShowDialog();
            return null;
        }
    }
}
