using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class GridColumnSetTests
    {
        [TestMethod]
        public void GridColumnSet_Merge()
        {
            var template = new Template();
            template.AddGridColumns("Auto", "Auto", "Auto", "Auto", "Auto");
            var gridColumns = template.GridColumns;

            var set = GridColumnSet.Empty;
            VerifyGridColumnSet(set, gridColumns);

            set = set.Merge(gridColumns[4]);
            VerifyGridColumnSet(set, gridColumns, 4);

            set = set.Merge(gridColumns[3]);
            VerifyGridColumnSet(set, gridColumns, 3, 4);

            var set2 = GridColumnSet.Empty.Merge(gridColumns[2]).Merge(gridColumns[1]);
            VerifyGridColumnSet(set.Merge(set2), gridColumns, 1, 2, 3, 4);
        }

        private static void VerifyGridColumnSet(IGridColumnSet set, ReadOnlyCollection<GridColumn> gridColumns, params int[] ordinals)
        {
            Assert.AreEqual(ordinals.Length, set.Count);
            for (int i = 0; i < set.Count; i++)
                Assert.AreEqual(gridColumns[ordinals[i]], set[i]);
        }
    }
}
