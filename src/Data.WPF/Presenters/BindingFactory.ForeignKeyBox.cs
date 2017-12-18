using DevZest.Data.Views;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    partial class BindingFactory
    {
        public static RowBinding<ForeignKeyBox> AsForeignKeyBox<TKey, TExtender>(this TKey key, TExtender extender, Func<ColumnValueBag, TKey, string> toString)
            where TKey : PrimaryKey
            where TExtender : ModelExtender
        {
            if (toString == null)
                throw new ArgumentNullException(nameof(toString));

            return AsForeignKeyBox(key, extender, (TextBlock v, ColumnValueBag valueBag, TKey paramKey, TExtender paramExt) =>
            {
                v.Text = toString(valueBag, paramKey);
            });
        }

        public static RowBinding<ForeignKeyBox> AsForeignKeyBox<TKey, TExtender>(this TKey key, TExtender extender, Func<ColumnValueBag, TKey, TExtender, string> toString)
            where TKey : PrimaryKey
            where TExtender : ModelExtender
        {
            if (toString == null)
                throw new ArgumentNullException(nameof(toString));

            return AsForeignKeyBox(key, extender, (TextBlock v, ColumnValueBag valueBag, TKey paramKey, TExtender paramExt) =>
            {
                v.Text = toString(valueBag, paramKey, paramExt);
            });
        }

        public static RowBinding<ForeignKeyBox> AsForeignKeyBox<TKey, TExtender, TView>(this TKey key, TExtender extender, Action<TView, ColumnValueBag, TExtender> refreshAction)
            where TKey : PrimaryKey
            where TExtender : ModelExtender
            where TView : UIElement, new()
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (extender == null)
                throw new ArgumentNullException(nameof(extender));
            if (refreshAction == null)
                throw new ArgumentNullException(nameof(refreshAction));

            return new RowBinding<ForeignKeyBox>(
                onSetup: (v, p) =>
                {
                    v.Content = new TView();
                    v.ForeignKey = key;
                    v.Extender = extender;
                },
                onRefresh: (v, p) => {
                    p.SetValueBag(v.ValueBag, v.ForeignKey, v.Extender);
                    refreshAction((TView)v.Content, v.ValueBag, extender);
                    },
                onCleanup: (v, p) => {
                    v.Content = null;
                }).WithInput(key, extender);
        }

        public static RowBinding<ForeignKeyBox> AsForeignKeyBox<TKey, TExtender, TView>(this TKey key, TExtender extender, Action<TView, ColumnValueBag, TKey, TExtender> refreshAction)
            where TKey : PrimaryKey
            where TExtender : ModelExtender
            where TView : UIElement, new()
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (extender == null)
                throw new ArgumentNullException(nameof(extender));
            if (refreshAction == null)
                throw new ArgumentNullException(nameof(refreshAction));

            return new RowBinding<ForeignKeyBox>(
                onSetup: (v, p) => {
                    v.Content = new TView();
                    v.ForeignKey = key;
                    v.Extender = extender;
                },
                onRefresh: (v, p) => {
                    p.SetValueBag(v.ValueBag, v.ForeignKey, v.Extender);
                    refreshAction((TView)v.Content, v.ValueBag, key, extender);
                },
                onCleanup: (v, p) => {
                    v.Content = null;
                }).WithInput(key, extender);
        }

        private static RowBinding<ForeignKeyBox> WithInput(this RowBinding<ForeignKeyBox> rowBinding, PrimaryKey foreignKey, ModelExtender extender)
        {
            var rowInput = rowBinding.BeginInput(ForeignKeyBox.ValueBagProperty);
            foreach (var columnSort in foreignKey)
                rowInput.WithFlush(columnSort.Column, v => v.ValueBag);
            if (extender != null)
            {
                foreach (var column in extender.Columns)
                {
                    if (column.IsExpression)
                        continue;
                    rowInput.WithFlush(column, v => v.ValueBag);
                }
            }
            return rowInput.EndInput();
        }
    }
}
