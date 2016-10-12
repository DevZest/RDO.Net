//using DevZest.Data.Windows.Primitives;

//namespace DevZest.Data.Windows.Factories
//{
//    public static class PlaceHolderFactory
//    {
//        public static ScalarBinding.Builder<PlaceHolder> ScalarBinding(this TemplateBuilder templateBuilder, bool isMultidimensional = false, double desiredWidth = 0, double desiredHeight = 0)
//        {
//            return templateBuilder.ScalarBinding<PlaceHolder>(isMultidimensional)
//                .OnSetup((x) =>
//                {
//                    x.DesiredWidth = desiredWidth;
//                    x.DesiredHeight = desiredHeight;
//                });
//        }

//        public static BlockBinding.Builder<PlaceHolder> BlockBinding(this TemplateBuilder templateBuilder, double desiredWidth = 0, double desiredHeight = 0)
//        {
//            return templateBuilder.BlockBinding<PlaceHolder>()
//                .OnSetup((x, ordinal, rows) =>
//                {
//                    x.DesiredWidth = desiredWidth;
//                    x.DesiredHeight = desiredHeight;
//                });
//        }

//        public static RowBinding.Builder<PlaceHolder> RowBinding(this TemplateBuilder templateBuilder, double desiredWidth = 0, double desiredHeight = 0)
//        {
//            return templateBuilder.RowBinding<PlaceHolder>()
//                .OnSetup((x, data) =>
//                {
//                    x.DesiredWidth = desiredWidth;
//                    x.DesiredHeight = desiredHeight;
//                });
//        }
//    }
//}
