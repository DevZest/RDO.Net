using System;
using System.ComponentModel;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Represents a trigger that executes when dependency property of view element changed.
    /// </summary>
    /// <typeparam name="T">The type of view element.</typeparam>
    public sealed class PropertyChangedTrigger<T> : Trigger<T>
        where T : UIElement, new()
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PropertyChangedTrigger{T}"/> class.
        /// </summary>
        /// <param name="property">The dependency property.</param>
        public PropertyChangedTrigger(DependencyProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            _property = property;
        }

        DependencyProperty _property;

        /// <inheritdoc/>
        protected internal override void Attach(T element)
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(_property, element.GetType());
            dpd.AddValueChanged(element, OnPropertyChanged);
        }

        /// <inheritdoc/>
        protected internal override void Detach(T element)
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(_property, element.GetType());
            dpd.RemoveValueChanged(element, OnPropertyChanged);
        }

        private void OnPropertyChanged(object sender, EventArgs e)
        {
            Execute((T)sender);
        }
    }
}
