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
                //Debug.Assert(targetColumns != null && targetColumns.Count > 0);
                _targetColumns = targetColumns;
                BindableIsFirstRowHeader = NewLinkedScalar<bool>(nameof(IsFirstRowHeader));

                var tabularText = TabularText.PasteFromClipboard();
                _hasData = tabularText.Count > 0;
                var textColumnsCount = tabularText._.TextColumns.Count;
                _headers = new string[textColumnsCount];
                InitHeaders(null);
                _bindableHeaders = new Func<string>[textColumnsCount];
                for (int i = 0; i < _bindableHeaders.Length; i++)
                    _bindableHeaders[i] = new Indexer<string>(_headers, i).GetValue;
                Show(dataView, tabularText);
                IsFirstRowHeader = true;
            }

            private void InitHeaders(IReadOnlyList<Column<string>> columns)
            {
                for (int i = 0; i < _headers.Length; i++)
                    _headers[i] = columns == null ? "Column" + (i + 1) : columns[i][0];
            }

            private readonly IReadOnlyList<Column> _targetColumns;
            private readonly Scalar<Column> _mappedColumns;
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
                    builder.AddBinding(i, 1,
                        _bindableHeaders[i].BindToColumnHeader().WithAutoSizeWaiver(AutoSizeWaiver.Width));

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
