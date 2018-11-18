using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views;
using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public abstract class ScalarBagPresenter : BasePresenter
    {
        private sealed class DummyModel : Model
        {
        }

        protected sealed class TemplateBuilder : TemplateBuilder<TemplateBuilder>
        {
            internal TemplateBuilder(Template template)
                : base(template)
            {
            }
        }

        public void Show(ScalarBagView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            if (view.Presenter != null && view.Presenter != this)
                throw new ArgumentException(DiagnosticMessages.DataPresenter_InvalidDataView, nameof(view));

            var dataSet = DataSet<DummyModel>.New();
            AttachView(view);
            Mount(dataSet);
            OnViewChanged();
        }

        private void Mount(DataSet<DummyModel> dataSet)
        {
            var template = new Template();
            template.AddGridColumns("*");
            template.AddGridRows("*");
            template.RowRange = new GridRange(template.GridColumns[0], template.GridRows[0]);
            using (var builder = new TemplateBuilder(template))
            {
                BuildTemplate(builder);
                builder.Seal();
            }

            dataSet.EnsureInitialized();
            _layoutManager = LayoutManager.Create(null, this, template, dataSet, null, null, null);
            OnMounted(MountMode.Show);

            if (View.IsLoaded)
                Template.InitFocus();
            else
                View.Loaded += OnViewLoaded;
        }

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            View.Loaded -= OnViewLoaded;
            Template.InitFocus();
        }

        private LayoutManager _layoutManager;
        internal sealed override LayoutManager LayoutManager
        {
            get { return _layoutManager; }
        }

        public override void DetachView()
        {
            base.DetachView();
            _layoutManager = null;  // This must be called after base.DetachView()
        }

        public new ScalarBagView View { get; private set; }

        internal override IBaseView GetView()
        {
            return View;
        }

        internal override void SetView(IBaseView value)
        {
            View = (ScalarBagView)value;
        }

        protected abstract void BuildTemplate(TemplateBuilder builder);
    }
}
