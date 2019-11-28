using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using Microsoft.CodeAnalysis;
using System.Collections;
using System.Linq;

namespace DevZest.Data.Tools
{
    partial class RelationshipWindow
    {
        private sealed class Presenter : SimplePresenter
        {
            private readonly Scalar<string> _name;
            private readonly Scalar<IPropertySymbol> _foreignKey;
            private readonly Scalar<IPropertySymbol> _refTable;
            private readonly Scalar<IEnumerable> _refTableSelection;
            private readonly Scalar<string> _description;
            private readonly Scalar<ForeignKeyRule> _deleteRule;
            private readonly Scalar<ForeignKeyRule> _updateRule;

            public Presenter(DbMapper dbMapper, IPropertySymbol dbTable, RelationshipWindow window)
            {
                _window = window;
                _dbMapper = dbMapper;
                _dbTable = dbTable;

                _foreignKey = NewScalar<IPropertySymbol>().AddValidator(Extensions.ValidateRequired);
                _refTable = NewScalar<IPropertySymbol>().AddValidator(Extensions.ValidateRequired);
                _name = NewScalar<string>().AddValidator(Extensions.ValidateRequired).AddValidator(dbMapper.ValidateIdentifier);
                _refTableSelection = NewScalar<IEnumerable>();
                _description = NewScalar<string>();
                _deleteRule = NewScalar(ForeignKeyRule.None);
                _updateRule = NewScalar(ForeignKeyRule.None);

                Show(_window._view);
            }

            private readonly RelationshipWindow _window;
            private readonly DbMapper _dbMapper;
            private readonly IPropertySymbol _dbTable;

            protected override void OnValueChanged(IScalars scalars)
            {
                base.OnValueChanged(scalars);

                if (scalars.Contains(_foreignKey))
                {
                    var refTables = _dbMapper.GetRefTables(_dbTable, _foreignKey.Value);
                    _refTableSelection.Value = refTables.Select(x => new { Value = x, Display = x == _dbTable ? "_" : x.Name }).OrderBy(x => x.Display);
                    _refTable.EditValue(refTables.FirstOrDefault());
                    _name.EditValue(GetName());
                }
            }

            private string GetName()
            {
                var fkName = _foreignKey.Value?.Name;
                if (string.IsNullOrEmpty(fkName))
                    return null;

                if (fkName.StartsWith("FK_"))
                    fkName = fkName.Substring(3);
                return string.Format("FK_{0}_{1}", _dbTable.Name, fkName);
            }

            public void Execute(AddRelationshipDelegate addRelationship)
            {
                addRelationship(_name.Value, _foreignKey.Value, _refTable.Value, _description.Value, _deleteRule.Value, _updateRule.Value);
            }

            private IEnumerable GetRuleSelection()
            {
                yield return new { Value = ForeignKeyRule.None, Display = nameof(ForeignKeyRule.None) };
                yield return new { Value = ForeignKeyRule.Cascade, Display = nameof(ForeignKeyRule.Cascade) };
                yield return new { Value = ForeignKeyRule.SetDefault, Display = nameof(ForeignKeyRule.SetDefault) };
                yield return new { Value = ForeignKeyRule.SetNull, Display = nameof(ForeignKeyRule.SetNull) };
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder
                    .AddBinding(_window._comboBoxForeignKey, _foreignKey.BindToComboBox(_dbMapper.GetFkSelection(_dbTable)))
                    .AddBinding(_window._comboBoxRefTable, _refTable.BindToComboBox(_refTableSelection))
                    .AddBinding(_window._textBoxName, _name.BindToTextBox())
                    .AddBinding(_window._textBoxDescription, _description.BindToTextBox())
                    .AddBinding(_window._comboBoxDeleteRule, _deleteRule.BindToComboBox(GetRuleSelection()))
                    .AddBinding(_window._comboBoxUpdateRule, _updateRule.BindToComboBox(GetRuleSelection()));
            }
        }
    }
}
