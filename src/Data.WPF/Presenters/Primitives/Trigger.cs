using System;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Represents a trigger that can be attached to view element and perform actions conditionally.
    /// </summary>
    /// <typeparam name="T">The type of view element.</typeparam>
    public abstract class Trigger<T>
        where T : UIElement, new()
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Trigger{T}"/> class.
        /// </summary>
        protected Trigger()
        {
        }

        /// <summary>
        /// Gets or sets the action to be executed.
        /// </summary>
        protected internal Action<T> Action { get; set; }

        /// <summary>
        /// Attaches this trigger to view element.
        /// </summary>
        /// <param name="element">The view element.</param>
        protected internal abstract void Attach(T element);

        /// <summary>
        /// Detaches this trigger from view element.
        /// </summary>
        /// <param name="element">The view element.</param>
        protected internal abstract void Detach(T element);

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="element">The view element.</param>
        protected void Execute(T element)
        {
            if (Action != null)
                Action(element);
        }

        /// <summary>
        /// Sets the action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>This trigger for fluent coding.</returns>
        public Trigger<T> WithAction(Action<T> action)
        {
            Action = action;
            return this;
        }
    }
}
