using DevZest.Data.Presenters.Primitives;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public sealed class ExplicitTrigger<T> : Trigger<T>
        where T : UIElement, new()
    {
        public ExplicitTrigger()
        {
        }

        protected internal override void Attach(T element)
        {
        }

        protected internal override void Detach(T element)
        {
        }

        public new void Execute(T element)
        {
            base.Execute(element);
        }
    }
}
