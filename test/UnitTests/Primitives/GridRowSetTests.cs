using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class GridRowSetTests
    {
        [TestMethod]
        public void GridRowSet_Merge()
        {
            var template = new Template(null);
            template.AddGridRows("Auto", "Auto", "Auto", "Auto", "Auto");
            var gridRows = template.GridRows;

            var set = GridRowSet.Empty;
            VerifyGridRowSet(set, gridRows);

            set = set.Merge(gridRows[4]);
            VerifyGridRowSet(set, gridRows, 4);

            set = set.Merge(gridRows[3]);
            VerifyGridRowSet(set, gridRows, 3, 4);

            var set2 = GridRowSet.Empty.Merge(gridRows[2]).Merge(gridRows[1]);
            VerifyGridRowSet(set.Merge(set2), gridRows, 1, 2, 3, 4);
        }

        private static void VerifyGridRowSet(IGridRowSet set, ReadOnlyCollection<GridRow> gridRows, params int[] ordinals)
        {
            Assert.AreEqual(ordinals.Length, set.Count);
            for (int i = 0; i < set.Count; i++)
                Assert.AreEqual(gridRows[ordinals[i]], set[i]);
        }
    }
}
