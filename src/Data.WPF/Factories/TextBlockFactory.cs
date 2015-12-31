using System;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Factories
{
    public static class TextBlockFactory
    {
        public static DataSetPresenterBuilder TextBlock(this DataSetPresenterBuilderRange builderRange, Column column, Action<TextBlock> initializer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return builderRange.BeginListEntry<TextBlock>()
                .Initialize(initializer)
                .Bind(x => x.Text = x.GetSourceText(column).ToString())
                .End();
        }

        public static DataSetPresenterBuilder TextBlockLabel(this DataSetPresenterBuilderRange builderRange, Column column, Action<TextBlock> initializer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return builderRange.BeginListEntry<TextBlock>()
                .Initialize(initializer)
                .Bind(x => x.Text = column.DisplayName + ":")
                .End();
        }

        public static DataSetPresenterBuilder TextBlockName(this DataSetPresenterBuilderRange builderRange, Column column, Action<TextBlock> initializer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return builderRange.BeginListEntry<TextBlock>()
                .Initialize(initializer)
                .Bind(x => x.Text = column.DisplayName)
                .End();
        }
    }
}
