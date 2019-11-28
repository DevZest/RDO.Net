using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Tools
{
    public abstract class IndexPresenterBase : DataPresenter<ModelMapper.IndexEntry>
    {
        public interface IUIParams
        {
            DataView View { get; }
            TextBox Name { get; }
            TextBox Description { get; }
            TextBox DbName { get; }
        }

        private readonly ModelMapper _modelMapper;
        private readonly string _defaultName;
        private readonly Scalar<string> _name;
        private readonly Scalar<string> _description;
        private readonly Scalar<string> _dbName;

        protected IndexPresenterBase(string defaultName, ModelMapper modelMapper)
        {
            _modelMapper = modelMapper;
            _defaultName = defaultName;
            _name = NewScalar(defaultName).AddValidator(Extensions.ValidateRequired).AddValidator(modelMapper.ValidateIdentifier);
            _description = NewScalar<string>();
            _dbName = NewScalar<string>();
        }

        protected void Show()
        {
            Show(UIParams.View, _modelMapper.CreateIndexEntries());
        }

        protected abstract IUIParams UIParams { get; }

        private IEnumerable Columns
        {
            get { return _modelMapper.GetColumns().Select(x => new { Value = x, Display = x.Name }); }
        }

        private IEnumerable SortDirections
        {
            get
            {
                return new[]
                {
                        new { Value = SortDirection.Unspecified, Display = string.Empty },
                        new { Value = SortDirection.Ascending, Display = UserMessages.SortDirection_Asc },
                        new { Value = SortDirection.Descending, Display = UserMessages.SortDirection_Desc }
                    };
            }
        }

        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder.GridRows("Auto", "Auto")
                .GridColumns("20", "*", "100")
                .Layout(Orientation.Vertical)
                .WithVirtualRowPlacement(VirtualRowPlacement.Tail)
                .AddBinding(1, 0, this.BindToTextBlock(_.Column.DisplayName))
                .AddBinding(2, 0, this.BindToTextBlock(_.SortDirection.DisplayName))
                .AddBinding(0, 1, _.BindToRowHeader().WithStyle(RowHeader.Styles.Flat))
                .AddBinding(1, 1, _.Column.BindToComboBox(Columns))
                .AddBinding(2, 1, _.SortDirection.BindToComboBox(SortDirections).ApplyRefresh(RefreshIsEnabled))
                .AddBinding(UIParams.Name, _name.BindToTextBox())
                .AddBinding(UIParams.Description, _description.BindToTextBox())
                .AddBinding(UIParams.DbName, _dbName.BindToTextBox());
        }

        protected override void OnEdit(Column column)
        {
            CurrentRow.EndEdit();
            if (column == _.Column && !CurrentRow.IsEditing)
                RefreshName();
        }

        public void RefreshName()
        {
            _name.EditValue(_defaultName + string.Join("_", DataSet.Select(x => _.Column[x].Name)));
        }

        private void RefreshIsEnabled(UIElement v, RowPresenter p)
        {
            v.IsEnabled = p.GetValue(_.Column) != null;
        }

        public string Name
        {
            get { return _name.Value?.Trim(); }
        }

        public string Description
        {
            get { return _description.Value?.Trim(); }
        }

        public string DbName
        {
            get { return _dbName.Value?.Trim(); }
        }
    }
}
