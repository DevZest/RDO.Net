using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using System;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// Interaction logic for ProjectionWindow.xaml
    /// </summary>
    public partial class ProjectionWindow : CommonDialogWindow
    {
        public ProjectionWindow()
        {
            InitializeComponent();
        }

        public static void Show(string defaultTypeName, ModelMapper modelMapper, AddProjectionDelegate addProjection)
        {
            new ProjectionWindow().ShowDialog(defaultTypeName, modelMapper, addProjection);
        }

        private Presenter _presenter;
        protected override BasePresenter GetPresenter()
        {
            return _presenter;
        }

        private AddProjectionDelegate _addProjection;
        private void ShowDialog(string defaultTypeName, ModelMapper modelMapper, AddProjectionDelegate addProjection)
        {
            _presenter = new Presenter(modelMapper, _dataView, _textBoxName, defaultTypeName, _checkBoxSortBySelection);
            _addProjection = addProjection;
            ShowDialog();
        }

        protected override bool CanApply
        {
            get { return base.CanApply && _presenter.SelectedRows.Count > 0; }
        }

        protected override bool Validate()
        {
            // only scalar values needs to be validated
            var scalarValidation = _presenter.ScalarValidation;
            scalarValidation.Validate();
            return !scalarValidation.HasVisibleError;
        }

        protected override void ExecApply()
        {
            _presenter.Execute(_addProjection);
        }
    }
}
