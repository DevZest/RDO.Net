﻿using DevZest.Data;
using DevZest.Data.Primitives;
using System;

namespace DevZest.Samples.AdventureWorksLT
{
    /// <remarks><see cref="SalesOrder"/> and <see cref="SalesOrderDetail"/> are chosen for having foreing key to non-existing table(s) and
    /// parent-child relationship.</remarks>
    public sealed class SalesOrderMockDb : MockDb<Db>
    {
        private static DataSet<SalesOrderHeader> Headers()
        {
            DataSet<SalesOrderHeader> result = DataSet<SalesOrderHeader>.Create().AddRows(4);
            SalesOrderHeader _ = result._;
            _.SuspendIdentity();
            _.SalesOrderID[0] = 1;
            _.SalesOrderID[1] = 2;
            _.SalesOrderID[2] = 3;
            _.SalesOrderID[3] = 4;
            _.RevisionNumber[0] = 2;
            _.RevisionNumber[1] = 2;
            _.RevisionNumber[2] = 2;
            _.RevisionNumber[3] = 2;
            _.OrderDate[0] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.OrderDate[1] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.OrderDate[2] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.OrderDate[3] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.DueDate[0] = Convert.ToDateTime("2008-06-13T00:00:00.000");
            _.DueDate[1] = Convert.ToDateTime("2008-06-13T00:00:00.000");
            _.DueDate[2] = Convert.ToDateTime("2008-06-13T00:00:00.000");
            _.DueDate[3] = Convert.ToDateTime("2008-06-13T00:00:00.000");
            _.ShipDate[0] = Convert.ToDateTime("2008-06-08T00:00:00.000");
            _.ShipDate[1] = Convert.ToDateTime("2008-06-08T00:00:00.000");
            _.ShipDate[2] = Convert.ToDateTime("2008-06-08T00:00:00.000");
            _.ShipDate[3] = Convert.ToDateTime("2008-06-08T00:00:00.000");
            _.Status[0] = SalesOrderStatus.Shipped;
            _.Status[1] = SalesOrderStatus.Shipped;
            _.Status[2] = SalesOrderStatus.Shipped;
            _.Status[3] = SalesOrderStatus.Shipped;
            _.OnlineOrderFlag[0] = false;
            _.OnlineOrderFlag[1] = false;
            _.OnlineOrderFlag[2] = false;
            _.OnlineOrderFlag[3] = false;
            _.PurchaseOrderNumber[0] = "PO348186287";
            _.PurchaseOrderNumber[1] = "PO19952192051";
            _.PurchaseOrderNumber[2] = "PO19604173239";
            _.PurchaseOrderNumber[3] = "PO19372114749";
            _.AccountNumber[0] = "10-4020-000609";
            _.AccountNumber[1] = "10-4020-000106";
            _.AccountNumber[2] = "10-4020-000340";
            _.AccountNumber[3] = "10-4020-000582";
            _.CustomerID[0] = 29847;
            _.CustomerID[1] = 30072;
            _.CustomerID[2] = 30113;
            _.CustomerID[3] = 29485;
            _.ShipToAddressID[0] = 1092;
            _.ShipToAddressID[1] = 640;
            _.ShipToAddressID[2] = 653;
            _.ShipToAddressID[3] = 1086;
            _.BillToAddressID[0] = 1092;
            _.BillToAddressID[1] = 640;
            _.BillToAddressID[2] = 653;
            _.BillToAddressID[3] = 1086;
            _.ShipMethod[0] = "CARGO TRANSPORT 5";
            _.ShipMethod[1] = "CARGO TRANSPORT 5";
            _.ShipMethod[2] = "CARGO TRANSPORT 5";
            _.ShipMethod[3] = "CARGO TRANSPORT 5";
            _.CreditCardApprovalCode[0] = null;
            _.CreditCardApprovalCode[1] = null;
            _.CreditCardApprovalCode[2] = null;
            _.CreditCardApprovalCode[3] = null;
            _.SubTotal[0] = 880.3484M;
            _.SubTotal[1] = 78.8100M;
            _.SubTotal[2] = 38418.6895M;
            _.SubTotal[3] = 39785.3304M;
            _.TaxAmt[0] = 70.4279M;
            _.TaxAmt[1] = 6.3048M;
            _.TaxAmt[2] = 3073.4952M;
            _.TaxAmt[3] = 3182.8264M;
            _.Freight[0] = 22.0087M;
            _.Freight[1] = 1.9703M;
            _.Freight[2] = 960.4672M;
            _.Freight[3] = 994.6333M;
            _.Comment[0] = null;
            _.Comment[1] = null;
            _.Comment[2] = null;
            _.Comment[3] = null;
            _.RowGuid[0] = new Guid("89e42cdc-8506-48a2-b89b-eb3e64e3554e");
            _.RowGuid[1] = new Guid("8a3448c5-e677-4158-a29b-dd33069be0b0");
            _.RowGuid[2] = new Guid("a47665d2-7ac9-4cf3-8a8b-2a3883554284");
            _.RowGuid[3] = new Guid("f1be45a5-5c57-4a50-93c6-5f8be44cb7cb");
            _.ModifiedDate[0] = Convert.ToDateTime("2008-06-08T00:00:00.000");
            _.ModifiedDate[1] = Convert.ToDateTime("2008-06-08T00:00:00.000");
            _.ModifiedDate[2] = Convert.ToDateTime("2008-06-08T00:00:00.000");
            _.ModifiedDate[3] = Convert.ToDateTime("2008-06-08T00:00:00.000");
            _.ResumeIdentity();
            return result;
        }

        private static DataSet<SalesOrderDetail> Details()
        {
            DataSet<SalesOrderDetail> result = DataSet<SalesOrderDetail>.Create().AddRows(32);
            SalesOrderDetail _ = result._;
            _.SuspendIdentity();
            _.SalesOrderID[0] = 1;
            _.SalesOrderID[1] = 1;
            _.SalesOrderID[2] = 2;
            _.SalesOrderID[3] = 3;
            _.SalesOrderID[4] = 3;
            _.SalesOrderID[5] = 3;
            _.SalesOrderID[6] = 3;
            _.SalesOrderID[7] = 3;
            _.SalesOrderID[8] = 3;
            _.SalesOrderID[9] = 3;
            _.SalesOrderID[10] = 3;
            _.SalesOrderID[11] = 3;
            _.SalesOrderID[12] = 3;
            _.SalesOrderID[13] = 3;
            _.SalesOrderID[14] = 3;
            _.SalesOrderID[15] = 3;
            _.SalesOrderID[16] = 3;
            _.SalesOrderID[17] = 3;
            _.SalesOrderID[18] = 3;
            _.SalesOrderID[19] = 3;
            _.SalesOrderID[20] = 3;
            _.SalesOrderID[21] = 3;
            _.SalesOrderID[22] = 3;
            _.SalesOrderID[23] = 3;
            _.SalesOrderID[24] = 3;
            _.SalesOrderID[25] = 3;
            _.SalesOrderID[26] = 3;
            _.SalesOrderID[27] = 3;
            _.SalesOrderID[28] = 3;
            _.SalesOrderID[29] = 3;
            _.SalesOrderID[30] = 3;
            _.SalesOrderID[31] = 3;
            _.SalesOrderDetailID[0] = 1;
            _.SalesOrderDetailID[1] = 2;
            _.SalesOrderDetailID[2] = 3;
            _.SalesOrderDetailID[3] = 4;
            _.SalesOrderDetailID[4] = 5;
            _.SalesOrderDetailID[5] = 6;
            _.SalesOrderDetailID[6] = 7;
            _.SalesOrderDetailID[7] = 8;
            _.SalesOrderDetailID[8] = 9;
            _.SalesOrderDetailID[9] = 10;
            _.SalesOrderDetailID[10] = 11;
            _.SalesOrderDetailID[11] = 12;
            _.SalesOrderDetailID[12] = 13;
            _.SalesOrderDetailID[13] = 14;
            _.SalesOrderDetailID[14] = 15;
            _.SalesOrderDetailID[15] = 16;
            _.SalesOrderDetailID[16] = 17;
            _.SalesOrderDetailID[17] = 18;
            _.SalesOrderDetailID[18] = 19;
            _.SalesOrderDetailID[19] = 20;
            _.SalesOrderDetailID[20] = 21;
            _.SalesOrderDetailID[21] = 22;
            _.SalesOrderDetailID[22] = 23;
            _.SalesOrderDetailID[23] = 24;
            _.SalesOrderDetailID[24] = 25;
            _.SalesOrderDetailID[25] = 26;
            _.SalesOrderDetailID[26] = 27;
            _.SalesOrderDetailID[27] = 28;
            _.SalesOrderDetailID[28] = 29;
            _.SalesOrderDetailID[29] = 30;
            _.SalesOrderDetailID[30] = 31;
            _.SalesOrderDetailID[31] = 32;
            _.OrderQty[0] = 1;
            _.OrderQty[1] = 1;
            _.OrderQty[2] = 1;
            _.OrderQty[3] = 4;
            _.OrderQty[4] = 2;
            _.OrderQty[5] = 6;
            _.OrderQty[6] = 2;
            _.OrderQty[7] = 1;
            _.OrderQty[8] = 1;
            _.OrderQty[9] = 1;
            _.OrderQty[10] = 4;
            _.OrderQty[11] = 2;
            _.OrderQty[12] = 4;
            _.OrderQty[13] = 1;
            _.OrderQty[14] = 6;
            _.OrderQty[15] = 1;
            _.OrderQty[16] = 2;
            _.OrderQty[17] = 3;
            _.OrderQty[18] = 1;
            _.OrderQty[19] = 2;
            _.OrderQty[20] = 2;
            _.OrderQty[21] = 3;
            _.OrderQty[22] = 3;
            _.OrderQty[23] = 2;
            _.OrderQty[24] = 3;
            _.OrderQty[25] = 5;
            _.OrderQty[26] = 3;
            _.OrderQty[27] = 1;
            _.OrderQty[28] = 2;
            _.OrderQty[29] = 1;
            _.OrderQty[30] = 7;
            _.OrderQty[31] = 1;
            _.ProductID[0] = 836;
            _.ProductID[1] = 822;
            _.ProductID[2] = 907;
            _.ProductID[3] = 905;
            _.ProductID[4] = 983;
            _.ProductID[5] = 988;
            _.ProductID[6] = 748;
            _.ProductID[7] = 990;
            _.ProductID[8] = 926;
            _.ProductID[9] = 743;
            _.ProductID[10] = 782;
            _.ProductID[11] = 918;
            _.ProductID[12] = 780;
            _.ProductID[13] = 937;
            _.ProductID[14] = 867;
            _.ProductID[15] = 985;
            _.ProductID[16] = 989;
            _.ProductID[17] = 991;
            _.ProductID[18] = 992;
            _.ProductID[19] = 993;
            _.ProductID[20] = 984;
            _.ProductID[21] = 986;
            _.ProductID[22] = 987;
            _.ProductID[23] = 981;
            _.ProductID[24] = 982;
            _.ProductID[25] = 783;
            _.ProductID[26] = 809;
            _.ProductID[27] = 810;
            _.ProductID[28] = 935;
            _.ProductID[29] = 925;
            _.ProductID[30] = 869;
            _.ProductID[31] = 880;
            _.UnitPrice[0] = 356.8980M;
            _.UnitPrice[1] = 356.8980M;
            _.UnitPrice[2] = 63.9000M;
            _.UnitPrice[3] = 218.4540M;
            _.UnitPrice[4] = 461.6940M;
            _.UnitPrice[5] = 112.9980M;
            _.UnitPrice[6] = 818.7000M;
            _.UnitPrice[7] = 323.9940M;
            _.UnitPrice[8] = 149.8740M;
            _.UnitPrice[9] = 809.7600M;
            _.UnitPrice[10] = 1376.9940M;
            _.UnitPrice[11] = 158.4300M;
            _.UnitPrice[12] = 1391.9940M;
            _.UnitPrice[13] = 48.5940M;
            _.UnitPrice[14] = 41.9940M;
            _.UnitPrice[15] = 112.9980M;
            _.UnitPrice[16] = 323.9940M;
            _.UnitPrice[17] = 323.9940M;
            _.UnitPrice[18] = 323.9940M;
            _.UnitPrice[19] = 323.9940M;
            _.UnitPrice[20] = 112.9980M;
            _.UnitPrice[21] = 112.9980M;
            _.UnitPrice[22] = 112.9980M;
            _.UnitPrice[23] = 461.6940M;
            _.UnitPrice[24] = 461.6940M;
            _.UnitPrice[25] = 1376.9940M;
            _.UnitPrice[26] = 37.1520M;
            _.UnitPrice[27] = 72.1620M;
            _.UnitPrice[28] = 24.2940M;
            _.UnitPrice[29] = 149.8740M;
            _.UnitPrice[30] = 41.9940M;
            _.UnitPrice[31] = 32.9940M;
            _.UnitPriceDiscount[0] = 0M;
            _.UnitPriceDiscount[1] = 0M;
            _.UnitPriceDiscount[2] = 0M;
            _.UnitPriceDiscount[3] = 0M;
            _.UnitPriceDiscount[4] = 0M;
            _.UnitPriceDiscount[5] = 0.4000M;
            _.UnitPriceDiscount[6] = 0M;
            _.UnitPriceDiscount[7] = 0M;
            _.UnitPriceDiscount[8] = 0M;
            _.UnitPriceDiscount[9] = 0M;
            _.UnitPriceDiscount[10] = 0M;
            _.UnitPriceDiscount[11] = 0M;
            _.UnitPriceDiscount[12] = 0M;
            _.UnitPriceDiscount[13] = 0M;
            _.UnitPriceDiscount[14] = 0M;
            _.UnitPriceDiscount[15] = 0.4000M;
            _.UnitPriceDiscount[16] = 0M;
            _.UnitPriceDiscount[17] = 0M;
            _.UnitPriceDiscount[18] = 0M;
            _.UnitPriceDiscount[19] = 0M;
            _.UnitPriceDiscount[20] = 0.4000M;
            _.UnitPriceDiscount[21] = 0.4000M;
            _.UnitPriceDiscount[22] = 0.4000M;
            _.UnitPriceDiscount[23] = 0M;
            _.UnitPriceDiscount[24] = 0M;
            _.UnitPriceDiscount[25] = 0M;
            _.UnitPriceDiscount[26] = 0M;
            _.UnitPriceDiscount[27] = 0M;
            _.UnitPriceDiscount[28] = 0M;
            _.UnitPriceDiscount[29] = 0M;
            _.UnitPriceDiscount[30] = 0M;
            _.UnitPriceDiscount[31] = 0M;
            _.RowGuid[0] = new Guid("e3a1994c-7a68-4ce8-96a3-77fdd3bbd730");
            _.RowGuid[1] = new Guid("5c77f557-fdb6-43ba-90b9-9a7aec55ca32");
            _.RowGuid[2] = new Guid("6dbfe398-d15d-425e-aa58-88178fe360e5");
            _.RowGuid[3] = new Guid("377246c9-4483-48ed-a5b9-e56f005364e0");
            _.RowGuid[4] = new Guid("43a54bcd-536d-4a1b-8e69-24d083507a14");
            _.RowGuid[5] = new Guid("12706fab-f3a2-48c6-b7c7-1ccde4081f18");
            _.RowGuid[6] = new Guid("b12f0d3b-5b4e-4f1f-b2f0-f7cde99dd826");
            _.RowGuid[7] = new Guid("f117a449-039d-44b8-a4b2-b12001dacc01");
            _.RowGuid[8] = new Guid("92e5052b-72d0-4c91-9a8c-42591803667e");
            _.RowGuid[9] = new Guid("8bd33bed-c4f6-4d44-84fb-a7d04afcd794");
            _.RowGuid[10] = new Guid("686999fb-42e6-4d00-9a14-83ffa86833e3");
            _.RowGuid[11] = new Guid("82940b03-c70b-4183-8660-6b3418908429");
            _.RowGuid[12] = new Guid("644b0cd6-b2c3-4e4d-ab43-091c2ef6c829");
            _.RowGuid[13] = new Guid("7f5feb17-8ef4-4236-9f1c-15046d9638f0");
            _.RowGuid[14] = new Guid("ac78838d-b503-41a5-9791-480e528f028c");
            _.RowGuid[15] = new Guid("2c10a282-a13d-442a-8f45-f4d6b23a7d9c");
            _.RowGuid[16] = new Guid("654fb79e-70df-4b92-9832-9fa67013215b");
            _.RowGuid[17] = new Guid("3d6ca7ab-055e-4536-8940-76234cc9bcde");
            _.RowGuid[18] = new Guid("560feee1-dd54-4c34-abb1-4f8841d0aa41");
            _.RowGuid[19] = new Guid("19570052-4023-4658-bc56-dc5c619bd00e");
            _.RowGuid[20] = new Guid("27562675-f8c3-4a38-bd9e-b366b83e5204");
            _.RowGuid[21] = new Guid("e193ce39-ef33-4969-87b1-468d2f7b48ad");
            _.RowGuid[22] = new Guid("e38e076f-5072-437a-a771-ada53b5ab803");
            _.RowGuid[23] = new Guid("26c00b7d-6e19-4fbf-b9f1-23c2609e8893");
            _.RowGuid[24] = new Guid("6666a81b-90a1-4204-a39e-9f660ca43e5f");
            _.RowGuid[25] = new Guid("332dcf9e-dfd2-4345-9015-f4b53ac396ee");
            _.RowGuid[26] = new Guid("c2b08405-a9be-4f71-906c-5d7b1e26bde4");
            _.RowGuid[27] = new Guid("351a29fb-ceb1-4ca6-bb36-506d87b82a95");
            _.RowGuid[28] = new Guid("1918cfd2-69e8-4593-b4d6-8677f18b8f62");
            _.RowGuid[29] = new Guid("21624302-ca0f-402f-8a46-5a3fffa7d5f3");
            _.RowGuid[30] = new Guid("169c75f6-a364-46e3-8ddb-033528177458");
            _.RowGuid[31] = new Guid("c3fbb3ec-3ff6-4ee1-88cf-230e128815f3");
            _.ModifiedDate[0] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[1] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[2] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[3] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[4] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[5] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[6] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[7] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[8] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[9] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[10] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[11] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[12] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[13] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[14] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[15] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[16] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[17] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[18] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[19] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[20] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[21] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[22] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[23] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[24] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[25] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[26] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[27] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[28] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[29] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[30] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ModifiedDate[31] = Convert.ToDateTime("2008-06-01T00:00:00.000");
            _.ResumeIdentity();
            return result;
        }

        protected override void Initialize()
        {
            // The order of mocking table does not matter, the dependencies will be sorted out automatically.
            Mock(Db.SalesOrderDetail, Details);
            Mock(Db.SalesOrderHeader, Headers);
        }
    }
}
