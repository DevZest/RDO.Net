using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;

namespace DevZest.Data.Tools
{
    partial class IndexWindow
    {
        private sealed class Presenter : IndexPresenterBase
        {
            private Scalar<bool> _isUnique;
            private Scalar<bool> _isValidOnTable;
            private Scalar<bool> _isValidOnTempTable;

            public Presenter(ModelMapper modelMapper, IndexWindow.IUIParams uiParams)
                : base(string.Format("IX_{0}_", modelMapper.ModelType.Name), modelMapper)
            {
                _uiParams = uiParams;

                _isUnique = NewScalar(false);
                _isValidOnTable = NewScalar(true);
                _isValidOnTempTable = NewScalar(false);

                Show();
            }

            private readonly IndexWindow.IUIParams _uiParams;
            protected override IUIParams UIParams
            {
                get { return _uiParams; }
            }

            public bool IsUnique
            {
                get { return _isUnique.Value; }
            }

            public bool IsValidOnTable
            {
                get { return _isValidOnTable.Value; }
            }

            public bool IsValidOnTempTable
            {
                get { return _isValidOnTempTable.Value; }
            }

            public void Execute(AddIndexDelegate addIndex)
            {
                addIndex(Name, Description, DbName, IsUnique, IsValidOnTable, IsValidOnTempTable, DataSet);
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                base.BuildTemplate(builder);

                builder.AddBinding(_uiParams.Unique, _isUnique.BindToCheckBox())
                    .AddBinding(_uiParams.Table, _isValidOnTable.BindToCheckBox())
                    .AddBinding(_uiParams.TempTable, _isValidOnTempTable.BindToCheckBox());

            }
        }
    }
}
