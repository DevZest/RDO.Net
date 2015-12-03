using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace DevZest.Data
{
    [TestClass]
    public class DataSetTests : SimpleModelDataSetHelper
    {
        [TestMethod]
        public void DataSet_new_row()
        {
            int count = 3;
            var dataSet = GetDataSet(count);
            var model = dataSet._;
            var childModel = model.Child;
            var grandChildModel = childModel.Child;

            VerifyDataSet(dataSet, count);
            for (int i = 0; i < dataSet.Count; i++)
            {
                var children = dataSet[i].Children(childModel);
                VerifyDataSet(children, count);
                for (int j = 0; j < children.Count; j++)
                {
                    var grandChildren = children[j].Children(grandChildModel);
                    VerifyDataSet(grandChildren, count);
                }
            }

            Assert.AreEqual(count, model.DataSet.Count);
            Assert.AreEqual(count * count, childModel.DataSet.Count);
            Assert.AreEqual(count * count * count, grandChildModel.DataSet.Count);
        }

        private void VerifyDataSet(DataSet<SimpleModel> dataSet, int count)
        {
            Assert.AreEqual(count == 0, dataSet._.DesignMode);
            Assert.AreEqual(count, dataSet.Count);
            var model = dataSet._;
            for (int i = 0; i < count; i++)
                Assert.AreEqual(i, model.Id[dataSet[i]]);
        }

        [TestMethod]
        public void DataSet_clear_all_rows()
        {
            int count = 3;
            var dataSet = GetDataSet(count);

            var model = dataSet._;
            var childModel = model.Child;
            var grandChildModel = childModel.Child;
            var modelRows = ToList(model.DataSet);
            Assert.AreEqual(count, modelRows.Count);
            var childModelRows = ToList(childModel.DataSet);
            Assert.AreEqual(count * count, childModelRows.Count);
            var grandChildModelRows = ToList(grandChildModel.DataSet);
            Assert.AreEqual(count * count * count, grandChildModelRows.Count);

            dataSet.Clear();

            Assert.AreEqual(0, model.DataSet.Count);
            Assert.AreEqual(0, childModel.DataSet.Count);
            Assert.AreEqual(0, grandChildModel.DataSet.Count);

            VerifyDisposed(modelRows);
            VerifyDisposed(childModelRows);
            VerifyDisposed(grandChildModelRows);
        }

        [TestMethod]
        public void DataSet_remove_row()
        {
            int count = 3;
            var dataSet = GetDataSet(count);
            var model = dataSet._;

            var modelRows = new List<DataRow> { dataSet[0] };
            var childModelRows = new List<DataRow>();
            var grandChildModelRows = new List<DataRow>();
            {
                var children = dataSet[0].Children(model.Child);
                foreach (var child in children)
                {
                    childModelRows.Add(child);

                    var grandChildren = child.Children(model.Child.Child);
                    foreach (var grandChild in grandChildren)
                    {
                        Assert.AreEqual(child, grandChild.parentDataRow);
                        grandChildModelRows.Add(grandChild);
                    }
                }
            }
            Assert.AreEqual(1, modelRows.Count);
            Assert.AreEqual(count, childModelRows.Count);
            Assert.AreEqual(count * count, grandChildModelRows.Count);

            dataSet.Remove(dataSet[0]);

            VerifyDisposed(modelRows);
            VerifyDisposed(childModelRows);
            VerifyDisposed(grandChildModelRows);

            Assert.AreEqual(count - 1, model.DataSet.Count);
            for (int i = 0; i < dataSet.Count; i++)
                Assert.AreEqual(i + 1, model.Id[dataSet[i]]);
            Assert.AreEqual((count - 1) * count, model.Child.DataSet.Count);
            Assert.AreEqual((count - 1) * count * count, model.Child.Child.DataSet.Count);
        }

        private static List<DataRow> ToList(DataSet dataSet)
        {
            var result = new List<DataRow>();
            for (int i = 0; i < dataSet.Count; i++)
                result.Add(dataSet[i]);

            return result;
        }

        private static void VerifyDisposed(List<DataRow> rows)
        {
            foreach (var row in rows)
            {
                Assert.AreEqual(null, row.Model);
                Assert.AreEqual(null, row.parentDataRow);
            }
        }

        [TestMethod]
        public void DataSet_serialize_deserialize_json()
        {
            var result = DataSet<ProductCategory>.ParseJson(StringRes.ProductCategoriesJson);

            var childModel = result._.SubCategories;
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(3, result[0].Children(childModel).Count);
            Assert.AreEqual(14, result[1].Children(childModel).Count);
            Assert.AreEqual(8, result[2].Children(childModel).Count);
            Assert.AreEqual(12, result[3].Children(childModel).Count);

            Assert.AreEqual(StringRes.ProductCategoriesJson.Trim(), result.ToString().Trim());
        }

        [TestMethod]
        public void DataSet_DataRow_child_DataSet()
        {
            int count = 3;
            var dataSet = GetDataSet(count);

            Assert.AreEqual(count, dataSet[0][dataSet._.Child].Count);
        }

        [TestMethod]
        public void DataSet_Validate()
        {
            var dataSet = GetDataSet(3);
            Assert.IsTrue(dataSet.Model.Validators.Count == 2);
            Assert.IsFalse(dataSet.Validate());

            var validationResult = dataSet.GetValidationResult();
            var expectedJson =
@"[
   {
      ""DataRow"" : ""/[0]/Child[0]/Child[1]"",
      ""ValidatorId"" : ""DevZest.Data.Helpers.SimpleModel.IdMustBeEven"",
      ""ValidationLevel"" : 2,
      ""Columns"" : ""Id"",
      ""Description"" : ""The Id must be even.""
   },
   {
      ""DataRow"" : ""/[0]/Child[1]"",
      ""ValidatorId"" : ""DevZest.Data.Helpers.SimpleModel.IdMustBeEven"",
      ""ValidationLevel"" : 2,
      ""Columns"" : ""Id"",
      ""Description"" : ""The Id must be even.""
   },
   {
      ""DataRow"" : ""/[0]/Child[1]/Child[1]"",
      ""ValidatorId"" : ""DevZest.Data.Helpers.SimpleModel.IdMustBeEven"",
      ""ValidationLevel"" : 2,
      ""Columns"" : ""Id"",
      ""Description"" : ""The Id must be even.""
   },
   {
      ""DataRow"" : ""/[0]/Child[2]/Child[1]"",
      ""ValidatorId"" : ""DevZest.Data.Helpers.SimpleModel.IdMustBeEven"",
      ""ValidationLevel"" : 2,
      ""Columns"" : ""Id"",
      ""Description"" : ""The Id must be even.""
   },
   {
      ""DataRow"" : ""/[1]"",
      ""ValidatorId"" : ""DevZest.Data.Helpers.SimpleModel.IdMustBeEven"",
      ""ValidationLevel"" : 2,
      ""Columns"" : ""Id"",
      ""Description"" : ""The Id must be even.""
   },
   {
      ""DataRow"" : ""/[1]/Child[0]/Child[1]"",
      ""ValidatorId"" : ""DevZest.Data.Helpers.SimpleModel.IdMustBeEven"",
      ""ValidationLevel"" : 2,
      ""Columns"" : ""Id"",
      ""Description"" : ""The Id must be even.""
   },
   {
      ""DataRow"" : ""/[1]/Child[1]"",
      ""ValidatorId"" : ""DevZest.Data.Helpers.SimpleModel.IdMustBeEven"",
      ""ValidationLevel"" : 2,
      ""Columns"" : ""Id"",
      ""Description"" : ""The Id must be even.""
   },
   {
      ""DataRow"" : ""/[1]/Child[1]/Child[1]"",
      ""ValidatorId"" : ""DevZest.Data.Helpers.SimpleModel.IdMustBeEven"",
      ""ValidationLevel"" : 2,
      ""Columns"" : ""Id"",
      ""Description"" : ""The Id must be even.""
   },
   {
      ""DataRow"" : ""/[1]/Child[2]/Child[1]"",
      ""ValidatorId"" : ""DevZest.Data.Helpers.SimpleModel.IdMustBeEven"",
      ""ValidationLevel"" : 2,
      ""Columns"" : ""Id"",
      ""Description"" : ""The Id must be even.""
   },
   {
      ""DataRow"" : ""/[2]/Child[0]/Child[1]"",
      ""ValidatorId"" : ""DevZest.Data.Helpers.SimpleModel.IdMustBeEven"",
      ""ValidationLevel"" : 2,
      ""Columns"" : ""Id"",
      ""Description"" : ""The Id must be even.""
   },
   {
      ""DataRow"" : ""/[2]/Child[1]"",
      ""ValidatorId"" : ""DevZest.Data.Helpers.SimpleModel.IdMustBeEven"",
      ""ValidationLevel"" : 2,
      ""Columns"" : ""Id"",
      ""Description"" : ""The Id must be even.""
   },
   {
      ""DataRow"" : ""/[2]/Child[1]/Child[1]"",
      ""ValidatorId"" : ""DevZest.Data.Helpers.SimpleModel.IdMustBeEven"",
      ""ValidationLevel"" : 2,
      ""Columns"" : ""Id"",
      ""Description"" : ""The Id must be even.""
   },
   {
      ""DataRow"" : ""/[2]/Child[2]/Child[1]"",
      ""ValidatorId"" : ""DevZest.Data.Helpers.SimpleModel.IdMustBeEven"",
      ""ValidationLevel"" : 2,
      ""Columns"" : ""Id"",
      ""Description"" : ""The Id must be even.""
   }
]";
            Assert.AreEqual(expectedJson, validationResult.ToString());

            dataSet.UpdateValidationMessages(new ValidationResult());
            Assert.AreEqual("[]", dataSet.GetValidationResult().ToString());
            dataSet.UpdateValidationMessages(validationResult);
            Assert.AreEqual(expectedJson, dataSet.GetValidationResult().ToString());
        }

        [TestMethod]
        public void DataSet_Revision()
        {
            var dataSet = DataSet<SimpleModel>.New();

            {
                var revision0 = dataSet.Revision;
                dataSet.AddRow();
                var revision1 = dataSet.Revision;
                Assert.IsTrue(revision1 > revision0);

                dataSet._.Id[0] = 1;
                Assert.IsTrue(dataSet.Revision > revision1);
            }

            {
                var childMainSet = dataSet.Children(x => x.Child);
                var revisionMainSet0 = childMainSet.Revision;

                var childSet = dataSet.Children(x => x.Child, 0);
                var revisionChildSet0 = childSet.Revision;

                childSet.AddRow();
                var revisionMainSet1 = childMainSet.Revision;
                var revisionChildSet1 = childSet.Revision;

                Assert.IsTrue(revisionMainSet1 > revisionMainSet0);
                Assert.IsTrue(revisionChildSet1 > revisionChildSet0);

                childSet._.Id[0] = 2;
                Assert.IsTrue(childMainSet.Revision > revisionMainSet1);
                Assert.IsTrue(childSet.Revision > revisionChildSet1);
            }
        }

        [TestMethod]
        public void DataSet_MultiLevelProductCategories()
        {
            var productCategories = DataSet<ProductCategory>.ParseJson(StringRes.MultiLevelProductCategoryJson);
            Assert.AreEqual(StringRes.MultiLevelProductCategoryJson, productCategories.ToJsonString(true));
            Assert.AreEqual(2, productCategories.Count);
            Assert.AreEqual(3, productCategories.Children(x => x.SubCategories, 0).Count);
            Assert.AreEqual(2, productCategories.Children(x => x.SubCategories, 1).Count);
            Assert.AreEqual(3, productCategories.Children(x => x.SubCategories, 1).Children(x => x.SubCategories, 0).Count);
            Assert.AreEqual(3, productCategories.Children(x => x.SubCategories, 1).Children(x => x.SubCategories, 1).Count);
        }
    }
}
