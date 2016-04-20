namespace DevZest.Data.Windows.Factories
{
    public static class PlaceHolderFactory
    {
        public static TemplateBuilder DataElement(this TemplateItemBuilderFactory builderFactory, bool isMultidimensional = false, double desiredWidth = 0, double desiredHeight = 0)
        {
            return builderFactory.BeginDataItem<PlaceHolder>(isMultidimensional)
                .Initialize(x =>
                {
                    x.DesiredWidth = desiredWidth;
                    x.DesiredHeight = desiredHeight;
                })
                .End();
        }

        public static TemplateBuilder BlockElement(this TemplateItemBuilderFactory builderFactory, double desiredWidth = 0, double desiredHeight = 0)
        {
            return builderFactory.BeginBlockItem<PlaceHolder>()
                .Initialize(x =>
                {
                    x.DesiredWidth = desiredWidth;
                    x.DesiredHeight = desiredHeight;
                })
                .End();
        }

        public static TemplateBuilder RowElement(this TemplateItemBuilderFactory builderFactory, double desiredWidth = 0, double desiredHeight = 0)
        {
            return builderFactory.BeginRowItem<PlaceHolder>()
                .Initialize(x =>
                {
                    x.DesiredWidth = desiredWidth;
                    x.DesiredHeight = desiredHeight;
                })
                .End();
        }
    }
}
