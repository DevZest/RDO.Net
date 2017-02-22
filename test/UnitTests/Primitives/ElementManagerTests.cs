using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class ElementManagerTests
    {
        [TestMethod]
        public void ElementManager_Elements()
        {
            var dataSet = DataSetMock.ProductCategories(8, false);
            var _ = dataSet._;
            ScalarBinding<ColumnHeader> columnHeader1 = null;
            BlockBinding<BlockHeader> blockHeader = null;
            RowBinding<TextBlock> textBlock = null;
            ScalarBinding<ColumnHeader> columnHeader2 = null;
            var elementManager = dataSet.CreateElementManager((builder) =>
            {
                columnHeader1 = _.Name.ColumnHeader();
                blockHeader = _.BlockHeader();
                textBlock = _.Name.TextBlock();
                columnHeader2 = _.Name.ColumnHeader().WithFlowable(true);
                builder.GridColumns("100", "100")
                    .GridRows("100", "100", "100")
                    .Layout(Orientation.Vertical, 0)
                    .AddBinding(1, 0, columnHeader1)
                    .AddBinding(0, 1, blockHeader)
                    .AddBinding(1, 1, textBlock)
                    .AddBinding(1, 2, columnHeader2);
            });

            {
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

                elementManager.FlowCount = 3;
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
                    Assert.AreNotEqual(columnHeader2[0], columnHeader2[1]);
                    Assert.AreNotEqual(columnHeader2[0], columnHeader2[2]);
                    Assert.AreNotEqual(columnHeader2[1], columnHeader2[2]);
                }

                elementManager.InternalContainerViewList.RealizeFirst(1);
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

                elementManager.InternalContainerViewList.RealizePrev();
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

                elementManager.InternalContainerViewList.RealizeNext();
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

                elementManager.FlowCount = 2;
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
                    Assert.AreNotEqual(columnHeader2[0], columnHeader2[1]);
                }

                elementManager.InternalContainerViewList.RealizeFirst(1);
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

                elementManager.InternalContainerViewList.RealizePrev();
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

                elementManager.InternalContainerViewList.RealizeNext();
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

                elementManager.InternalContainerViewList.VirtualizeAll();
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
            var dataSet = DataSetMock.ProductCategories(8, false);
            var _ = dataSet._;
            RowBinding<TextBlock> textBlock = null;
            var elementManager = dataSet.CreateElementManager((builder) =>
            {
                textBlock = _.Name.TextBlock();
                builder.GridColumns("100").GridRows("100").AddBinding(0, 0, textBlock);
            });

            {
                var template = elementManager.Template;
                var rows = elementManager.Rows;

                elementManager.InternalContainerViewList.RealizeFirst(1);
                dataSet._.Name[1] = "CHANGED NAME";
                Assert.AreEqual(dataSet._.Name[1], textBlock[rows[1]].Text);
            }
        }

        [TestMethod]
        public void ElementManager_RefreshElements_IsCurrent()
        {
            var dataSet = DataSetMock.ProductCategories(8, false);
            var _ = dataSet._;
            RowBinding<RowHeader> rowHeader = null;
            var elementManager = dataSet.CreateElementManager((builder) =>
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
            var dataSet = DataSetMock.ProductCategories(8, false);
            var _ = dataSet._;
            RowBinding<RowHeader> rowHeader = null;
            var elementManager = dataSet.CreateElementManager((builder) =>
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
