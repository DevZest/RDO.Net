using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Wpf
{
    internal struct DataSet
    {
        internal static DataSet Get<T>(DataSet<T> dataSet)
            where T : Model, new()
        {
            return new DataSet(dataSet.Model, dataSet);
        }

        internal static DataSet Get(DataRowControl dataRowControl, Model model)
        {
            Debug.Assert(dataRowControl != null);

            var dataRow = dataRowControl.DataRow;
            Debug.Assert(dataRow != null);
            if (dataRow.Model == model)
                return dataRowControl.GetParent<DataSetControl>().DataSet;

            Debug.Assert(model.GetParentModel() == dataRow.Model.GetParentModel());
            return new DataSet(model, dataRow.GetChildren(model));
        }

        private DataSet(Model model, IList<DataRow> rows)
        {
            Debug.Assert(model != null);
            Debug.Assert(rows != null);

            Model = model;
            Rows = rows;
        }

        public readonly Model Model;
        public readonly IList<DataRow> Rows;
    }
}
