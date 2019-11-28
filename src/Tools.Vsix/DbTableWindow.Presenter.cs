using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using Microsoft.CodeAnalysis;

namespace DevZest.Data.Tools
{
    partial class DbTableWindow
    {
        private sealed class Presenter : SimplePresenter
        {
            private readonly Scalar<INamedTypeSymbol> _model;
            private readonly Scalar<string> _name;
            private readonly Scalar<string> _dbName;
            private readonly Scalar<string> _description;

            public Presenter(DbMapper dbMapper, DbTableWindow window)
            {
                _window = window;
                _dbMapper = dbMapper;

                _model = NewScalar<INamedTypeSymbol>().AddValidator(Extensions.ValidateRequired);
                _name = NewScalar<string>().AddValidator(Extensions.ValidateRequired).AddValidator(dbMapper.ValidateIdentifier);
                _dbName = NewScalar<string>();
                _description = NewScalar<string>();

                Show(_window._view);
            }

            private readonly DbTableWindow _window;
            private readonly DbMapper _dbMapper;

            protected override void OnValueChanged(IScalars scalars)
            {
                base.OnValueChanged(scalars);

                if (scalars.Contains(_model))
                {
                    var modelType = _model.GetValue();
                    _name.EditValue(modelType?.Name);
                }
            }

            public void Execute(AddDbTableDelegate addDbTable)
            {
                addDbTable(_model.Value, _name.Value, _dbName.Value, _description.Value);
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder
                    .AddBinding(_window._comboBoxModel, _model.BindToComboBox(_dbMapper.GetModelTypeSelection()))
                    .AddBinding(_window._textBoxName, _name.BindToTextBox())
                    .AddBinding(_window._textBoxDbName, _dbName.BindToTextBox())
                    .AddBinding(_window._textBoxDescription, _description.BindToTextBox());
            }
        }
    }
}
