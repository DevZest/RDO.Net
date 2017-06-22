using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views.Primitives;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Views
{
    [ContentPropertyAttribute(nameof(CompositeView.Content))]
    public class CompositeView : ContentPresenter, ICompositeView
    {
        public CompositeView()
        {
            _compositeBinding = new CompositeBinding(this);
        }

        private readonly CompositeBinding _compositeBinding;
        public CompositeBinding CompositeBinding
        {
            get { return _compositeBinding; }
        }

        public IReadOnlyList<UIElement> Children
        {
            get { return CompositeBinding.Children; }
        }

        public virtual ContentPresenter GetPlaceholder(string name)
        {
            return FindName(name) as ContentPresenter;
        }
    }
}
