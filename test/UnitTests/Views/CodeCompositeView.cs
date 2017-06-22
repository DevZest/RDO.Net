using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Views
{
    public class CodeCompositeView : CompositeView
    {
        public const string NAME_LEFT = "LEFT";
        public const string NAME_RIGHT = "RIGHT";

        ContentPresenter _left;
        ContentPresenter _right;

        public CodeCompositeView()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            _left = new ContentPresenter();
            Grid.SetRow(_left, 0);
            Grid.SetColumn(_left, 0);
            _right = new ContentPresenter();
            Grid.SetRow(_right, 0);
            Grid.SetColumn(_right, 1);
            grid.Children.Add(_left);
            grid.Children.Add(_right);
            Content = grid;
        }

        public override ContentPresenter GetPlaceholder(string name)
        {
            switch (name)
            {
                case NAME_LEFT:
                    return _left;
                case NAME_RIGHT:
                    return _right;
            }
            return base.GetPlaceholder(name);
        }
    }
}
