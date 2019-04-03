using DevZest.Data.Presenters;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;

namespace DevZest.Data.Views
{
    /// <summary>
    /// Interaction logic for PasteAppendWindow.xaml
    /// </summary>
    internal partial class PasteAppendWindow : Window
    {
        public static readonly RoutedCommand Submit = new RoutedCommand(nameof(Submit), typeof(PasteAppendWindow));

        public static bool GetAutoTooltip(TextBlock textBlock)
        {
            return (bool)textBlock.GetValue(AutoTooltipProperty);
        }

        public static void SetAutoTooltip(TextBlock textBlock, bool value)
        {
            textBlock.SetValue(AutoTooltipProperty, value);
        }

        public static readonly DependencyProperty AutoTooltipProperty = DependencyProperty.RegisterAttached("AutoTooltip",
                typeof(bool), typeof(PasteAppendWindow), new PropertyMetadata(BooleanBoxes.False, OnAutoTooltipPropertyChanged));

        private static void OnAutoTooltipPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBlock textBlock = d as TextBlock;
            if (textBlock == null)
                return;

            if ((bool)e.NewValue)
            {
                ComputeAutoTooltip(textBlock);
                textBlock.SizeChanged += TextBlock_SizeChanged;
            }
            else
            {
                textBlock.SizeChanged -= TextBlock_SizeChanged;
            }
        }

        private static void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            ComputeAutoTooltip(textBlock);
        }

        private static void ComputeAutoTooltip(TextBlock textBlock)
        {
            textBlock.Measure(new Size(double.PositiveInfinity, textBlock.ActualHeight));
            var width = textBlock.DesiredSize.Width;

            if (textBlock.ActualWidth < width)
                ToolTipService.SetToolTip(textBlock, textBlock.Text);
            else
                ToolTipService.SetToolTip(textBlock, null);
        }

        public PasteAppendWindow()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(Submit, ExecSubmit));
        }

        private IReadOnlyList<ColumnValueBag> _result;
        private void ExecSubmit(object sender, ExecutedRoutedEventArgs e)
        {
            _result = _presenter.Submit();
            if (_result != null)
                Close();
            e.Handled = true;
        }

        private Presenter _presenter;
        public IReadOnlyList<ColumnValueBag> Show(DataPresenter sourcePresenter, IReadOnlyList<Column> columns)
        {
            _presenter = new Presenter(sourcePresenter, columns, _dataView, _firstRowContainsColumnHeadings);
            Owner = Window.GetWindow(sourcePresenter.View);
            ShowDialog();
            return _result;
        }
    }
}
