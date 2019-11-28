using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace DevZest.Data.Tools
{
    partial class ProjectionWindow
    {
        private sealed class Presenter : DataPresenter<ModelMapper.ProjectionEntry>, IComparer<DataRow>
        {
            private readonly ModelMapper _modelMapper;
            private readonly Scalar<string> _typeName;

            public Presenter(ModelMapper modelMapper, DataView dataView, TextBox textBoxTypeName, string defaultTypeName, CheckBox checkBoxSortBySelection)
            {
                _textBoxTypeName = textBoxTypeName;
                _checkBoxSortBySelection = checkBoxSortBySelection;

                _modelMapper = modelMapper;
                _typeName = NewScalar(defaultTypeName).AddValidator(Extensions.ValidateRequired).AddValidator(modelMapper.ValidateIdentifier);
                Show(dataView, modelMapper.CreateProjectionEntries());
            }

            private readonly TextBox _textBoxTypeName;
            private readonly CheckBox _checkBoxSortBySelection;

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder.GridRows("Auto", "Auto")
                    .GridColumns("20", "*", "*")
                    .Layout(Orientation.Vertical)
                    .WithFrozenTop(1)
                    .AddBinding(0, 0, this.BindToCheckBox())
                    .AddBinding(1, 0, this.BindToTextBlock(_.Column.DisplayName))
                    .AddBinding(2, 0, this.BindToTextBlock(_.Mounter.DisplayName))
                    .AddBinding(0, 1, _.BindToCheckBox().ApplyRefresh((v, p) =>
                    {
                        v.IsEnabled = CanSelect(p);
                    }))
                    .AddBinding(1, 1, _.Column.BindToTextBox())
                    .AddBinding(2, 1, _.Mounter.BindToTextBox())
                    .AddBinding(_textBoxTypeName, _typeName.BindToTextBox())
                    .AddBinding(_checkBoxSortBySelection, NewLinkedScalar<bool>(nameof(SortBySelection)).BindToCheckBox());
            }

            private string TypeName
            {
                get { return _typeName.Value?.Trim(); }
            }

            public void Execute(AddProjectionDelegate addProjection)
            {
                var result = _modelMapper.CreateProjectionEntries(fillColumns: false);
                foreach (var row in SelectedRows.OrderBy(x => x.Index))
                {
                    var dataRow = result.AddRow();
                    result._.Column[dataRow] = row.GetValue(_.Column);
                }
                addProjection(TypeName, result);
            }

            private bool CanSelect(RowPresenter row)
            {
                return row.GetValue(_.Column) != null && row.GetValue(_.Mounter) != null;
            }

            private HashSet<DataRow> _selectedDataRows;
            private bool SortBySelection
            {
                get { return _selectedDataRows != null; }
                set
                {
                    if (SortBySelection == value)
                        return;

                    if (value)
                    {
                        _selectedDataRows = new HashSet<DataRow>();
                        foreach (var row in SelectedRows)
                            _selectedDataRows.Add(row.DataRow);
                    }
                    else
                        _selectedDataRows = null;

                    OrderBy = value ? this : null;
                }
            }

            protected override void OnIsSelectedChanged(RowPresenter row)
            {
                if (row.IsSelected && !CanSelect(row))
                {
                    row.IsSelected = false;
                    return;
                }

                if (_selectedDataRows == null)
                    return;

                if (row.IsSelected)
                    _selectedDataRows.Add(row.DataRow);
                else
                    _selectedDataRows.Remove(row.DataRow);

                row.UpdateSortOrder();
            }

            int IComparer<DataRow>.Compare(DataRow x, DataRow y)
            {
                if (_selectedDataRows == null || _selectedDataRows.Contains(x) == _selectedDataRows.Contains(y))
                    return Comparer<int>.Default.Compare(x.Ordinal, y.Ordinal);

                return _selectedDataRows.Contains(x) ? -1 : 1;
            }
        }
    }
}
