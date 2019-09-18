# RowSelector

<xref:DevZest.Data.Views.RowSelector> represents the control that can perform row selection operation, as shown in `FileExplorer` sample:

![image](/images/RowSelector.jpg)

## Features

* Command handlers for row selection operations.

## Usage

Put the <xref:DevZest.Data.Views.RowSelector> inside a <xref:DevZest.Data.Views.RowView>, it will be wired up and work automatically.

You can add <xref:DevZest.Data.Views.RowSelector> via composite row binding, as demonstrated in `FileExplorer` sample:

```xaml
...
<StackPanel Orientation="Horizontal">
    <dz:RowExpander Name="_rowExpander" VerticalAlignment="Center" />
    <dz:RowSelector>
        <StackPanel Orientation="Horizontal">
            <Image Name="_icon" Width="20" Height="20" Stretch="Fill" />
            <TextBlock Name="_textBlock" Margin="5,0" />
        </StackPanel>
    </dz:RowSelector>
</StackPanel>
...
```

You can also use <xref:DevZest.Data.Views.RowSelector> in control template of <xref:DevZest.Data.Views.RowView>, for example, the [RowView.Styles.Selectable](xref:DevZest.Data.Views.RowView.Styles.Selectable) style.

## Implemented Commands

| Command | Input | Implementation |
|---------|-------|----------------|
| <xref:DevZest.Data.Views.RowSelector.Commands.MoveUp> | Up Key | Moves current row up. |
| <xref:DevZest.Data.Views.RowSelector.Commands.MoveDown> | Down Key | Moves current row down. |
| <xref:DevZest.Data.Views.RowSelector.Commands.MoveLeft> | Left Key | Moves current row left. |
| <xref:DevZest.Data.Views.RowSelector.Commands.MoveRight> | Right Key | Moves current row right. |
| <xref:DevZest.Data.Views.RowSelector.Commands.MoveToPageUp> | PageUp Key | Moves current row to one page up. |
| <xref:DevZest.Data.Views.RowSelector.Commands.MoveToPageDown> | PageDown Key | Moves current row to one page down. |
| <xref:DevZest.Data.Views.RowSelector.Commands.MoveToHome> | Home Key | Moves current row to the first row. |
| <xref:DevZest.Data.Views.RowSelector.Commands.MoveToEnd> | End Key | Moves current row to the last row. |
| <xref:DevZest.Data.Views.RowSelector.Commands.SelectExtendedUp> | SHIFT-Up Key | Selects multiple consecutive rows up. |
| <xref:DevZest.Data.Views.RowSelector.Commands.SelectExtendedDown> | SHIFT-Down Key | Selects multiple consecutive rows down. |
| <xref:DevZest.Data.Views.RowSelector.Commands.SelectExtendedLeft> | SHIFT-Left Key | Selects multiple consecutive rows left. |
| <xref:DevZest.Data.Views.RowSelector.Commands.SelectExtendedRight> | SHIFT-Right Key | Selects multiple consecutive rows right. |
| <xref:DevZest.Data.Views.RowSelector.Commands.SelectExtendedPageUp> | SHIFT-PageUp Key | Selects multiple consecutive rows to one page up. |
| <xref:DevZest.Data.Views.RowSelector.Commands.SelectExtendedPageDown> | SHIFT-PageDown Key | Selects multiple consecutive rows to one page down. |
| <xref:DevZest.Data.Views.RowSelector.Commands.SelectExtendedHome> | SHIFT-Home Key | Selects multiple consecutive rows to the first row. |
| <xref:DevZest.Data.Views.RowSelector.Commands.SelectExtendedEnd> | SHIFT-End Key | Selects multiple consecutive rows to the last row. |
| <xref:DevZest.Data.Views.RowSelector.Commands.ToggleSelection> | SPACE Key | Toggles the selection mode of current row. |

## Customizable Services

* <xref:DevZest.Data.Views.RowSelector.ICommandService>: Your data presenter can implement this service to change the commands implementation of this class.
