using DevZest.Samples.AdventureWorksLT;
using System.Globalization;

namespace DevZest.Data.Windows
{
    internal static class ProductCategoryDataSet
    {
        public static DataSet<ProductCategory> Mock(int count, bool multiLevel = true)
        {
            var dataSet = DataSet<ProductCategory>.New();

            string namePrefix = "Name";
            AddRows(dataSet, namePrefix, count);
            if (multiLevel)
            {
                for (int i = 0; i < dataSet.Count; i++)
                {
                    var children = dataSet.SubCategories(i);
                    AddRows(children, GetName(namePrefix, i), count);
                    for (int j = 0; j < children.Count; j++)
                    {
                        var grandChildren = children.SubCategories(j);
                        AddRows(grandChildren, GetName(GetName(namePrefix, i), j), count);
                    }
                }
            }

            return dataSet;
        }

        private static void AddRows(DataSet<ProductCategory> dataSet, string namePrefix, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet.AddRow();
                dataSet._.Name[dataRow] = GetName(namePrefix, i);
            }
        }

        private static string GetName(string namePrefix, int index)
        {
            return string.Format("{0}-{1}", namePrefix, (index + 1).ToString(CultureInfo.InvariantCulture));
        }

        public static DataSet<ProductCategory> SubCategories(this DataSet<ProductCategory> dataSet, int rowIndex)
        {
            return dataSet.Children(x => x.SubCategories, rowIndex);
        }
    }
}
