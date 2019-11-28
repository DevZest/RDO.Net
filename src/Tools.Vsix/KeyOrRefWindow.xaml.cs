using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using System;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// Interaction logic for KeyOrRefWindow.xaml
    /// </summary>
    public partial class KeyOrRefWindow : CommonDialogWindow
    {
        public KeyOrRefWindow()
        {
            InitializeComponent();
        }

        public static void Show(string title, string defaultTypeName, ModelMapper modelMapper, AddKeyOrRefDelegate addKeyOrRef)
        {
            new KeyOrRefWindow().ShowDialog(title, defaultTypeName, modelMapper, addKeyOrRef);
        }

        private Presenter _presenter;
        protected override BasePresenter GetPresenter()
        {
            return _presenter;
        }

        private AddKeyOrRefDelegate _addKeyOrRef;
        private void ShowDialog(string title, string defaultTypeName, ModelMapper modelMapper, AddKeyOrRefDelegate addKeyOrRef)
        {
            Title = title;
            _presenter = new Presenter(modelMapper, _dataView, _textBoxName, defaultTypeName);
            _addKeyOrRef = addKeyOrRef;
            ShowDialog();
        }

        protected override void ExecApply()
        {
            _presenter.Execute(_addKeyOrRef);
        }
    }
}
