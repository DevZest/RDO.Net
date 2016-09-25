using DevZest.Data.Windows.Primitives;

namespace DevZest.Data.Windows.Factories
{
    public static class PlaceHolderFactory
    {
        public static ScalarItem.Builder<PlaceHolder> ScalarItem(this TemplateBuilder templateBuilder, bool isMultidimensional = false, double desiredWidth = 0, double desiredHeight = 0)
        {
            return templateBuilder.ScalarItem<PlaceHolder>(isMultidimensional)
                .OnSetup((x) =>
                {
                    x.DesiredWidth = desiredWidth;
                    x.DesiredHeight = desiredHeight;
                });
        }

        public static BlockItem.Builder<PlaceHolder> BlockItem(this TemplateBuilder templateBuilder, double desiredWidth = 0, double desiredHeight = 0)
        {
            return templateBuilder.BlockItem<PlaceHolder>()
                .OnSetup((x, ordinal, rows) =>
                {
                    x.DesiredWidth = desiredWidth;
                    x.DesiredHeight = desiredHeight;
                });
        }

        public static RowItem.Builder<PlaceHolder> RowItem(this TemplateBuilder templateBuilder, double desiredWidth = 0, double desiredHeight = 0)
        {
            return templateBuilder.RowItem<PlaceHolder>()
                .OnSetup((x, data) =>
                {
                    x.DesiredWidth = desiredWidth;
                    x.DesiredHeight = desiredHeight;
                });
        }
    }
}
