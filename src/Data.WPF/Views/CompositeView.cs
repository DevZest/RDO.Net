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
            _presenter = new CompositePresenter(this);
        }

        private readonly CompositePresenter _presenter;
        public CompositePresenter Presenter
        {
            get { return _presenter; }
        }

        public virtual ContentPresenter GetPlaceholder(string name)
        {
            return FindName(name) as ContentPresenter;
        }
    }
}
