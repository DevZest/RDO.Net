using DevZest.Data.Views;
using System;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        /// <summary>
        /// Binds a column to <see cref="ColumnHeader"/>.
        /// </summary>
        /// <param name="column">The source column.</param>
        /// <param name="title">The title of <see cref="ColumnHeader"/>. If null, value of <see cref="Column.DisplayShortName"/> will be used.</param>
        /// <returns></returns>
        public static ScalarBinding<ColumnHeader> BindToColumnHeader(this Column column, object title = null)
        {
            return new ScalarBinding<ColumnHeader>(
                onRefresh: null,
                onCleanup: null,
                onSetup: v =>
                {
                    v.Column = column;
                    v.Content = title ?? column.DisplayShortName;
                });
        }

        /// <summary>
        /// Binds title getter to <see cref="ColumnHeader"/>.
        /// </summary>
        /// <param name="titleGetter">The title getter.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<ColumnHeader> BindToColumnHeader(this Func<string> titleGetter)
        {
            return new ScalarBinding<ColumnHeader>(
                onSetup: null,
                onCleanup: null,
                onRefresh: e =>
                {
                    e.Content = titleGetter();
                });
        }
    }
}
