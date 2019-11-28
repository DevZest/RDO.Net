using DevZest.Data.Presenters;
using Microsoft.CodeAnalysis;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// Interaction logic for MessageView.xaml
    /// </summary>
    public partial class MessageView : UserControl
    {
        public MessageView()
        {
            InitializeComponent();
        }

        private static IEnumerable GetResourceTypeSelection(INamedTypeSymbol resourceType)
        {
            if (resourceType == null)
                yield return new { Value = resourceType, Display = UserMessages.MessageView_MissingMessageResourceAttribute };
            else
            {
                yield return new { Value = (INamedTypeSymbol)null, Display = string.Empty };
                yield return new { Value = resourceType, Display = resourceType.Name };
            }
        }

        private static IEnumerable GetResourcePropertySelection(INamedTypeSymbol messageType)
        {
            return messageType.GetMembers().OfType<IPropertySymbol>().Where(IsResourceProperty).Select(x => new { Value = x, Display = x.Name });
        }

        private static bool IsResourceProperty(IPropertySymbol property)
        {
            return property.IsStatic && property.GetMethod != null && property.Type.SpecialType == SpecialType.System_String;
        }

        public void AddBinding<T>(TemplateBuilder<T> builder, Scalar<INamedTypeSymbol> resourceType, Scalar<IPropertySymbol> resourceProperty, Scalar<string> message)
            where T : TemplateBuilder<T>
        {
            var resourceTypeValue = resourceType.Value;
            builder.AddBinding(_comboBoxResourceType, resourceType.BindToComboBox(GetResourceTypeSelection(resourceTypeValue)).ApplyRefresh((v, p) =>
            {
                Refresh(resourceType.Value != null);
            }));
            if (resourceTypeValue != null)
                builder.AddBinding(_comboBoxResourceProperty, resourceProperty.BindToComboBox(GetResourcePropertySelection(resourceTypeValue)));
            else
                _comboBoxResourceType.IsEnabled = false;
            builder.AddBinding(_textBoxMessage, message.BindToTextBox());
        }

        private void Refresh(bool hasResourceType)
        {
            _comboBoxResourceProperty.Visibility = hasResourceType ? Visibility.Visible : Visibility.Collapsed;
            _textBoxMessage.Visibility = hasResourceType ? Visibility.Collapsed : Visibility.Visible;
        }

        public static IScalarValidationErrors Validate(IScalarValidationErrors result, Scalar<INamedTypeSymbol> resourceType, Scalar<IPropertySymbol> resourceProperty)
        {
            if (resourceType.Value != null && resourceProperty.Value == null)
                result = result.Add(new ScalarValidationError(UserMessages.MessageView_ValueIsRequired, resourceProperty));

            return result;
        }

        public static IScalarValidationErrors Validate(IScalarValidationErrors result, Scalar<INamedTypeSymbol> resourceType, Scalar<IPropertySymbol> resourceProperty,
            Scalar<string> message)
        {
             result = Validate(result, resourceType, resourceProperty);

            if (resourceType.Value == null && string.IsNullOrWhiteSpace(message.Value))
                result = result.Add(new ScalarValidationError(UserMessages.MessageView_ValueIsRequired, message));
            return result;
        }
    }
}
