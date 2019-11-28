using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System;
using System.Windows.Controls;

namespace DevZest.Data.Tools
{
    partial class KeyOrRefWindow
    {
        private sealed class Presenter : DataPresenter<ModelMapper.PrimaryKeyEntry>
        {
            private readonly Scalar<string> _typeName;

            public Presenter(ModelMapper modelMapper, DataView dataView, TextBox textBoxTypeName, string defaultTypeName)
            {
                _textBoxTypeName = textBoxTypeName;

                _typeName = NewScalar(defaultTypeName).AddValidator(Extensions.ValidateRequired).AddValidator(modelMapper.ValidateIdentifier);
                Show(dataView, modelMapper.CreatePrimaryKeyEntries());
            }

            private static TextBox _textBoxTypeName;

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder.GridRows("Auto", "Auto")
                    .GridColumns("20", "*", "*", "*")
                    .Layout(Orientation.Vertical)
                    .AddBinding(1, 0, this.BindToTextBlock(_.Column.DisplayName))
                    .AddBinding(2, 0, this.BindToTextBlock(_.Mounter.DisplayName))
                    .AddBinding(3, 0, this.BindToTextBlock(_.ConstructorParamName.DisplayName))
                    .AddBinding(0, 1, _.BindToRowHeader().WithStyle(RowHeader.Styles.Flat))
                    .AddBinding(1, 1, _.Column.BindToTextBox())
                    .AddBinding(2, 1, _.Mounter.BindToTextBox())
                    .AddBinding(3, 1, _.ConstructorParamName.BindToTextBox())
                    .AddBinding(_textBoxTypeName, _typeName.BindToTextBox());
            }

            private string TypeName
            {
                get { return _typeName.Value?.Trim(); }
            }

            public void Execute(AddKeyOrRefDelegate addKeyOrRef)
            {
                addKeyOrRef(TypeName, DataSet);
            }
        }
    }
}
