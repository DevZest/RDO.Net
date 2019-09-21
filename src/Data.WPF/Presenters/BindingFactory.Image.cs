using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Presenters
{
    static partial class BindingFactory
    {
        /// <summary>
        /// Binds <see cref="ImageSource"/> column to <see cref="Image"/>.
        /// </summary>
        /// <param name="source">The source column.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<Image> BindToImage(this Column<ImageSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return new RowBinding<Image>(onRefresh: (v, p) =>
            {
                v.Source = p.GetValue(source);
            });
        }

        /// <summary>
        /// Binds <see cref="ImageSource"/> scalar data to <see cref="Image"/>.
        /// </summary>
        /// <param name="source">The source scalar data.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<Image> BindToImage(this Scalar<ImageSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return new ScalarBinding<Image>(onRefresh: (v, p) =>
            {
                v.Source = source.Value;
            });
        }
    }
}
