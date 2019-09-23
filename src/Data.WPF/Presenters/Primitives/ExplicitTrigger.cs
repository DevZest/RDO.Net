using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Represents a trigger that will be executed explicitly.
    /// </summary>
    /// <typeparam name="T">The type of view element.</typeparam>
    public sealed class ExplicitTrigger<T> : Trigger<T>
        where T : UIElement, new()
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ExplicitTrigger{T}"/> class.
        /// </summary>
        public ExplicitTrigger()
        {
        }

        /// <inheritdoc/>
        protected internal override void Attach(T element)
        {
        }

        /// <inheritdoc/>
        protected internal override void Detach(T element)
        {
        }

        /// <summary>
        /// Executes the trigger explicitly.
        /// </summary>
        /// <param name="element">The view element.</param>
        public new void Execute(T element)
        {
            base.Execute(element);
        }
    }
}
