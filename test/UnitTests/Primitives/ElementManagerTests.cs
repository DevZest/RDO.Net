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

        #endregion

        [TestMethod]
        public void ElementManager_Elements()
        {
            var dataSet = ProductCategoryDataSet.Mock(8, false);
            ScalarBinding<ColumnHeader> columnHeader1 = null;
            BlockBinding<BlockHeader> blockHeader = null;
            RowBinding<TextBlock> textBlock = null;
            ScalarBinding<ColumnHeader> columnHeader2 = null;
            var elementManager = CreateElementManager(dataSet, (builder, _) =>
            {
                columnHeader1 = _.Name.ColumnHeader();
                blockHeader = _.BlockHeader();
                textBlock = _.Name.TextBlock();
                columnHeader2 = _.Name.ColumnHeader().WithIsMultidimensional(true);
                builder.GridColumns("100", "100")
                    .GridRows("100", "100", "100")
                    .Layout(Orientation.Vertical, 0)
                    .AddBinding(1, 0, columnHeader1)
                    .AddBinding(0, 1, blockHeader)
                    .AddBinding(1, 1, textBlock)
                    .AddBinding(1, 2, columnHeader2);
            });

            {
                var _ = dataSet._;
                var template = elementManager.Template;
                var rows = elementManager.Rows;

                {
                    var elements = elementManager.Elements;
                    Assert.AreEqual(3, elements.Count);
                    Assert.AreEqual(columnHeader1[0], elements[0]);
                    var blockView = (BlockView)elements[1];
                    Assert.AreEqual(2, blockView.Elements.Count);
                    Assert.AreEqual(blockHeader[0], blockView.Elements[0]);
                    var rowView = (RowView)blockView.Elements[1];
                    Assert.AreEqual(1, rowView.Elements.Count);
                    Assert.AreEqual(textBlock[rows[0]], rowView.Elements[0]);
                    Assert.AreEqual(columnHeader2[0], elements[2]);
                }

                elementManager.BlockDimensions = 3;
                {
                    var elements = elementManager.Elements;
                    Assert.AreEqual(5, elements.Count);
                    Assert.AreEqual(columnHeader1[0], elements[0]);
                    var blockView = (BlockView)elements[1];
                    Assert.AreEqual(4, blockView.Elements.Count);
                    Assert.AreEqual(blockHeader[0], blockView.Elements[0]);
                    {
                        var rowView = (RowView)blockView.Elements[1];
                        Assert.AreEqual(1, rowView.Elements.Count);
                        Assert.AreEqual(textBlock[rows[0]], rowView.Elements[0]);
                    }
                    {
                        var rowView = (RowView)blockView.Elements[2];
                        Assert.AreEqual(1, rowView.Elements.Count);
                        Assert.AreEqual(textBlock[rows[1]], rowView.Elements[0]);
                    }
                    {
                        var rowView = (RowView)blockView.Elements[3];
                        Assert.AreEqual(1, rowView.Elements.Count);
                        Assert.AreEqual(textBlock[rows[2]], rowView.Elements[0]);
                    }
                    Assert.AreEqual(columnHeader2[0], elements[2]);
                    Assert.AreEqual(columnHeader2[1], elements[3]);
                    Assert.AreEqual(columnHeader2[2], elements[4]);
                }

                elementManager.ContainerViewList.RealizeFirst(1);
                {
                    var elements = elementManager.Elements;
                    Assert.AreEqual(6, elements.Count);
                    Assert.AreEqual(columnHeader1[0], elements[0]);
                    {
                        var blockView = (BlockView)elements[1];
                        Assert.AreEqual(4, blockView.Elements.Count);
                        Assert.AreEqual(blockHeader[0], blockView.Elements[0]);
                        {
                            var rowView = (RowView)blockView.Elements[1];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[0]], rowView.Elements[0]);
                        }
                        {
                            var rowView = (RowView)blockView.Elements[2];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[1]], rowView.Elements[0]);
                        }
                        {
                            var rowView = (RowView)blockView.Elements[3];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[2]], rowView.Elements[0]);
                        }
                    }
                    {
                        var blockView = (BlockView)elements[2];
                        Assert.AreEqual(4, blockView.Elements.Count);
                        Assert.AreEqual(blockHeader[1], blockView.Elements[0]);
                        {
                            var rowView = (RowView)blockView.Elements[1];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[3]], rowView.Elements[0]);
                        }
                        {
                            var rowView = (RowView)blockView.Elements[2];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[4]], rowView.Elements[0]);
                        }
                        {
                            var rowView = (RowView)blockView.Elements[3];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[5]], rowView.Elements[0]);
                        }
                    }
                    Assert.AreEqual(columnHeader2[0], elements[3]);
                    Assert.AreEqual(columnHeader2[1], elements[4]);
                    Assert.AreEqual(columnHeader2[2], elements[5]);
                }

                elementManager.ContainerViewList.RealizePrev();
                {
                    var elements = elementManager.Elements;
                    Assert.AreEqual(6, elements.Count);
                    Assert.AreEqual(columnHeader1[0], elements[0]);
                    {
                        var blockView = (BlockView)elements[1];
                        Assert.AreEqual(4, blockView.Elements.Count);
                        Assert.AreEqual(blockHeader[0], blockView.Elements[0]);
                        {
                            var rowView = (RowView)blockView.Elements[1];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[0]], rowView.Elements[0]);
                        }
                        {
                            var rowView = (RowView)blockView.Elements[2];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[1]], rowView.Elements[0]);
                        }
                        {
                            var rowView = (RowView)blockView.Elements[3];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[2]], rowView.Elements[0]);
                        }
                    }
                    {
                        var blockView = (BlockView)elements[2];
                        Assert.AreEqual(4, blockView.Elements.Count);
                        Assert.AreEqual(blockHeader[1], blockView.Elements[0]);
                        {
                            var rowView = (RowView)blockView.Elements[1];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[3]], rowView.Elements[0]);
                        }
                        {
                            var rowView = (RowView)blockView.Elements[2];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[4]], rowView.Elements[0]);
                        }
                        {
                            var rowView = (RowView)blockView.Elements[3];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[5]], rowView.Elements[0]);
                        }
                    }
                    Assert.AreEqual(columnHeader2[0], elements[3]);
                    Assert.AreEqual(columnHeader2[1], elements[4]);
                    Assert.AreEqual(columnHeader2[2], elements[5]);
                }

                elementManager.ContainerViewList.RealizeNext();
                {
                    var elements = elementManager.Elements;
                    Assert.AreEqual(7, elements.Count);
                    Assert.AreEqual(columnHeader1[0], elements[0]);
                    {
                        var blockView = (BlockView)elements[1];
                        Assert.AreEqual(4, blockView.Elements.Count);
                        Assert.AreEqual(blockHeader[0], blockView.Elements[0]);
                        {
                            var rowView = (RowView)blockView.Elements[1];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[0]], rowView.Elements[0]);
                        }
                        {
                            var rowView = (RowView)blockView.Elements[2];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[1]], rowView.Elements[0]);
                        }
                        {
                            var rowView = (RowView)blockView.Elements[3];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[2]], rowView.Elements[0]);
                        }
                    }
                    {
                        var blockView = (BlockView)elements[2];
                        Assert.AreEqual(4, blockView.Elements.Count);
                        Assert.AreEqual(blockHeader[1], blockView.Elements[0]);
                        {
                            var rowView = (RowView)blockView.Elements[1];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[3]], rowView.Elements[0]);
                        }
                        {
                            var rowView = (RowView)blockView.Elements[2];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[4]], rowView.Elements[0]);
                        }
                        {
                            var rowView = (RowView)blockView.Elements[3];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[5]], rowView.Elements[0]);
                        }
                    }
                    {
                        var blockView = (BlockView)elements[3];
                        Assert.AreEqual(3, blockView.Elements.Count);
                        Assert.AreEqual(blockHeader[2], blockView.Elements[0]);
                        {
                            var rowView = (RowView)blockView.Elements[1];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[6]], rowView.Elements[0]);
                        }
                        {
                            var rowView = (RowView)blockView.Elements[2];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[7]], rowView.Elements[0]);
                        }
                    }
                    Assert.AreEqual(columnHeader2[0], elements[4]);
                    Assert.AreEqual(columnHeader2[1], elements[5]);
                    Assert.AreEqual(columnHeader2[2], elements[6]);
                }

                elementManager.BlockDimensions = 2;
                {
                    var elements = elementManager.Elements;
                    Assert.AreEqual(4, elements.Count);
                    Assert.AreEqual(columnHeader1[0], elements[0]);
                    var blockView = (BlockView)elements[1];
                    Assert.AreEqual(3, blockView.Elements.Count);
                    Assert.AreEqual(blockHeader[0], blockView.Elements[0]);
                    {
                        var rowView = (RowView)blockView.Elements[1];
                        Assert.AreEqual(1, rowView.Elements.Count);
                        Assert.AreEqual(textBlock[rows[0]], rowView.Elements[0]);
                    }
                    {
                        var rowView = (RowView)blockView.Elements[2];
                        Assert.AreEqual(1, rowView.Elements.Count);
                        Assert.AreEqual(textBlock[rows[1]], rowView.Elements[0]);
                    }
                    Assert.AreEqual(columnHeader2[0], elements[2]);
                    Assert.AreEqual(columnHeader2[1], elements[3]);
                }

                elementManager.ContainerViewList.RealizeFirst(1);
                {
                    var elements = elementManager.Elements;
                    Assert.AreEqual(5, elements.Count);
                    Assert.AreEqual(columnHeader1[0], elements[0]);
                    {
                        var blockView = (BlockView)elements[1];
                        Assert.AreEqual(3, blockView.Elements.Count);
                        Assert.AreEqual(blockHeader[0], blockView.Elements[0]);
                        {
                            var rowView = (RowView)blockView.Elements[1];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[0]], rowView.Elements[0]);
                        }
                        {
                            var rowView = (RowView)blockView.Elements[2];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[1]], rowView.Elements[0]);
                        }
                    }
                    {
                        var blockView = (BlockView)elements[2];
                        Assert.AreEqual(3, blockView.Elements.Count);
                        Assert.AreEqual(blockHeader[1], blockView.Elements[0]);
                        {
                            var rowView = (RowView)blockView.Elements[1];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[2]], rowView.Elements[0]);
                        }
                        {
                            var rowView = (RowView)blockView.Elements[2];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[3]], rowView.Elements[0]);
                        }
                    }
                    Assert.AreEqual(columnHeader2[0], elements[3]);
                    Assert.AreEqual(columnHeader2[1], elements[4]);
                }

                elementManager.ContainerViewList.RealizePrev();
                {
                    var elements = elementManager.Elements;
                    Assert.AreEqual(5, elements.Count);
                    Assert.AreEqual(columnHeader1[0], elements[0]);
                    {
                        var blockView = (BlockView)elements[1];
                        Assert.AreEqual(3, blockView.Elements.Count);
                        Assert.AreEqual(blockHeader[0], blockView.Elements[0]);
                        {
                            var rowView = (RowView)blockView.Elements[1];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[0]], rowView.Elements[0]);
                        }
                        {
                            var rowView = (RowView)blockView.Elements[2];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[1]], rowView.Elements[0]);
                        }
                    }
                    {
                        var blockView = (BlockView)elements[2];
                        Assert.AreEqual(3, blockView.Elements.Count);
                        Assert.AreEqual(blockHeader[1], blockView.Elements[0]);
                        {
                            var rowView = (RowView)blockView.Elements[1];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[2]], rowView.Elements[0]);
                        }
                        {
                            var rowView = (RowView)blockView.Elements[2];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[3]], rowView.Elements[0]);
                        }
                    }
                    Assert.AreEqual(columnHeader2[0], elements[3]);
                    Assert.AreEqual(columnHeader2[1], elements[4]);
                }

                elementManager.ContainerViewList.RealizeNext();
                {
                    var elements = elementManager.Elements;
                    Assert.AreEqual(6, elements.Count);
                    Assert.AreEqual(columnHeader1[0], elements[0]);
                    {
                        var blockView = (BlockView)elements[1];
                        Assert.AreEqual(3, blockView.Elements.Count);
                        Assert.AreEqual(blockHeader[0], blockView.Elements[0]);
                        {
                            var rowView = (RowView)blockView.Elements[1];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[0]], rowView.Elements[0]);
                        }
                        {
                            var rowView = (RowView)blockView.Elements[2];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[1]], rowView.Elements[0]);
                        }
                    }
                    {
                        var blockView = (BlockView)elements[2];
                        Assert.AreEqual(3, blockView.Elements.Count);
                        Assert.AreEqual(blockHeader[1], blockView.Elements[0]);
                        {
                            var rowView = (RowView)blockView.Elements[1];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[2]], rowView.Elements[0]);
                        }
                        {
                            var rowView = (RowView)blockView.Elements[2];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[3]], rowView.Elements[0]);
                        }
                    }
                    {
                        var blockView = (BlockView)elements[3];
                        Assert.AreEqual(3, blockView.Elements.Count);
                        Assert.AreEqual(blockHeader[2], blockView.Elements[0]);
                        {
                            var rowView = (RowView)blockView.Elements[1];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[4]], rowView.Elements[0]);
                        }
                        {
                            var rowView = (RowView)blockView.Elements[2];
                            Assert.AreEqual(1, rowView.Elements.Count);
                            Assert.AreEqual(textBlock[rows[5]], rowView.Elements[0]);
                        }
                    }
                    Assert.AreEqual(columnHeader2[0], elements[4]);
                    Assert.AreEqual(columnHeader2[1], elements[5]);
                }

                elementManager.ContainerViewList.VirtualizeAll();
                {
                    var elements = elementManager.Elements;
                    Assert.AreEqual(4, elements.Count);
                    Assert.AreEqual(columnHeader1[0], elements[0]);
                    var blockView = (BlockView)elements[1];
                    Assert.AreEqual(3, blockView.Elements.Count);
                    Assert.AreEqual(blockHeader[0], blockView.Elements[0]);
                    {
                        var rowView = (RowView)blockView.Elements[1];
                        Assert.AreEqual(1, rowView.Elements.Count);
                        Assert.AreEqual(textBlock[rows[0]], rowView.Elements[0]);
                    }
                    {
                        var rowView = (RowView)blockView.Elements[2];
                        Assert.AreEqual(1, rowView.Elements.Count);
                        Assert.AreEqual(textBlock[rows[1]], rowView.Elements[0]);
                    }
                    Assert.AreEqual(columnHeader2[0], elements[2]);
                    Assert.AreEqual(columnHeader2[1], elements[3]);
                }

                elementManager.ClearElements();
                Assert.IsNull(elementManager.Elements);
            }
        }

        [TestMethod]
        public void ElementManager_RefreshElements()
        {
            var dataSet = ProductCategoryDataSet.Mock(8, false);
            RowBinding<TextBlock> textBlock = null;
            var elementManager = CreateElementManager(dataSet, (builder, _) =>
            {
                textBlock = _.Name.TextBlock();
                builder.GridColumns("100").GridRows("100").AddBinding(0, 0, textBlock);
            });

            {
                var _ = dataSet._;
                var template = elementManager.Template;
                var rows = elementManager.Rows;

                elementManager.ContainerViewList.RealizeFirst(1);
                dataSet._.Name[1] = "CHANGED NAME";
                Assert.AreEqual(dataSet._.Name[1], textBlock[rows[1]].Text);
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
