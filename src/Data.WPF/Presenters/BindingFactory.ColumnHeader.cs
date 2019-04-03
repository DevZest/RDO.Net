using DevZest.Data.Views;
using System;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static ScalarBinding<ColumnHeader> BindToColumnHeader(this Column column, object title = null)
        {
            return new ScalarBinding<ColumnHeader>(
                onRefresh: null,
                onCleanup: null,
                onSetup: e =>
                {
                    e.Column = column;
                    e.Content = title ?? column.DisplayShortName;
                });
        }

        public static ScalarBinding<ColumnHeader> BindToColumnHeader(this Func<string> header)
        {
            return new ScalarBinding<ColumnHeader>(
                onSetup: null,
                onCleanup: null,
                onRefresh: e =>
                {
                    e.Content = header();
                });
        }
    }
}
