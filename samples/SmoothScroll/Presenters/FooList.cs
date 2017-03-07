using DevZest.Windows.Data;
using SmoothScroll.Models;
using System.Windows.Controls;

namespace SmoothScroll.Presenters
{
    internal class FooList : DataPresenter<Foo>
    {
        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder
                /*** Vertical layout ***/
                .GridColumns("*")
                .GridRows("Auto")
                .Layout(Orientation.Vertical)
                /*** Flowable vertical layout ***/
                //.GridColumns("300")
                //.GridRows("Auto")
                //.Layout(Orientation.Vertical, 2)
                .AddBinding(0, 0, _.TextBlock())
                .AddBinding(0, 0, _.BindBorder());
        }
    }
}
