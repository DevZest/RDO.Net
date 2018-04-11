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

            public Presenter(DataPresenter sourcePresenter, IReadOnlyList<Column> targetColumns, DataView dataView)
            {
                Debug.Assert(sourcePresenter != null);
                Debug.Assert(targetColumns != null && targetColumns.Count > 0);
                _sourcePresenter = sourcePresenter;
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
                result[result.Length - 1] = new ColumnSelection(IGNORE, UserMessages.PasteAppendWindow_Ignore);
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

            private static readonly Column IGNORE = new _String();
            private readonly DataPresenter _sourcePresenter;
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
                    if (_scalar.Value == IGNORE)
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
                    .Layout(Orientation.Vertical)
                    .WithFrozenTop(2);

                for (int i = 0; i < _columnHeadings.Length; i++)
                    builder.AddBinding(i, 0, _bindableColumnHeadings[i].BindToColumnHeader());

                for (int i = 0; i < _columnMappings.Length; i++)
                    builder.AddBinding(i, 1, _columnMappings[i].BindToComboBox(_columnSelections, nameof(ColumnSelection.Column), nameof(ColumnSelection.Display)));

                for (int i = 0; i < textColumns.Count; i++)
                {
                    var textBlockGrayOut = new TextBlockGrayOut(_columnMappings[i]);
                    var textColumn = textColumns[i];
                    var gridCellBinding = textColumn.BindToTextBlock().OverrideRefresh(textBlockGrayOut.Refresh).AddToGridCell();
                    builder.AddBinding(i, 2, textColumn.BindToValidationPlaceholder(gridCellBinding));
                    builder.AddBinding(i, 2, gridCellBinding);
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

                    if (column == IGNORE)
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

            public ColumnValueBag[] Submit()
            {
                RowValidation.SetAsyncErrors(DataValidationResults.Empty);

                if (!SubmitInput())
                    return null;

                var result = new ColumnValueBag[DataSet.Count];
                var serializers = GetColumnSerializers();
                var validationResults = DataValidationResults.Empty;

                for (int i = 0; i <  DataSet.Count; i++)
                {
                    var validationErrors = DataValidationErrors.Empty;
                    var columnValueBag = new ColumnValueBag();
                    result[i] = columnValueBag;
                    for (int j = 0; j < serializers.Count; j++)
                    {
                        var serializer = serializers[j];
                        if (serializer == null)
                            continue;

                        var textColumn = _.TextColumns[j];
                        try
                        {
                            serializer.Deserialize(textColumn[i], columnValueBag);
                        }
                        catch (Exception ex)
                        {
                            validationErrors = validationErrors.Add(new DataValidationError(ex.Message, textColumn));
                        }
                    }

                    if (validationErrors.Count > 0)
                        validationResults = validationResults.Add(new DataValidationResult(DataSet[i], validationErrors));
                }

                if (validationResults.Count > 0)
                {
                    RowValidation.SetAsyncErrors(validationResults);
                    return null;
                }
                return result;
            }

            private IReadOnlyList<ColumnSerializer> GetColumnSerializers()
            {
                var result = new ColumnSerializer[_columnMappings.Length];
                for (int i = 0; i < _columnMappings.Length; i++)
                {
                    var column = _columnMappings[i].Value;
                    if (column == IGNORE)
                        continue;
                    result[i] = _sourcePresenter.GetSerializer(column);
                }

                return result;
            }
        }
    }
}
