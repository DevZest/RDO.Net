using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Presenters
{
    static partial class BindingFactory
    {
        public static RowBinding<Image> BindToImage(this Column<ImageSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return new RowBinding<Image>(onRefresh: (v, p) =>
            {
                v.Source = p.GetValue(source);
            });
        }

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
