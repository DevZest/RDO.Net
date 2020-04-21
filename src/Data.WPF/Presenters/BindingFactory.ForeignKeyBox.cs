using DevZest.Data.Views;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    partial class BindingFactory
    {
        /// <summary>
        /// Binds a foreign key to <see cref="ForeignKeyBox"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the foreign key.</typeparam>
        /// <typeparam name="TLookup">The type of the lookup projection.</typeparam>
        /// <param name="key">The foreign key.</param>
        /// <param name="lookup">The lookup projection.</param>
        /// <param name="titleGetter">The getter that returns title string.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<ForeignKeyBox> BindToForeignKeyBox<TKey, TLookup>(this TKey key, TLookup lookup, Func<ColumnValueBag, TKey, string> titleGetter)
            where TKey : CandidateKey
            where TLookup : Projection
        {
            if (titleGetter == null)
                throw new ArgumentNullException(nameof(titleGetter));

            return BindToForeignKeyBox(key, lookup, (TextBlock v, ColumnValueBag valueBag, TKey paramKey, TLookup paramExt) =>
            {
                v.Text = titleGetter(valueBag, paramKey);
            });
        }

        /// <summary>
        /// Binds a foreign key to <see cref="ForeignKeyBox"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the foreign key.</typeparam>
        /// <typeparam name="TLookup">The type of the lookup projection.</typeparam>
        /// <param name="key">The foreign key.</param>
        /// <param name="lookup">The lookup projection.</param>
        /// <param name="titleGetter">The getter that returns title string.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<ForeignKeyBox> BindToForeignKeyBox<TKey, TLookup>(this TKey key, TLookup lookup, Func<ColumnValueBag, TKey, TLookup, string> titleGetter)
            where TKey : CandidateKey
            where TLookup : Projection
        {
            if (titleGetter == null)
                throw new ArgumentNullException(nameof(titleGetter));

            return BindToForeignKeyBox(key, lookup, (TextBlock v, ColumnValueBag valueBag, TKey paramKey, TLookup paramExt) =>
            {
                v.Text = titleGetter(valueBag, paramKey, paramExt);
            });
        }

        /// <summary>
        /// Binds a foreign key to <see cref="ForeignKeyBox"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the foreign key.</typeparam>
        /// <typeparam name="TLookup">The type of the lookup projection.</typeparam>
        /// <typeparam name="TView">The type of the content view to display lookup data.</typeparam>
        /// <param name="key">The foreign key.</param>
        /// <param name="lookup">The lookup projection.</param>
        /// <param name="refreshAction">The action to refresh the content view.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<ForeignKeyBox> BindToForeignKeyBox<TKey, TLookup, TView>(this TKey key, TLookup lookup, Action<TView, ColumnValueBag, TLookup> refreshAction)
            where TKey : CandidateKey
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

        /// <summary>
        /// Binds a foreign key to <see cref="ForeignKeyBox"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the foreign key.</typeparam>
        /// <typeparam name="TLookup">The type of the lookup projection.</typeparam>
        /// <typeparam name="TView">The type of the content view to display lookup data.</typeparam>
        /// <param name="key">The foreign key.</param>
        /// <param name="lookup">The lookup projection.</param>
        /// <param name="refreshAction">The action to refresh the content view.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<ForeignKeyBox> BindToForeignKeyBox<TKey, TLookup, TView>(this TKey key, TLookup lookup, Action<TView, ColumnValueBag, TKey, TLookup> refreshAction)
            where TKey : CandidateKey
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

        private static RowBinding<ForeignKeyBox> WithInput(this RowBinding<ForeignKeyBox> rowBinding, CandidateKey foreignKey, Projection lookup)
        {
            var rowInput = rowBinding.BeginInput(ForeignKeyBox.ValueBagProperty);
            foreach (var columnSort in foreignKey)
                rowInput.WithFlush(columnSort.Column, v => v.ValueBag);
            if (lookup != null)
            {
                foreach (var column in lookup.GetColumns())
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
