using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using Microsoft.CodeAnalysis;

namespace DevZest.Data.Tools
{
    partial class UniqueConstraintWindow
    {
        private sealed class Presenter : IndexPresenterBase
        {
            private readonly Scalar<INamedTypeSymbol> _resourceType;
            private readonly Scalar<IPropertySymbol> _resourceProperty;
            private readonly Scalar<string> _message;

            public Presenter(ModelMapper modelMapper, UniqueConstraintWindow.IUIParams uiParams)
                : base(string.Format("AK_{0}_", modelMapper.ModelType.Name), modelMapper)
            {
                _uiParams = uiParams;

                _resourceType = NewScalar(modelMapper.GetMessageResourceType());
                _resourceProperty = NewScalar<IPropertySymbol>();
                _message = NewScalar<string>();

                Show();
            }

            private readonly UniqueConstraintWindow.IUIParams _uiParams;
            protected override IUIParams UIParams
            {
                get { return _uiParams; }
            }

            private INamedTypeSymbol ResourceType
            {
                get { return _resourceType.Value; }
            }

            private IPropertySymbol ResourceProperty
            {
                get { return _resourceProperty.Value; }
            }

            private string Message
            {
                get { return _message.Value; }
            }

            protected override IScalarValidationErrors ValidateScalars(IScalarValidationErrors result)
            {
                return MessageView.Validate(result, _resourceType, _resourceProperty);
            }

            public void Execute(AddUniqueConstraintDelegate addUniqueConstraint)
            {
                addUniqueConstraint(Name, Description, DbName, ResourceType, ResourceProperty, Message, DataSet);
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                base.BuildTemplate(builder);

                builder.AddBinding(_uiParams.MessageView, _resourceType, _resourceProperty, _message);
                _resourceType.Value = null; // _resourceType.Value is required for MessageView initialization, change back to default value null when done.
            }
        }
    }
}
