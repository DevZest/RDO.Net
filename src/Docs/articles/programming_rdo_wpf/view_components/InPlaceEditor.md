# InPlaceEditor

<xref:DevZest.Data.Views.InPlaceEditor> represents a container that switches between inert and editor elements based on editing mode, as demonstrated in `FileExplorer` sample:

![image](/images/InPlaceEditor.jpg)

## Features

* Switches between inert and editor elements based on editing mode.

## Usage

Add <xref:DevZest.Data.Views.InPlaceEditor> as scalar or row binding via <xref:DevZest.Data.Presenters.BindingFactory.MergeIntoInPlaceEditor*>, as demonstrated in `FileExplorer` sample (composite row binding):

```csharp
public static RowCompositeBinding<LargeIconListItemView> BindToLargeIconListItemView(this LargeIconListItem _)
{
    var textBoxBinding = _.DisplayName.BindToTextBox();
    var textBlockBinding = _.DisplayName.BindToTextBlock();

    return new RowCompositeBinding<LargeIconListItemView>((v, p) => Refresh(v, _, p))
        .AddChild(textBoxBinding.MergeIntoInPlaceEditor(textBlockBinding), v => v.InPlaceEditor);
}
```

## Customizable Services

* <xref:DevZest.Data.Views.InPlaceEditor.IEditingPolicyService>:  The default implementation use <xref:DevZest.Data.Views.InPlaceEditor.IsScalarEditing>/<xref:DevZest.Data.Views.InPlaceEditor.IsRowEditing> to determine the editing mode, and preserves the keyboard focus after switching the mode. Your data presenter can implement this service to provide custom editing policy. Please note this service will be ignored if you set `InPlaceEditor.EditingPolicy` attached property (<xref:DevZest.Data.Views.InPlaceEditor.SetEditingPolicy*>) directly.
* <xref:DevZest.Data.Views.InPlaceEditor.IChildInitializer>: The default implementation selects the text when editor element is a TextBox. Your data presenter can implement this interface to provide custom initializer.
