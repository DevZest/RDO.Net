//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;

//namespace DevZest.Data.MySql.Helpers
//{
//    internal static class ModelCollectionExtensions
//    {
//        public static void Verify(this ModelCollection models, params Model[] expectedModels)
//        {
//            Assert.AreEqual(expectedModels.Length, models.Count);
//            for (int i = 0; i < models.Count; i++)
//            {
//                var model = models[i];
//                var expectedModel = expectedModels[i];
//                Assert.AreEqual(expectedModel, model);
//                Assert.AreEqual(i, expectedModel.Ordinal);
//            }
//        }
//    }
//}
