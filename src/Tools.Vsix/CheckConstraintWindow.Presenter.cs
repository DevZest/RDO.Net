using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using Microsoft.CodeAnalysis;

namespace DevZest.Data.Tools
{
    partial class CheckConstraintWindow
    {
        private sealed class Presenter : SimplePresenter
        {
            private readonly Scalar<string> _name;
            private readonly Scalar<string> _description;
            private readonly Scalar<INamedTypeSymbol> _resourceType;
            private readonly Scalar<IPropertySymbol> _resourceProperty;
            private readonly Scalar<string> _message;

            public Presenter(ModelMapper modelMapper, CheckConstraintWindow window)
            {
                _window = window;

                _name = NewScalar(string.Format("CK_{0}_", modelMapper.ModelType.Name));
                _description = NewScalar<string>();
                _resourceType = NewScalar(modelMapper.GetMessageResourceType());
                _resourceProperty = NewScalar<IPropertySymbol>();
                _message = NewScalar<string>();

                Show(_window._view);
            }

            private readonly CheckConstraintWindow _window;

            protected override IScalarValidationErrors ValidateScalars(IScalarValidationErrors result)
            {
                return MessageView.Validate(result, _resourceType, _resourceProperty, _message);
            }

            public void Execute(AddCheckConstraintDelegate addCheckConstraint)
            {
                addCheckConstraint(_name.Value, _description.Value, _resourceType.Value, _resourceProperty.Value, _message.Value);
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder
                    .AddBinding(_window._textBoxName, _name.BindToTextBox())
                    .AddBinding(_window._textBoxDescription, _description.BindToTextBox())
                    .AddBinding(_window._messageView, _resourceType, _resourceProperty, _message);
            }
        }
    }
}
