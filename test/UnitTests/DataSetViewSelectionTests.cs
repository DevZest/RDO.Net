﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    [TestClass]
    public class DataSetViewSelectionTests
    {
        private static void Verify(DataSetViewSelection result, int current, params int[] indexes)
        {
            Assert.AreEqual(indexes.Length, result.Count);
            for (int i = 0; i < result.Count; i++)
                Assert.AreEqual(result[i], indexes[i]);
        }

        private static void VerifyEmpty(DataSetViewSelection result)
        {
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void DataSetViewSelection_Empty()
        {
            var result = DataSetViewSelection.Empty;
            VerifyEmpty(result);
            result = result.Coerce(0);
            VerifyEmpty(result);
            result = result.Coerce(1);
            Verify(result, 0, 0);
        }

        [TestMethod]
        public void DataSetViewSelection_Single()
        {
            var result = DataSetViewSelection.Empty.Select(0, SelectionMode.Single);
            Verify(result, 0, 0);
            result = result.Select(1, SelectionMode.Single);
            Verify(result, 1, 1);
            result = result.Coerce(1);
            Verify(result, 0, 0);
        }

        [TestMethod]
        public void DataSetViewSelection_Extended()
        {
            var result = DataSetViewSelection.Empty.Select(10, SelectionMode.Extended);
            Verify(result, 10, 10);
            result = result.Select(12, SelectionMode.Extended);
            Verify(result, 10, 10, 11, 12);
            result = result.Select(8, SelectionMode.Extended);
            Verify(result, 10, 8, 9, 10);
            result = result.Coerce(9);
            Verify(result, 8, 8);
        }

        [TestMethod]
        public void DataSetViewSelection_Multiple()
        {
            var result = DataSetViewSelection.Empty.Select(1, SelectionMode.Multiple);
            Verify(result, 1, 1);
            result = result.Select(3, SelectionMode.Multiple);
            Verify(result, 3, 1, 3);
            result = result.Select(5, SelectionMode.Multiple);
            Verify(result, 5, 1, 3, 5);
            result = result.Select(3, SelectionMode.Multiple);
            Verify(result, 3, 1, 5);
            result = result.Coerce(3);
            Verify(result, 2, 1);
        }
    }
}
