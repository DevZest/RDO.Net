using DevZest.Data.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SmoothScroll
{
    internal class FooList : DataPresenter<Foo>
    {
        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder.GridColumns("*")
            .GridRows("Auto")
            .Layout(Orientation.Vertical)
            .AddBinding(0, 0, _.BindTextBlock())
            .AddBinding(0, 0, _.BindBorder());
        }
    }
}
