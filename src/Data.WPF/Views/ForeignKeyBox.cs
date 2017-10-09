using DevZest.Data.Presenters;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DevZest.Data.Views
{
    public class ForeignKeyBox : ButtonBase
    {
        public static readonly RoutedUICommand EditCommand = new RoutedUICommand();
        public static readonly RoutedUICommand ResetCommand = new RoutedUICommand();

        public KeyBase ForeignKey { get; set; }

        private RowPresenter RowPresenter
        {
            get { return this.GetRowPresenter(); }
        }

        private DataPresenter DataPresenter
        {
            get { return RowPresenter?.DataPresenter; }
        }

        protected override void OnClick()
        {
            base.OnClick();
        }
    }
}

