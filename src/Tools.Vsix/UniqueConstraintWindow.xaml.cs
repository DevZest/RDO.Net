using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using Microsoft.CodeAnalysis;
using System;
using System.Windows.Controls;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// Interaction logic for UniqueConstraintWindow.xaml
    /// </summary>
    public sealed partial class UniqueConstraintWindow : IndexWindowBase, UniqueConstraintWindow.IUIParams
    {
        internal interface IUIParams : IndexPresenterBase.IUIParams
        {
            MessageView MessageView { get; }
        }

        public static void Show(ModelMapper modelMapper, AddUniqueConstraintDelegate addUniqueConstraint)
        {
            new UniqueConstraintWindow().ShowDialog(modelMapper, addUniqueConstraint);
        }

        private UniqueConstraintWindow()
        {
            InitializeComponent();
        }

        private Presenter _presenter;
        private AddUniqueConstraintDelegate _addUniqueConstraint;

        MessageView IUIParams.MessageView => _messageView;

        DataView IndexPresenterBase.IUIParams.View => _dataView;

        TextBox IndexPresenterBase.IUIParams.Name => _textBoxName;

        TextBox IndexPresenterBase.IUIParams.Description => _textBoxDescription;

        TextBox IndexPresenterBase.IUIParams.DbName => _textBoxDbName;

        private void ShowDialog(ModelMapper modelMapper, AddUniqueConstraintDelegate addUniqueConstraint)
        {
            _presenter = new Presenter(modelMapper, this);
            _addUniqueConstraint = addUniqueConstraint;
            ShowDialog();
        }

        protected override BasePresenter GetPresenter()
        {
            return _presenter;
        }

        protected override void ExecApply()
        {
            _presenter.Execute(_addUniqueConstraint);
        }
    }
}
