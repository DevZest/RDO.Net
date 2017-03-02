using DevZest.Windows.Data.Primitives;
using System.Windows;

namespace DevZest.Windows.Data
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
    }
}
