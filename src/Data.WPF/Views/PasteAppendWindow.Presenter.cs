using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Views
{
    partial class PasteAppendWindow
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
                _columnSelections = InitColumnSelection(targetColumns);
                BindableFirstRowContainsColumnHeadings = NewLinkedScalar<bool>(nameof(FirstRowContainsColumnHeadings));

                var tabularText = TabularText.PasteFromClipboard();
                _hasData = tabularText.Count > 0;
                var textColumnsCount = tabularText._.TextColumns.Count;
                _columnHeadings = new string[textColumnsCount];
                InitHeaders(null);
                _bindableColumnHeadings = new Func<string>[textColumnsCount];
                for (int i = 0; i < _bindableColumnHeadings.Length; i++)
                    _bindableColumnHeadings[i] = new Indexer<string>(_columnHeadings, i).GetValue;
                _columnMappings = new Scalar<Column>[textColumnsCount];
                for (int i = 0; i < _columnMappings.Length; i++)
                    _columnMappings[i] = NewScalar<Column>();
                _firstRowContainsColumnHeadings = InitColumnMappings(tabularText, targetColumns);
                if (_firstRowContainsColumnHeadings)
                {
                    InitHeaders(tabularText._.TextColumns);
                    tabularText.RemoveAt(0);
                }
                Show(dataView, tabularText);
            }

            private ColumnSelection[] InitColumnSelection(IReadOnlyList<Column> targetColumns)
            {
                var result = new ColumnSelection[targetColumns.Count + 1];
                for (int i = 0; i < targetColumns.Count; i++)
                    result[i] = new ColumnSelection(targetColumns[i], targetColumns[i].DisplayName);
                result[result.Length - 1] = new ColumnSelection(_ignore, UserMessages.PasteAppendWindow_Ignore);
                return result;
            }

            private void InitHeaders(IReadOnlyList<Column<string>> columns)
            {
                for (int i = 0; i < _columnHeadings.Length; i++)
                    _columnHeadings[i] = columns == null ? UserMessages.PasteAppendWindow_ColumnHeading(i + 1) : columns[i][0];
            }

            private bool InitColumnMappings(DataSet<TabularText> tabularText, IReadOnlyList<Column> targetColumns)
            {
                if (InitColumnMappingsByFirstRowHeader(tabularText, targetColumns))
                    return true;

                var count = Math.Min(targetColumns.Count, tabularText._.TextColumns.Count);
                for (int i = 0; i < count; i++)
                    _columnMappings[i].Value = targetColumns[i];
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

            private sealed class ColumnSelection
            {
                public ColumnSelection(Column column, string display)
                {
                    Column = column;
                    Display = display;
                }

                public Column Column { get; private set; }
                public string Display { get; private set; }
            }

            private static readonly Column _ignore = new _String();
            private readonly ColumnSelection[] _columnSelections;
            private readonly Scalar<Column>[] _columnMappings;
            private readonly string[] _columnHeadings;
            private readonly Func<string>[] _bindableColumnHeadings;
            public readonly Scalar<bool> BindableFirstRowContainsColumnHeadings;

            private bool _firstRowContainsColumnHeadings;
            private bool FirstRowContainsColumnHeadings
            {
                get { return _firstRowContainsColumnHeadings; }
                set
                {
                    if (_firstRowContainsColumnHeadings == value)
                        return;

                    SuspendInvalidateView();
                    _firstRowContainsColumnHeadings = value;
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
                                for (int i = 0; i < _columnHeadings.Length; i++)
                                    _.TextColumns[i][dataRow] = _columnHeadings[i];
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
                get { return _hasData && FirstRowContainsColumnHeadings; }
            }

            private class TextBlockGrayOut
            {
                public TextBlockGrayOut(Scalar<Column> scalar)
                {
                    _scalar = scalar;
                }

                private readonly Scalar<Column> _scalar;

                public void Refresh(TextBlock v, RowPresenter p)
                {
                    if (_scalar.Value == _ignore)
                        v.Foreground = Brushes.LightGray;
                    else
                        v.ClearValue(ForegroundProperty);
                }
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var textColumns = _.TextColumns;

                builder.GridColumns(textColumns.Select(x => "100;min:20;max:200").ToArray())
                    .GridRows("Auto", "Auto", "Auto")
                    .Layout(Orientation.Vertical);

                for (int i = 0; i < _columnHeadings.Length; i++)
                    builder.AddBinding(i, 0, _bindableColumnHeadings[i].BindToColumnHeader());

                for (int i = 0; i < _columnMappings.Length; i++)
                    builder.AddBinding(i, 1, _columnMappings[i].BindToComboBox(_columnSelections, nameof(ColumnSelection.Column), nameof(ColumnSelection.Display)));

                for (int i = 0; i < textColumns.Count; i++)
                {
                    var textBlockGrayOut = new TextBlockGrayOut(_columnMappings[i]);
                    builder.AddBinding(i, 2, textColumns[i].BindToTextBlock().OverrideRefresh(textBlockGrayOut.Refresh).AddToGridCell());
                    builder.GridLineY(new GridPoint(i + 1, 2), 1);
                }

                builder.GridLineX(new GridPoint(0, 3), textColumns.Count);
            }

            protected override IScalarValidationErrors ValidateScalars(IScalarValidationErrors result)
            {
                for (int i = 0; i < _columnMappings.Length; i++)
                {
                    var scalar = _columnMappings[i];
                    var column = scalar.Value;

                    if (column == _ignore)
                        continue;

                    if (column == null)
                    {
                        result = result.Add(new ScalarValidationError(UserMessages.PasteAppendWindow_InputRequired, scalar));
                        continue;
                    }

                    for (int j = 0; j < i; j++)
                    {
                        if (column == _columnMappings[j].Value)
                        {
                            result = result.Add(new ScalarValidationError(UserMessages.PasteAppendWindow_DuplicateValueNotAllowed, scalar));
                            continue;
                        }
                    }
                }
                return result;
            }
        }
    }
}
