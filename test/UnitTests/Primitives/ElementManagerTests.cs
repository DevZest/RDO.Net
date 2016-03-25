using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class ElementManagerTests : RowManagerTestsBase
    {
        #region Helpers
        private sealed class ConcreteElementManager : ElementManager
        {
            public ConcreteElementManager(DataSet dataSet)
                : base(dataSet)
            {
            }
        }

        private static ElementManager MockElementManager<T>(DataSet<T> dataSet, ScalarRepeatMode[] scalarItemsBefore, ScalarRepeatMode[] scalarItemsAfter)
            where T : Model, new()
        {
            return CreateElementManager<T>(dataSet, (builder, model) =>
            {
                builder.AddGridColumns("100").AddGridRows("100", "100", "100").WithOrientation(RepeatOrientation.XY);

                for (int i = 0; i < scalarItemsBefore.Length; i++)
                    builder.Range(0, 0).BeginScalarItem<UIElement>().Repeat(scalarItemsBefore[i]).End();

                builder.Range(0, 1).BeginRepeatItem<UIElement>().End();

                for (int i = 0; i < scalarItemsAfter.Length; i++)
                    builder.Range(0, 2).BeginScalarItem<UIElement>().Repeat(scalarItemsAfter[i]).End();
            });
        }

        private static ElementManager CreateElementManager<T>(DataSet<T> dataSet, Action<TemplateBuilder, T> buildTemplateAction)
            where T : Model, new()
        {
            var result = new ConcreteElementManager(dataSet);
            using (var templateBuilder = new TemplateBuilder(result.Template, dataSet.Model))
            {
                buildTemplateAction(templateBuilder, dataSet._);
            }
            result.Initialize();
            result.InitializeElements(null);
            return result;
        }

        private static T[] Array<T>(params T[] array)
        {
            return array ?? EmptyArray<T>.Singleton;
        }

        private static void VerifyElements(ElementManager elementManager, int[] scalarItemsBefore, int[] realizedRows, int[] scalarItemsAfter)
        {
            var template = elementManager.Template;
            var rows = elementManager.Rows;
            var elements = elementManager.Elements;
            Assert.AreEqual(scalarItemsBefore.Length + realizedRows.Length + scalarItemsAfter.Length, elements.Count);

            for (int i = 0; i < elements.Count; i++)
            {
                if (i < scalarItemsBefore.Length)
                {
                    var expectedIndex = scalarItemsBefore[i];
                    Assert.AreEqual(template.ScalarItems[expectedIndex], elements[i].GetTemplateItem());
                }
                else if (i < scalarItemsBefore.Length + realizedRows.Length)
                {
                    var expectedOrdinal = realizedRows[i - scalarItemsBefore.Length];
                    var expectedRow = rows[expectedOrdinal];
                    Assert.IsNotNull(expectedRow.View);
                    Assert.AreEqual(expectedRow.View, elements[i]);
                }
                else
                {
                    var expectedIndex = scalarItemsAfter[i - realizedRows.Length - scalarItemsBefore.Length];
                    Assert.AreEqual(template.ScalarItems[expectedIndex], elements[i].GetTemplateItem());
                }
            }
        }
        #endregion

        [TestMethod]
        public void ElementManager_Elements()
        {
            var dataSet = MockProductCategories(3);
            var elementManager = MockElementManager(dataSet,
                Array<ScalarRepeatMode>(ScalarRepeatMode.None, ScalarRepeatMode.Flow),
                Array<ScalarRepeatMode>(ScalarRepeatMode.Grow));
            var rows = elementManager.Rows;

            VerifyElements(elementManager, Array<int>(0, 1), Array<int>(), Array<int>(2));

            elementManager.FlowCount = 3;
            VerifyElements(elementManager, Array<int>(0, 1, 1, 1), Array<int>(), Array<int>(2));

            elementManager.RealizedRows.RealizeFirst(rows[1]);
            VerifyElements(elementManager, Array<int>(0, 1, 1, 1), Array<int>(1), Array<int>(2));

            elementManager.RealizedRows.RealizePrev();
            VerifyElements(elementManager, Array<int>(0, 1, 1, 1), Array<int>(0, 1), Array<int>(2));

            elementManager.RealizedRows.RealizeNext();
            VerifyElements(elementManager, Array<int>(0, 1, 1, 1), Array<int>(0, 1, 2), Array<int>(2));

            elementManager.FlowCount = 2;
            VerifyElements(elementManager, Array<int>(0, 1, 1), Array<int>(0, 1, 2), Array<int>(2));

            elementManager.RealizedRows.VirtualizeTop(1);
            VerifyElements(elementManager, Array<int>(0, 1, 1), Array<int>(1, 2), Array<int>(2));

            elementManager.RealizedRows.VirtualizeBottom(1);
            VerifyElements(elementManager, Array<int>(0, 1, 1), Array<int>(1), Array<int>(2));

            elementManager.RealizedRows.VirtualizeAll();
            VerifyElements(elementManager, Array<int>(0, 1, 1), Array<int>(), Array<int>(2));

            elementManager.ClearElements();
            Assert.IsNull(elementManager.Elements);
        }
    }
}
