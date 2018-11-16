using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views;
using System;

namespace DevZest.Data.Presenters
{
    public abstract class ScalarBagPresenter : BasePresenter
    {
        private sealed class DummyModel : Model
        {
        }

        protected sealed class TemplateBuilder : BaseTemplateBuilder<TemplateBuilder>
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
            Mount(view, dataSet);
            OnViewChanged();
        }

        private void Mount(ScalarBagView view, DataSet<DummyModel> dataSet)
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
        }

        private LayoutManager _layoutManager;
        internal sealed override LayoutManager LayoutManager
        {
            get { return _layoutManager; }
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
