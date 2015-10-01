using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Helpers
{
    internal static class ColumnCollectionExtensions
    {
        public static void Verify(this ColumnCollection columns, params Column[] expectedColumns)
        {
            Assert.AreEqual(expectedColumns.Length, columns.Count);
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var expectedColumn = expectedColumns[i];
                Assert.AreEqual(expectedColumn, column);
                Assert.AreEqual(i, expectedColumn.Ordinal);
            }
        }
    }
}
