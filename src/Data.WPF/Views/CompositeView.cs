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
            _bindingDispatcher = new CompositeBindingDispatcher(this);
        }

        private readonly CompositeBindingDispatcher _bindingDispatcher;
        public CompositeBindingDispatcher BindingDispatcher
        {
            get { return _bindingDispatcher; }
        }

        public virtual ContentPresenter GetPlaceholder(string name)
        {
            return FindName(name) as ContentPresenter;
        }
    }
}
