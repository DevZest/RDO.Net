using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using System;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// Interaction logic for PrimaryKeyWindow.xaml
    /// </summary>
    public partial class PrimaryKeyWindow : RowArrangerDialogWindow
    {
        public PrimaryKeyWindow()
        {
            InitializeComponent();
        }

        public static void Show(ModelMapper modelMapper, AddPrimaryKeyDelegate addPrimaryKey)
        {
            new PrimaryKeyWindow().ShowDialog(modelMapper, addPrimaryKey);
        }

        private Presenter _presenter;
        protected override BasePresenter GetPresenter()
        {
            return _presenter;
        }

        private AddPrimaryKeyDelegate _addPrimaryKey;
        private void ShowDialog(ModelMapper modelMapper, AddPrimaryKeyDelegate addPrimaryKey)
        {
            _presenter = new Presenter(modelMapper, this);
            _addPrimaryKey = addPrimaryKey;
            ShowDialog();
        }

        protected override void ExecApply()
        {
            _presenter.Execute(_addPrimaryKey);
        }
    }
}
