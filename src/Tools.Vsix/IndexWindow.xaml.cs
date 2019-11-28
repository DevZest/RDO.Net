using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System;
using System.Windows.Controls;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// Interaction logic for IndexWindow.xaml
    /// </summary>
    public sealed partial class IndexWindow : IndexWindowBase, IndexWindow.IUIParams
    {
        internal interface IUIParams : IndexPresenterBase.IUIParams
        {
            CheckBox Unique { get; }
            CheckBox Table { get; }
            CheckBox TempTable { get; }
        }

        public static void Show(ModelMapper modelMapper, AddIndexDelegate addIndex)
        {
            new IndexWindow().ShowDialog(modelMapper, addIndex);
        }

        private IndexWindow()
        {
            InitializeComponent();
        }

        private Presenter _presenter;
        private AddIndexDelegate _addIndex;

        CheckBox IUIParams.Unique => _checkBoxUnique;

        CheckBox IUIParams.Table => _checkBoxTable;

        CheckBox IUIParams.TempTable => _checkBoxTempTable;

        DataView IndexPresenterBase.IUIParams.View => _dataView;

        TextBox IndexPresenterBase.IUIParams.Name => _textBoxName;

        TextBox IndexPresenterBase.IUIParams.Description => _textBoxDescription;

        TextBox IndexPresenterBase.IUIParams.DbName => _textBoxDbName;

        private void ShowDialog(ModelMapper modelMapper, AddIndexDelegate addIndex)
        {
            _presenter = new Presenter(modelMapper, this);
            _addIndex = addIndex;
            ShowDialog();
        }

        protected override BasePresenter GetPresenter()
        {
            return _presenter;
        }

        protected override void ExecApply()
        {
            _presenter.Execute(_addIndex);
        }
    }
}
