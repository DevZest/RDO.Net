using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// Interaction logic for TreeItemView.xaml
    /// </summary>
    public partial class TreeItemView : UserControl
    {
        public static readonly RoutedUICommand ShowContextMenuCommand = new RoutedUICommand();

        public TreeItemView()
        {
            InitializeComponent();
        }

        private int _depth;
        public int Depth
        {
            get { return _depth; }
            private set
            {
                _depth = value;
                var left = value == 0 ? 0d : 4d + 15d * (value - 1);
                _rowExpander.Margin = new Thickness(left, 0, 0, 0);
            }
        }

        public Visibility ExpanderVisibility
        {
            get { return _rowExpander.Visibility; }
            private set { _rowExpander.Visibility = value; }
        }

        public ImageSource Icon
        {
            get { return _icon.Source; }
            private set { _icon.Source = value; }
        }

        public string Text
        {
            get { return _textBlock.Text; }
            private set { _textBlock.Text = value; }
        }

        public bool IsLocal
        {
            get { return _textBlock.IsEnabled; }
            private set { _textBlock.IsEnabled = value; }
        }

        public void Refresh(int depth, Visibility expanderVisibility, ImageSource icon, string text, bool isLocal)
        {
            Depth = depth;
            ExpanderVisibility = expanderVisibility;
            Icon = icon;
            Text = text;
            IsLocal = isLocal;
        }
    }
}
