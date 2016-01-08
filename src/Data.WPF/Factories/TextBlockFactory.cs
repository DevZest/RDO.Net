using System;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Factories
{
    public static class TextBlockFactory
    {
        public static DataSetPresenterBuilder TextBlock(this GridRangeConfig rangeConfig, Column column, Action<TextBlock> initializer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return rangeConfig.BeginListUnit<TextBlock>()
                .Initialize(initializer)
                .Bind(x => x.Text = x.GetSourceText(column).ToString())
                .End();
        }

        public static DataSetPresenterBuilder TextBlockLabel(this GridRangeConfig rangeConfig, Column column, Action<TextBlock> initializer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return rangeConfig.BeginListUnit<TextBlock>()
                .Initialize(initializer)
                .Bind(x => x.Text = column.DisplayName + ":")
                .End();
        }

        public static DataSetPresenterBuilder TextBlockName(this GridRangeConfig rangeConfig, Column column, Action<TextBlock> initializer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return rangeConfig.BeginListUnit<TextBlock>()
                .Initialize(initializer)
                .Bind(x => x.Text = column.DisplayName)
                .End();
        }
    }
}
