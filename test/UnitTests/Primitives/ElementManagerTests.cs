using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

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

        private static ElementManager MockElementManager(DataSet<ProductCategory> dataSet, bool[] dataItemsBefore, bool[] dataItemsAfter)
        {
            return CreateElementManager(dataSet, (builder, model) =>
            {
                for (int i = 0; i < dataItemsBefore.Length + dataItemsAfter.Length + 1; i++)
                    builder.AddGridRow("100");

                builder.AddGridColumns("100")
                    .Stack(Orientation.Vertical, 0)
                    .RowView((RowView rowView) => rowView.RowPresenter.InitializeElements(null),
                        (RowView rowView) => rowView.RowPresenter.ClearElements());

                int dataItemIndex = 0;
                for (int i = 0; i < dataItemsBefore.Length; i++)
                {
                    var index = dataItemIndex++;
                    var isRepeatable = dataItemsBefore[i];
                    builder.Range(0, index).BeginDataItem<TextBlock>()
                        .Repeat(isRepeatable)
                        .Bind((src, element) => element.Text = GetDataItemText(index))
                        .End();
                }

                builder.Range(0, dataItemIndex).BeginRowItem<TextBlock>()
                    .Bind((row, element) => element.Text = row.GetValue(model.Name))
                    .End();

                for (int i = 0; i < dataItemsAfter.Length; i++)
                {
                    var index = dataItemIndex++;
                    var isRepeatable = dataItemsAfter[i];
                    builder.Range(0, index + 1)
                    .BeginDataItem<TextBlock>()
                    .Repeat(isRepeatable)
                    .Bind((src, element) => element.Text = GetDataItemText(index))
                    .End();
                }
            });
        }

        private static string GetDataItemText(DataItem dataItem)
        {
            return GetDataItemText(dataItem.Ordinal);
        }

        private static string GetDataItemText(int dataItemIndex)
        {
            return string.Format(CultureInfo.InvariantCulture, "Data_{0}", dataItemIndex);
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

        private static void VerifyElements(ElementManager elementManager, int[] dataItemsBefore, int[] realizedRows, int[] dataItemsAfter)
        {
            VerifyAccumulatedStackDimensionsDelta(elementManager);
            var template = elementManager.Template;
            var rows = elementManager.Rows;
            var elements = elementManager.Elements;
            Assert.AreEqual(dataItemsBefore.Length + realizedRows.Length + dataItemsAfter.Length, elements.Count);

            for (int i = 0; i < elements.Count; i++)
            {
                if (i < dataItemsBefore.Length)
                {
                    var expectedIndex = dataItemsBefore[i];
                    Assert.AreEqual(template.DataItems[expectedIndex], elements[i].GetTemplateItem());
                }
                else if (i < dataItemsBefore.Length + realizedRows.Length)
                {
                    var expectedOrdinal = realizedRows[i - dataItemsBefore.Length];
                    var expectedRow = rows[expectedOrdinal];
                    Assert.IsNotNull(expectedRow.View);
                    Assert.AreEqual(expectedRow.View, elements[i]);
                }
                else
                {
                    var expectedIndex = dataItemsAfter[i - realizedRows.Length - dataItemsBefore.Length];
                    Assert.AreEqual(template.DataItems[expectedIndex], elements[i].GetTemplateItem());
                }
            }
        }

        private static void VerifyAccumulatedStackDimensionsDelta(ElementManager elementManager)
        {
            var template = elementManager.Template;
            var dataItems = template.DataItems;
            if (dataItems.Count == 0)
                return;
            var lastDataItem = dataItems[dataItems.Count - 1];
            Assert.AreEqual(elementManager.Elements.Count - elementManager.RealizedRows.Count,
                dataItems.Count * elementManager.StackDimensions - lastDataItem.AccumulatedStackDimensionsDelta);
        }

        private static void VerifyElements(ElementManager elementManager, ProductCategory productCategory)
        {
            var elements = elementManager.Elements;
            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                Assert.IsNotNull(element);
                if (element is RowView)
                {
                    var rowPresenter = ((RowView)element).RowPresenter;
                    var rowElements = rowPresenter.Elements;
                    Assert.AreEqual(1, rowElements.Count);
                    Assert.AreEqual(rowPresenter.GetValue(productCategory.Name), ((TextBlock)rowElements[0]).Text);
                }
                else
                {
                    var templateItem = element.GetTemplateItem();
                    Assert.IsNotNull(templateItem);
                    var dataItem = templateItem as DataItem;
                    Assert.IsNotNull(dataItem != null);
                    Assert.AreEqual(GetDataItemText(dataItem), ((TextBlock)element).Text);
                }
            }
        }
        #endregion

        [TestMethod]
        public void ElementManager_Elements()
        {
            var dataSet = MockProductCategories(3);
            var elementManager = MockElementManager(dataSet, Array<bool>(false, true), Array<bool>(false));
            var rows = elementManager.Rows;

            VerifyElements(elementManager, dataSet._);
            VerifyElements(elementManager, Array<int>(0, 1), Array<int>(), Array<int>(2));

            elementManager.StackDimensions = 3;
            VerifyElements(elementManager, dataSet._);
            VerifyElements(elementManager, Array<int>(0, 1, 1, 1), Array<int>(), Array<int>(2));

            elementManager.RealizedRows.RealizeFirst(rows[1]);
            VerifyElements(elementManager, dataSet._);
            VerifyElements(elementManager, Array<int>(0, 1, 1, 1), Array<int>(1), Array<int>(2));

            elementManager.RealizedRows.RealizePrev();
            VerifyElements(elementManager, dataSet._);
            VerifyElements(elementManager, Array<int>(0, 1, 1, 1), Array<int>(0, 1), Array<int>(2));

            elementManager.RealizedRows.RealizeNext();
            VerifyElements(elementManager, dataSet._);
            VerifyElements(elementManager, Array<int>(0, 1, 1, 1), Array<int>(0, 1, 2), Array<int>(2));

            elementManager.StackDimensions = 2;
            VerifyElements(elementManager, dataSet._);
            VerifyElements(elementManager, Array<int>(0, 1, 1), Array<int>(0, 1, 2), Array<int>(2));

            elementManager.RealizedRows.VirtualizeTop(1);
            VerifyElements(elementManager, dataSet._);
            VerifyElements(elementManager, Array<int>(0, 1, 1), Array<int>(1, 2), Array<int>(2));

            elementManager.RealizedRows.VirtualizeBottom(1);
            VerifyElements(elementManager, dataSet._);
            VerifyElements(elementManager, Array<int>(0, 1, 1), Array<int>(1), Array<int>(2));

            elementManager.RealizedRows.VirtualizeAll();
            VerifyElements(elementManager, dataSet._);
            VerifyElements(elementManager, Array<int>(0, 1, 1), Array<int>(), Array<int>(2));

            elementManager.ClearElements();
            Assert.IsNull(elementManager.Elements);
        }

        [TestMethod]
        public void ElementManager_RefreshElements()
        {
            var dataSet = MockProductCategories(3);
            var elementManager = MockElementManager(dataSet, Array<bool>(false, true), Array<bool>(false));
            var rows = elementManager.Rows;

            elementManager.RefreshElements();
            VerifyElements(elementManager, dataSet._);

            elementManager.StackDimensions = 3;
            elementManager.RefreshElements();
            VerifyElements(elementManager, dataSet._);

            elementManager.RealizedRows.RealizeFirst(rows[1]);
            elementManager.RefreshElements();
            VerifyElements(elementManager, dataSet._);

            elementManager.RealizedRows.RealizePrev();
            elementManager.RefreshElements();
            VerifyElements(elementManager, dataSet._);

            elementManager.RealizedRows.RealizeNext();
            elementManager.RefreshElements();
            VerifyElements(elementManager, dataSet._);

            elementManager.StackDimensions = 2;
            elementManager.RefreshElements();
            VerifyElements(elementManager, dataSet._);

            elementManager.RealizedRows.VirtualizeTop(1);
            elementManager.RefreshElements();
            VerifyElements(elementManager, dataSet._);

            elementManager.RealizedRows.VirtualizeBottom(1);
            elementManager.RefreshElements();
            VerifyElements(elementManager, dataSet._);

            elementManager.RealizedRows.VirtualizeAll();
            elementManager.RefreshElements();
            VerifyElements(elementManager, dataSet._);
        }
    }
}
