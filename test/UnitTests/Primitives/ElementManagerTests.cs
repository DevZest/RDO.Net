﻿using DevZest.Data.Windows.Helpers;
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
    public class ElementManagerTests
    {
        #region Helpers

        private sealed class ConcreteElementManager : ElementManager
        {
            public ConcreteElementManager(Template template, DataSet dataSet, _Boolean where = null, ColumnSort[] orderBy = null, bool emptyBlockViewList = false)
                : base(template, dataSet, where, orderBy, emptyBlockViewList)
            {
            }
        }

        private static ElementManager CreateElementManager<T>(DataSet<T> dataSet, Action<TemplateBuilder, T> buildTemplateAction)
            where T : Model, new()
        {
            var template = new Template();
            using (var templateBuilder = new TemplateBuilder(template, dataSet.Model))
            {
                buildTemplateAction(templateBuilder, dataSet._);
                templateBuilder.BlockView<AutoInitBlockView>()
                    .RowView<AutoInitRowView>();
            }
            var result = new ConcreteElementManager(template, dataSet);
            result.InitializeElements(null);
            return result;
        }

        private static void Verify(TextBlock textBlock, Binding binding, string text)
        {
            Assert.AreEqual(binding, textBlock.GetBinding());
            Assert.AreEqual(text, textBlock.Text);
        }

        #endregion

        [TestMethod]
        public void ElementManager_Elements()
        {
            var dataSet = ProductCategoryDataSet.Mock(8, false);
            var elementManager = CreateElementManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("100", "100")
                    .GridRows("100", "100", "100")
                    .Layout(Orientation.Vertical, 0)
                    .AddBinding(1, 0, _.Name.BindDisplayNameToTextBlock())
                    .AddBinding(0, 1, _.BindBlockOrdinalToTextBlock())
                    .AddBinding(1, 1, _.Name.TextBlock())
                    .AddBinding(1, 2, _.Name.BindDisplayNameToTextBlock().WithIsMultidimensional(true));
            });

            {
                var _ = dataSet._;
                var template = elementManager.Template;
                var rows = elementManager.Rows;

                elementManager.Elements
                    .Verify((TextBlock t) => Verify(t, template.ScalarBindings[0], _.Name.DisplayName))
                    .Verify((BlockView b) => b.Elements
                        .Verify((TextBlock y) => Verify(y, template.BlockBindings[0], "0"))
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[0].GetValue(_.Name)))
                            .VerifyEof())
                        .VerifyEof())
                    .Verify((TextBlock t) => Verify(t, template.ScalarBindings[1], _.Name.DisplayName))
                    .VerifyEof();

                elementManager.BlockDimensions = 3;
                elementManager.Elements
                    .Verify((TextBlock t) => Verify(t, template.ScalarBindings[0], _.Name.DisplayName))
                    .Verify((BlockView b) => b.Elements
                        .Verify((TextBlock y) => Verify(y, template.BlockBindings[0], "0"))
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[0].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[1].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[2].GetValue(_.Name)))
                            .VerifyEof())
                        .VerifyEof())
                    .Verify((TextBlock t) => Verify(t, template.ScalarBindings[1], _.Name.DisplayName))
                    .Verify((TextBlock t) => Verify(t, template.ScalarBindings[1], _.Name.DisplayName))
                    .Verify((TextBlock t) => Verify(t, template.ScalarBindings[1], _.Name.DisplayName))
                    .VerifyEof();

                elementManager.ContainerViewList.RealizeFirst(1);
                elementManager.Elements
                    .Verify((TextBlock t) => Verify(t, template.ScalarBindings[0], _.Name.DisplayName))
                    .Verify((BlockView b) => b.Elements
                        .Verify((TextBlock y) => Verify(y, template.BlockBindings[0], "0"))
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[0].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[1].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[2].GetValue(_.Name)))
                            .VerifyEof())
                        .VerifyEof())
                    .Verify((BlockView b) => b.Elements
                        .Verify((TextBlock y) => Verify(y, template.BlockBindings[0], "1"))
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[3].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[4].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[5].GetValue(_.Name)))
                            .VerifyEof())
                        .VerifyEof())
                    .Verify((TextBlock x) => Verify(x, template.ScalarBindings[1], _.Name.DisplayName))
                    .Verify((TextBlock x) => Verify(x, template.ScalarBindings[1], _.Name.DisplayName))
                    .Verify((TextBlock x) => Verify(x, template.ScalarBindings[1], _.Name.DisplayName))
                    .VerifyEof();

                elementManager.ContainerViewList.RealizePrev();
                elementManager.Elements
                    .Verify((TextBlock t) => Verify(t, template.ScalarBindings[0], _.Name.DisplayName))
                    .Verify((BlockView b) => b.Elements
                        .Verify((TextBlock y) => Verify(y, template.BlockBindings[0], "0"))
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[0].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[1].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[2].GetValue(_.Name)))
                            .VerifyEof())
                        .VerifyEof())
                    .Verify((BlockView b) => b.Elements
                        .Verify((TextBlock y) => Verify(y, template.BlockBindings[0], "1"))
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[3].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[4].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[5].GetValue(_.Name)))
                            .VerifyEof())
                        .VerifyEof())
                    .Verify((TextBlock x) => Verify(x, template.ScalarBindings[1], _.Name.DisplayName))
                    .Verify((TextBlock x) => Verify(x, template.ScalarBindings[1], _.Name.DisplayName))
                    .Verify((TextBlock x) => Verify(x, template.ScalarBindings[1], _.Name.DisplayName))
                    .VerifyEof();

                elementManager.ContainerViewList.RealizeNext();
                elementManager.Elements
                    .Verify((TextBlock t) => Verify(t, template.ScalarBindings[0], _.Name.DisplayName))
                    .Verify((BlockView b) => b.Elements
                        .Verify((TextBlock y) => Verify(y, template.BlockBindings[0], "0"))
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[0].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[1].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[2].GetValue(_.Name)))
                            .VerifyEof())
                        .VerifyEof())
                    .Verify((BlockView b) => b.Elements
                        .Verify((TextBlock y) => Verify(y, template.BlockBindings[0], "1"))
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[3].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[4].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[5].GetValue(_.Name)))
                            .VerifyEof())
                        .VerifyEof())
                    .Verify((BlockView b) => b.Elements
                        .Verify((TextBlock y) => Verify(y, template.BlockBindings[0], "2"))
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[6].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[7].GetValue(_.Name)))
                            .VerifyEof())
                        .VerifyEof())
                    .Verify((TextBlock x) => Verify(x, template.ScalarBindings[1], _.Name.DisplayName))
                    .Verify((TextBlock x) => Verify(x, template.ScalarBindings[1], _.Name.DisplayName))
                    .Verify((TextBlock x) => Verify(x, template.ScalarBindings[1], _.Name.DisplayName))
                    .VerifyEof();

                elementManager.BlockDimensions = 2;
                elementManager.Elements
                    .Verify((TextBlock t) => Verify(t, template.ScalarBindings[0], _.Name.DisplayName))
                    .Verify((BlockView b) => b.Elements
                        .Verify((TextBlock y) => Verify(y, template.BlockBindings[0], "0"))
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[0].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[1].GetValue(_.Name)))
                            .VerifyEof())
                        .VerifyEof())
                    .Verify((TextBlock t) => Verify(t, template.ScalarBindings[1], _.Name.DisplayName))
                    .Verify((TextBlock t) => Verify(t, template.ScalarBindings[1], _.Name.DisplayName))
                    .VerifyEof();

                elementManager.ContainerViewList.RealizeFirst(1);
                elementManager.Elements
                    .Verify((TextBlock t) => Verify(t, template.ScalarBindings[0], _.Name.DisplayName))
                    .Verify((BlockView b) => b.Elements
                        .Verify((TextBlock y) => Verify(y, template.BlockBindings[0], "0"))
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[0].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[1].GetValue(_.Name)))
                            .VerifyEof())
                        .VerifyEof())
                    .Verify((BlockView b) => b.Elements
                        .Verify((TextBlock y) => Verify(y, template.BlockBindings[0], "1"))
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[2].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[3].GetValue(_.Name)))
                            .VerifyEof())
                        .VerifyEof())
                    .Verify((TextBlock x) => Verify(x, template.ScalarBindings[1], _.Name.DisplayName))
                    .Verify((TextBlock x) => Verify(x, template.ScalarBindings[1], _.Name.DisplayName))
                    .VerifyEof();

                elementManager.ContainerViewList.RealizePrev();
                elementManager.Elements
                    .Verify((TextBlock t) => Verify(t, template.ScalarBindings[0], _.Name.DisplayName))
                    .Verify((BlockView b) => b.Elements
                        .Verify((TextBlock y) => Verify(y, template.BlockBindings[0], "0"))
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[0].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[1].GetValue(_.Name)))
                            .VerifyEof())
                        .VerifyEof())
                    .Verify((BlockView b) => b.Elements
                        .Verify((TextBlock y) => Verify(y, template.BlockBindings[0], "1"))
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[2].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[3].GetValue(_.Name)))
                            .VerifyEof())
                        .VerifyEof())
                    .Verify((TextBlock x) => Verify(x, template.ScalarBindings[1], _.Name.DisplayName))
                    .Verify((TextBlock x) => Verify(x, template.ScalarBindings[1], _.Name.DisplayName))
                    .VerifyEof();

                elementManager.ContainerViewList.RealizeNext();
                elementManager.Elements
                    .Verify((TextBlock t) => Verify(t, template.ScalarBindings[0], _.Name.DisplayName))
                    .Verify((BlockView b) => b.Elements
                        .Verify((TextBlock y) => Verify(y, template.BlockBindings[0], "0"))
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[0].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[1].GetValue(_.Name)))
                            .VerifyEof())
                        .VerifyEof())
                    .Verify((BlockView b) => b.Elements
                        .Verify((TextBlock y) => Verify(y, template.BlockBindings[0], "1"))
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[2].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[3].GetValue(_.Name)))
                            .VerifyEof())
                        .VerifyEof())
                    .Verify((BlockView b) => b.Elements
                        .Verify((TextBlock y) => Verify(y, template.BlockBindings[0], "2"))
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[4].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[5].GetValue(_.Name)))
                            .VerifyEof())
                        .VerifyEof())
                    .Verify((TextBlock x) => Verify(x, template.ScalarBindings[1], _.Name.DisplayName))
                    .Verify((TextBlock x) => Verify(x, template.ScalarBindings[1], _.Name.DisplayName))
                    .VerifyEof();

                elementManager.ContainerViewList.VirtualizeAll();
                elementManager.Elements
                    .Verify((TextBlock t) => Verify(t, template.ScalarBindings[0], _.Name.DisplayName))
                    .Verify((BlockView b) => b.Elements
                        .Verify((TextBlock y) => Verify(y, template.BlockBindings[0], "0"))
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[0].GetValue(_.Name)))
                            .VerifyEof())
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[1].GetValue(_.Name)))
                            .VerifyEof())
                        .VerifyEof())
                    .Verify((TextBlock x) => Verify(x, template.ScalarBindings[1], _.Name.DisplayName))
                    .Verify((TextBlock x) => Verify(x, template.ScalarBindings[1], _.Name.DisplayName))
                    .VerifyEof();

                elementManager.ClearElements();
                Assert.IsNull(elementManager.Elements);
            }
        }

        [TestMethod]
        public void ElementManager_RefreshElements()
        {
            var dataSet = ProductCategoryDataSet.Mock(8, false);
            var elementManager = CreateElementManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("100", "100")
                    .GridRows("100", "100", "100")
                    .Layout(Orientation.Vertical, 0)
                    .AddBinding(1, 0, _.Name.BindDisplayNameToTextBlock())
                    .AddBinding(0, 1, _.BindBlockOrdinalToTextBlock())
                    .AddBinding(1, 1, _.Name.TextBlock())
                    .AddBinding(1, 2, _.Name.BindDisplayNameToTextBlock().WithIsMultidimensional(true));
            });

            {
                var _ = dataSet._;
                var template = elementManager.Template;
                var rows = elementManager.Rows;

                elementManager.ContainerViewList.RealizeFirst(1);
                dataSet._.Name[1] = "CHANGED NAME";
                elementManager.Elements
                    .Verify((TextBlock t) => Verify(t, template.ScalarBindings[0], _.Name.DisplayName))
                    .Verify((BlockView b) => b.Elements
                        .Verify((TextBlock y) => Verify(y, template.BlockBindings[0], "0"))
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], rows[0].GetValue(_.Name)))
                            .VerifyEof())
                        .VerifyEof())
                    .Verify((BlockView b) => b.Elements
                        .Verify((TextBlock y) => Verify(y, template.BlockBindings[0], "1"))
                        .Verify((RowView r) => r.Elements
                            .Verify((TextBlock t) => Verify(t, template.RowBindings[0], "CHANGED NAME"))
                            .VerifyEof())
                        .VerifyEof())
                    .Verify((TextBlock x) => Verify(x, template.ScalarBindings[1], _.Name.DisplayName))
                    .VerifyEof();
            }
        }

        [TestMethod]
        public void ElementManager_RefreshElements_IsCurrent()
        {
            var dataSet = ProductCategoryDataSet.Mock(8, false);
            RowBinding<RowHeader> rowHeader = null;
            var elementManager = CreateElementManager(dataSet, (builder, _) =>
            {
                rowHeader = _.RowHeader();
                builder.GridColumns("100").GridRows("100").AddBinding(0, 0, rowHeader);
            });


            var template = elementManager.Template;
            var rows = elementManager.Rows;

            Assert.IsTrue(rows[0].IsCurrent);
            {
                var elements = elementManager.Elements;
                Assert.AreEqual(1, elements.Count);
                var rowView = (RowView)elements[0];
                Assert.AreEqual(1, rowView.Elements.Count);
                Assert.AreEqual(rowHeader[rows[0]], rowView.Elements[0]);
                Assert.IsTrue(rowHeader[rows[0]].IsCurrent);
            }

            elementManager.CurrentRow = rows[1];
            Assert.IsTrue(rows[1].IsCurrent);
            {
                var elements = elementManager.Elements;
                Assert.AreEqual(1, elements.Count);
                var rowView = (RowView)elements[0];
                Assert.AreEqual(1, rowView.Elements.Count);
                Assert.AreEqual(rowHeader[rows[1]], rowView.Elements[0]);
                Assert.IsTrue(rowHeader[rows[1]].IsCurrent);
            }
        }

        [TestMethod]
        public void ElementManager_RefreshElements_IsEditing()
        {
            var dataSet = ProductCategoryDataSet.Mock(8, false);
            RowBinding<RowHeader> rowHeader = null;
            var elementManager = CreateElementManager(dataSet, (builder, _) =>
            {
                rowHeader = _.RowHeader();
                builder.GridColumns("100").GridRows("100")
                    .AddBinding(0, 0, rowHeader);
            });

            var template = elementManager.Template;
            var rows = elementManager.Rows;

            Assert.IsFalse(rows[0].IsEditing);

            {
                var elements = elementManager.Elements;
                Assert.AreEqual(1, elements.Count);
                var rowView = (RowView)elements[0];
                Assert.AreEqual(1, rowView.Elements.Count);
                Assert.AreEqual(rowHeader[rows[0]], rowView.Elements[0]);
                Assert.IsFalse(rowHeader[rows[0]].IsEditing);
            }

            rows[0].BeginEdit();
            Assert.IsTrue(rows[0].IsEditing);
            Assert.IsTrue(rowHeader[rows[0]].IsEditing);
        }
    }
}
