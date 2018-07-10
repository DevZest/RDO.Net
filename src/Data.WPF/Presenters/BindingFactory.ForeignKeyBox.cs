using DevZest.Data.Views;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    partial class BindingFactory
    {
        public static RowBinding<ForeignKeyBox> BindToForeignKeyBox<TKey, TLookup>(this TKey key, TLookup lookup, Func<ColumnValueBag, TKey, string> toString)
            where TKey : PrimaryKey
            where TLookup : Projection
        {
            if (toString == null)
                throw new ArgumentNullException(nameof(toString));

            return BindToForeignKeyBox(key, lookup, (TextBlock v, ColumnValueBag valueBag, TKey paramKey, TLookup paramExt) =>
            {
                v.Text = toString(valueBag, paramKey);
            });
        }

        public static RowBinding<ForeignKeyBox> BindToForeignKeyBox<TKey, TLookup>(this TKey key, TLookup lookup, Func<ColumnValueBag, TKey, TLookup, string> toString)
            where TKey : PrimaryKey
            where TLookup : Projection
        {
            if (toString == null)
                throw new ArgumentNullException(nameof(toString));

            return BindToForeignKeyBox(key, lookup, (TextBlock v, ColumnValueBag valueBag, TKey paramKey, TLookup paramExt) =>
            {
                v.Text = toString(valueBag, paramKey, paramExt);
            });
        }

        public static RowBinding<ForeignKeyBox> BindToForeignKeyBox<TKey, TLookup, TView>(this TKey key, TLookup lookup, Action<TView, ColumnValueBag, TLookup> refreshAction)
            where TKey : PrimaryKey
            where TLookup : Projection
            where TView : UIElement, new()
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (lookup == null)
                throw new ArgumentNullException(nameof(lookup));
            if (refreshAction == null)
                throw new ArgumentNullException(nameof(refreshAction));

            return new RowBinding<ForeignKeyBox>(
                onSetup: (v, p) =>
                {
                    v.Content = new TView();
                    v.ForeignKey = key;
                    v.Lookup = lookup;
                },
                onRefresh: (v, p) => {
                    p.SetValueBag(v.ValueBag, v.ForeignKey, v.Lookup);
                    refreshAction((TView)v.Content, v.ValueBag, lookup);
                    },
                onCleanup: (v, p) => {
                    v.Content = null;
                }).WithInput(key, lookup);
        }

        public static RowBinding<ForeignKeyBox> BindToForeignKeyBox<TKey, TLookup, TView>(this TKey key, TLookup lookup, Action<TView, ColumnValueBag, TKey, TLookup> refreshAction)
            where TKey : PrimaryKey
            where TLookup : Projection
            where TView : UIElement, new()
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (lookup == null)
                throw new ArgumentNullException(nameof(lookup));
            if (refreshAction == null)
                throw new ArgumentNullException(nameof(refreshAction));

            return new RowBinding<ForeignKeyBox>(
                onSetup: (v, p) => {
                    v.Content = new TView();
                    v.ForeignKey = key;
                    v.Lookup = lookup;
                },
                onRefresh: (v, p) => {
                    p.SetValueBag(v.ValueBag, v.ForeignKey, v.Lookup);
                    refreshAction((TView)v.Content, v.ValueBag, key, lookup);
                },
                onCleanup: (v, p) => {
                    v.Content = null;
                }).WithInput(key, lookup);
        }

        private static RowBinding<ForeignKeyBox> WithInput(this RowBinding<ForeignKeyBox> rowBinding, PrimaryKey foreignKey, Projection lookup)
        {
            var rowInput = rowBinding.BeginInput(ForeignKeyBox.ValueBagProperty);
            foreach (var columnSort in foreignKey)
                rowInput.WithFlush(columnSort.Column, v => v.ValueBag);
            if (lookup != null)
            {
                foreach (var column in lookup.Columns)
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
