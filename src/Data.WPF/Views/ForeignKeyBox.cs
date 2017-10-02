using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Views
{
    public class ForeignKeyBox : Button
    {
        public static readonly RoutedUICommand SelectCommand = new RoutedUICommand();
        public static readonly RoutedUICommand ResetCommand = new RoutedUICommand();

        public KeyBase ForeignKey { get; set; }
    }
}
