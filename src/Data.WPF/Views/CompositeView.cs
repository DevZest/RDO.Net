using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views.Primitives;
using System.Windows.Controls;
using System.Windows.Markup;

namespace DevZest.Data.Views
{
    [ContentPropertyAttribute(nameof(CompositeView.Content))]
    public class CompositeView : ContentPresenter, ICompositeView
    {
        public CompositeView()
        {
            _bindingManager = new CompositeBindingManager(this);
        }

        private readonly CompositeBindingManager _bindingManager;
        public CompositeBindingManager BindingManager
        {
            get { return _bindingManager; }
        }

        public virtual ContentPresenter GetPlaceholder(string name)
        {
            return FindName(name) as ContentPresenter;
        }
    }
}
