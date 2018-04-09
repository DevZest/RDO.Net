using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public sealed class ValueChangedTrigger<T> : Trigger<T>
        where T : UIElement, new()
    {
        public ValueChangedTrigger(IColumns columns, RowBinding<T> rowBinding)
        {
            Check.NotNull(columns, nameof(columns));
            Check.NotNull(rowBinding, nameof(rowBinding));

            _columns = columns;
            _rowBinding = rowBinding;
        }

        private readonly IColumns _columns;
        private readonly RowBinding<T> _rowBinding;

        protected internal override void Attach(T element)
        {
            var row = element.GetRowPresenter();
            row.ValueChanged += OnValueChanged;
        }

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
