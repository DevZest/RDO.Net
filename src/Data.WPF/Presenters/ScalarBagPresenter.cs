using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views;
using System;

namespace DevZest.Data.Presenters
{
    public class ScalarBagPresenter : DataPresenterBase
    {
        private sealed class DummyModel : Model
        {
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

            dataSet.EnsureInitialized();
            _layoutManager = LayoutManager.Create(null, this, template, dataSet, null, null, null);
            OnMounted(MountEventArgs.Select(MountMode.Show));
        }

        private LayoutManager _layoutManager;
        internal sealed override LayoutManager LayoutManager
        {
            get { return _layoutManager; }
        }

        public new ScalarBagView View { get; private set; }

        internal override IDataView GetView()
        {
            return View;
        }

        internal override void SetView(IDataView value)
        {
            View = (ScalarBagView)value;
        }
    }
}
