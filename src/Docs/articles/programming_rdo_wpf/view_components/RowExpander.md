# RowExpander

<xref:DevZest.Data.Views.RowExpander> represents the control that displays a header that can expand/collapse child rows, as demonstrated in `FileExplorer` sample:

![image](/images/RowExpander.jpg)

## Features

* Toggle the expand/collapse state of current row.

## Usage

Put the <xref:DevZest.Data.Views.RowExpander> inside a <xref:DevZest.Data.Views.RowView>, it will be wired up and work automatically, as demonstrated in `FileExplorer` sample:

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
