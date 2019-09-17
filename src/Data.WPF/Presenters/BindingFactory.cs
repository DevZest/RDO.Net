using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Provides static binding factory extension methods.
    /// </summary>
    public static partial class BindingFactory
    {
        /// <summary>
        /// Binds to UI element as scalar binding.
        /// </summary>
        /// <typeparam name="T">Type of the UI element.</typeparam>
        /// <param name="source">The source presenter.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<T> BindTo<T>(this BasePresenter source)
            where T : UIElement, new()
        {
            return new ScalarBinding<T>(onRefresh: (Action<T>)null);
        }

        /// <summary>
        /// Binds to UI element as row binding.
        /// </summary>
        /// <typeparam name="T">Type of the UI element.</typeparam>
        /// <param name="source">The source model.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<T> BindTo<T>(this Model source)
            where T : UIElement, new()
        {
            return new RowBinding<T>(onRefresh: null);
        }
    }
}
