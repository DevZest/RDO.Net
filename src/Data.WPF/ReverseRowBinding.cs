using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ReverseRowBinding<T> : ReverseBinding<T>
        where T : UIElement, new()
    {
        internal static ReverseRowBinding<T> Create<TData>(Input<T> input, Column<TData> column, Func<T, TData> dataGetter)
        {
            return new ReverseRowBinding<T>(input).Bind(column, dataGetter);
        }
            

        private ReverseRowBinding(Input<T> input)
            : base(input)
        {
        }

        private IColumnSet _columns = ColumnSet.Empty;
        private List<Action<RowPresenter, T>> _flushActions = new List<Action<RowPresenter, T>>();

        public ReverseRowBinding<T> Bind<TData>(Column<TData> column, Func<T, TData> dataGetter)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (dataGetter == null)
                throw new ArgumentNullException(nameof(dataGetter));

            VerifyNotSealed();
            _columns = _columns.Merge(column);
            _flushActions.Add((rowPresenter, element) => rowPresenter.EditValue(column, dataGetter(element)));
            return this;
        }

        public ReverseRowBinding<T> OnGetError(Func<T, ReverseBindingError> onGetError)
        {
            VerifyNotSealed();
            base._onGetError = onGetError;
            return this;
        }

        internal override IColumnSet Columns
        {
            get { return _columns; }
        }

        internal override void Flush(T element)
        {
            var rowPresenter = element.GetRowPresenter();
            foreach (var flushAction in _flushActions)
                flushAction(rowPresenter, element);
        }
    }
}
