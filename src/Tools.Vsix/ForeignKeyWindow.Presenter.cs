using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using Microsoft.CodeAnalysis;
using System.Windows.Controls;

namespace DevZest.Data.Tools
{
    partial class ForeignKeyWindow
    {
        private sealed class Presenter : DataPresenter<ModelMapper.ForeignKeyEntry>
        {
            private readonly ModelMapper _modelMapper;
            private readonly Scalar<INamedTypeSymbol> _fkType;
            private readonly Scalar<string> _fkName;

            public Presenter(ModelMapper modelMapper, ForeignKeyWindow window)
            {
                _window = window;

                _modelMapper = modelMapper;
                _fkType = NewScalar<INamedTypeSymbol>().AddValidator(Extensions.ValidateRequired);
                _fkName = NewScalar("FK_").AddValidator(Extensions.ValidateRequired).AddValidator(modelMapper.ValidateIdentifier);
                Show(window._dataView, modelMapper.CreateForeignKeyEntries());
            }

            private readonly ForeignKeyWindow _window;

            protected override void OnValueChanged(IScalars scalars)
            {
                base.OnValueChanged(scalars);

                if (scalars.Contains(_fkType))
                {
                    var fkType = _fkType.GetValue();
                    if (fkType == null)
                        return;
                    var containingType = fkType.ContainingType;
                    _fkName.EditValue(containingType == null ? "FK_" : "FK_" + containingType.Name);
                    _modelMapper.InitForeignKeyEntries(DataSet, FkType);
                }
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder.GridRows("Auto", "Auto")
                    .GridColumns("20", "*", "*")
                    .Layout(Orientation.Vertical)
                    .GridLineX(new GridPoint(1, 1), 2)
                    .GridLineX(new GridPoint(1, 2), 2)
                    .GridLineY(new GridPoint(1, 1), 1)
                    .WithFrozenTop(1)
                    .AddBinding(1, 0, this.BindToTextBlock(_.Parameter.DisplayName))
                    .AddBinding(2, 0, this.BindToTextBlock(_.Column.DisplayName))
                    .AddBinding(0, 1, _.BindToRowHeader().WithStyle(RowHeader.Styles.Flat))
                    .AddBinding(1, 1, _.Parameter.BindToTextBlock())
                    .AddBinding(2, 1, _.Column.BindToComboBox(_.ColumnSelection))
                    .AddBinding(_window._comboBoxPkType, _fkType.BindToComboBox(_modelMapper.GetKeyTypeSelection()))
                    .AddBinding(_window._textBoxFkName, _fkName.BindToTextBox());
                ;
            }

            private INamedTypeSymbol FkType
            {
                get { return _fkType.Value; }
            }

            private string FkName
            {
                get { return _fkName.Value?.Trim(); }
            }

            public void Execute(AddForeignKeyDelegate addForeignKey)
            {
                addForeignKey(FkType, FkName, DataSet);
            }
        }
    }
}
