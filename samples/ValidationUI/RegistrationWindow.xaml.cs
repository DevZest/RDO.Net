﻿using DevZest.Data;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ValidationUI
{
    /// <summary>
    /// Interaction logic for RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        public static class Commands
        {
            public static readonly RoutedUICommand Submit = new RoutedUICommand();
        }

        public RegistrationWindow()
        {
            InitializeComponent();
            InitializeCommandBindings();
        }

        private void InitializeCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(Commands.Submit, Submit, CanSubmit));
        }

        private void Submit(object sender, ExecutedRoutedEventArgs e)
        {
            if (_presenter.SubmitInput())
                Close();
        }

        private void CanSubmit(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !_presenter.HasVisibleInputError;
        }

        private DataPresenter _presenter;

        public void ShowDefault(Window ownerWindow)
        {
            Show<DefaultPresenter, Registration>(ownerWindow, "Default");
        }

        public void ShowVerbose(Window ownerWindow)
        {
            Show<VerbosePresenter, Registration>(ownerWindow, "Verbose", ValidationErrorsControl.Templates.Failed, DevZest.Data.Presenters.Validation.Templates.Succeeded);
        }

        public void ShowDefaultScalar(Window ownerWindow)
        {
            Show<DefaultScalarPresenter, DummyModel>(ownerWindow, "Default - Scalar");
        }

        public void ShowVerboseScalar(Window ownerWindow)
        {
            Show<VerboseScalarPresenter, DummyModel>(ownerWindow, "Verbose - Scalar", ValidationErrorsControl.Templates.Failed, DevZest.Data.Presenters.Validation.Templates.Succeeded);
        }

        private void Show<T, TModel>(Window ownerWindow, string windowTitleSuffix, ControlTemplate failedTemplate = null, ControlTemplate succeededTemplate = null)
            where T : DataPresenter<TModel>, new()
            where TModel : Model, new()
        {
            Title = string.Format("{0} ({1})", Title, windowTitleSuffix);
            if (failedTemplate != null)
                _dataView.SetFailedTemplate(failedTemplate);
            if (succeededTemplate != null)
                _dataView.SetSucceededTemplate(succeededTemplate);
            var dataSet = DataSet<TModel>.New();
            dataSet.Add(new DataRow());
            var presenter = new T();
            _presenter = presenter;
            presenter.Show(_dataView, dataSet);
            Owner = ownerWindow;
            ShowDialog();
        }
    }
}
