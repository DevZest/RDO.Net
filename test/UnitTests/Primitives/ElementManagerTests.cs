using DevZest.Data.Windows.Controls;
using DevZest.Data.Windows.Helpers;
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
            public ConcreteElementManager(Template template, DataSet dataSet)
                : base(template, dataSet)
            {
            }
        }

        private static ElementManager MockElementManager(DataSet<ProductCategory> dataSet)
        {
            return CreateElementManager(dataSet, (builder, model) => BuildTemplate(builder, model));
        }

        private static void BuildTemplate(TemplateBuilder builder, ProductCategory _)
        {
            builder.GridColumns("100", "100")
                .GridRows("100", "100", "100")
                .Layout(Orientation.Vertical, 0)
                .BlockView<AutoInitBlockView>()
                .RowView<AutoInitRowView>()
                .ScalarItem<TextBlock>()
                    .OnMount(v => v.Text = _.Name.DisplayName)
                .At(1, 0)
                .BlockItem<TextBlock>()
                    .OnRefresh((v, ordinal, rows) => v.Text = ordinal.ToString())
                .At(0, 1)
                .RowItem<TextBlock>()
                    .OnRefresh((v, p) => v.Text = p.GetValue(_.Name))
                .At(1, 1)
                .ScalarItem<TextBlock>(true)
                    .OnMount(v => v.Text = _.Name.DisplayName)
                .At(1, 2);
        }

        private static ElementManager CreateElementManager<T>(DataSet<T> dataSet, Action<TemplateBuilder, T> buildTemplateAction)
            where T : Model, new()
        {
            var template = new Template();
            using (var templateBuilder = new TemplateBuilder(template, dataSet.Model))
            {
                buildTemplateAction(templateBuilder, dataSet._);
            }
            var result = new ConcreteElementManager(template, dataSet);
            result.InitializeElements(null);
            return result;
        }

        private static void Verify(TextBlock textBlock, TemplateItem templateItem, string text)
        {
            Assert.AreEqual(templateItem, textBlock.GetTemplateItem());
            Assert.AreEqual(text, textBlock.Text);
        }

        #endregion

        [TestMethod]
        public void ElementManager_Elements()
        {
            var dataSet = MockProductCategories(8, false);
            var _ = dataSet._;
            var elementManager = MockElementManager(dataSet);
            var template = elementManager.Template;
            var rows = elementManager.Rows;

            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.ScalarItems[0], _.Name.DisplayName))
                .Verify((TextBlock t) => Verify(t, template.ScalarItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockDimensions = 3;
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.ScalarItems[0], _.Name.DisplayName))
                .Verify((TextBlock t) => Verify(t, template.ScalarItems[1], _.Name.DisplayName))
                .Verify((TextBlock t) => Verify(t, template.ScalarItems[1], _.Name.DisplayName))
                .Verify((TextBlock t) => Verify(t, template.ScalarItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.RealizeFirst(1);
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.ScalarItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[4].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[5].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.RealizePrev();
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.ScalarItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "0"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[0].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[1].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[2].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[4].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[5].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.RealizeNext();
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.ScalarItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "0"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[0].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[1].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[2].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[4].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[5].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "2"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[6].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[7].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockDimensions = 2;
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.ScalarItems[0], _.Name.DisplayName))
                .Verify((TextBlock t) => Verify(t, template.ScalarItems[1], _.Name.DisplayName))
                .Verify((TextBlock t) => Verify(t, template.ScalarItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.RealizeFirst(1);
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.ScalarItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[2].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.RealizePrev();
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.ScalarItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "0"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[0].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[1].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[2].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.RealizeNext();
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.ScalarItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "0"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[0].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[1].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[2].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "2"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[4].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[5].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.VirtualizeHead(1);
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.ScalarItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[2].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "2"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[4].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[5].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.VirtualizeTail(1);
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.ScalarItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[2].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.VirtualizeAll();
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.ScalarItems[0], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.ClearElements();
            Assert.IsNull(elementManager.Elements);
        }

        [TestMethod]
        public void ElementManager_RefreshElements()
        {
            var dataSet = MockProductCategories(8, false);
            var _ = dataSet._;
            var elementManager = MockElementManager(dataSet);
            var template = elementManager.Template;
            var rows = elementManager.Rows;

            elementManager.BlockViews.RealizeFirst(1);
            dataSet._.Name[1] = "CHANGED NAME";
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.ScalarItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], "CHANGED NAME"))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock x) => Verify(x, template.ScalarItems[1], _.Name.DisplayName))
                .VerifyEof();
        }
    }
}
