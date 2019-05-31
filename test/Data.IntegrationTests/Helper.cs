using DevZest.Samples.AdventureWorksLT;
using System;

namespace DevZest.Data
{
    internal static class Helper
    {
        internal static void AddTestDataRows(this DataSet<SalesOrder> salesOrder, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var ordinal = salesOrder.IndexOf(salesOrder.AddRow());

                salesOrder._.DueDate[ordinal] = DateTime.Now;
                salesOrder._.CustomerID[ordinal] = ordinal + 1;
                salesOrder._.ShipMethod[ordinal] = "TRUCK" + (ordinal + 1).ToString();
            }
        }
    }
}
