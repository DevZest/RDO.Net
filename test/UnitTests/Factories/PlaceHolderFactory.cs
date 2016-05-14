using DevZest.Data.Windows.Primitives;

namespace DevZest.Data.Windows.Factories
{
    public static class PlaceHolderFactory
    {
        public static ScalarItem.Builder<PlaceHolder> DataElement(this TemplateBuilder templateBuilder, bool isMultidimensional = false, double desiredWidth = 0, double desiredHeight = 0)
        {
            return templateBuilder.ScalarItem<PlaceHolder>(isMultidimensional)
                .Initialize(x =>
                {
                    x.DesiredWidth = desiredWidth;
                    x.DesiredHeight = desiredHeight;
                });
        }

        public static BlockItem.Builder<PlaceHolder> BlockElement(this TemplateBuilder templateBuilder, double desiredWidth = 0, double desiredHeight = 0)
        {
            return templateBuilder.BlockItem<PlaceHolder>()
                .Initialize(x =>
                {
                    x.DesiredWidth = desiredWidth;
                    x.DesiredHeight = desiredHeight;
                });
        }

        public static RowItem.Builder<PlaceHolder> RowElement(this TemplateBuilder templateBuilder, double desiredWidth = 0, double desiredHeight = 0)
        {
            return templateBuilder.RowItem<PlaceHolder>()
                .Initialize(x =>
                {
                    x.DesiredWidth = desiredWidth;
                    x.DesiredHeight = desiredHeight;
                });
        }
    }
}
