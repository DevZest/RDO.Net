using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Represents a trigger that executes when data values changed.
    /// </summary>
    /// <typeparam name="T">The type of view element.</typeparam>
    public sealed class ValueChangedTrigger<T> : Trigger<T>
        where T : UIElement, new()
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ValueChangedTrigger{T}"/> class.
        /// </summary>
        /// <param name="columns">The columns.</param>
        /// <param name="rowBinding">The row binding.</param>
        public ValueChangedTrigger(IColumns columns, RowBinding<T> rowBinding)
        {
            _columns = columns.VerifyNotNull(nameof(columns));
            _rowBinding = rowBinding.VerifyNotNull(nameof(rowBinding));
        }

        private readonly IColumns _columns;
        private readonly RowBinding<T> _rowBinding;

        /// <inheritdoc/>
        protected internal override void Attach(T element)
        {
            var row = element.GetRowPresenter();
            row.ValueChanged += OnValueChanged;
        }

        /// <inheritdoc/>
        protected internal override void Detach(T element)
        {
            var row = element.GetRowPresenter();
            row.ValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.Columns.Overlaps(_columns))
            {
                var row = (RowPresenter)sender;
                Execute((T)_rowBinding[row]);
            }
        }
    }
}
