using System;
using System.Linq;
using System.Text;

namespace DevZest.Data.Helpers
{
    public abstract class SimpleModelDataSetHelper
    {
        protected class SimpleModel : SimpleModelBase
        {
            public static readonly Accessor<SimpleModel, _Int32> InheritedValueAccessor = RegisterColumn((SimpleModel x) => x.InheritedValue);

            public static readonly Accessor<SimpleModel, _Int32> ChildCountAccessor = RegisterColumn((SimpleModel x) => x.ChildCount);

            public static readonly Accessor<SimpleModel, SimpleModel> ChildAccessor = RegisterChildModel((SimpleModel x) => x.Child,
                x => x.ParentKey, (ColumnMappingsBuilder builder, SimpleModel child, SimpleModel parent) => builder.Select(child.InheritedValue, parent.InheritedValue));

            public const string MESSAGE_ID = "IdMustBeEven";

            public SimpleModel()
            {
                Validators.Add(Validator.Create(MESSAGE_ID, ValidationSeverity.Error, Id, Id % 2 == 0, "The Id must be even."));
            }

            protected override void OnBeforeChildModelsInitialized()
            {
                ChildCount.ComputedAs(Child.Id.CountRows(), false);
            }

            public _Int32 InheritedValue { get; private set; }

            public _Int32 ChildCount { get; private set; }

            public SimpleModel Child { get; private set; }

            public StringBuilder StartLog(int depth)
            {
                var log = new StringBuilder();
                for (var _ = this; (depth--) >= 0; _ = _.Child)
                {
                    _.DataRowInserting += dataRow => LogDataRowInserting(log, dataRow);
                    _.BeforeDataRowInserted += dataRow => LogBeforeDataRowInserted(log, dataRow);
                    _.AfterDataRowInserted += dataRow => LogAfterDataRowInserted(log, dataRow);
                    _.DataRowRemoving += (dataRow) => LogDataRowRemoving(log, dataRow);
                    _.DataRowRemoved += (dataRow, baseDataSet, ordinal, parentDataSet, index) => LogDataRowRemoved(log, baseDataSet, ordinal);
                    _.ValueChanged += (dataRow, columns) => LogValueChanged(log, dataRow, columns);
                }
                return log;
            }

            private static void LogDataRowInserting(StringBuilder log, DataRow dataRow)
            {
                log.AppendLine(string.Format("DataRowInserting: DataSet-{0}[{1}].", dataRow.Model.Depth, dataRow.Ordinal));
            }

            private static void LogBeforeDataRowInserted(StringBuilder log, DataRow dataRow)
            {
                log.AppendLine(string.Format("BeforeDataRowInserted: DataSet-{0}[{1}].", dataRow.Model.Depth, dataRow.Ordinal));
            }

            private static void LogAfterDataRowInserted(StringBuilder log, DataRow dataRow)
            {
                log.AppendLine(string.Format("AfterDataRowInserted: DataSet-{0}[{1}].", dataRow.Model.Depth, dataRow.Ordinal));
            }

            private static void LogDataRowRemoving(StringBuilder log, DataRow dataRow)
            {
                log.AppendLine(string.Format("DataRowRemoving: DataSet-{0}[{1}].", dataRow.Model.Depth, dataRow.Ordinal));
            }

            private static void LogDataRowRemoved(StringBuilder log, DataSet baseDataSet, int ordinal)
            {
                log.AppendLine(string.Format("DataRowRemoved: DataSet-{0}[{1}].", baseDataSet.Model.Depth, ordinal));
            }

            private static void LogValueChanged(StringBuilder log, DataRow dataRow, Column column)
            {
                log.AppendLine(string.Format("ValueChanged: DataSet-{0}[{1}], [{2}]", dataRow.Model.Depth, dataRow.Ordinal, column.Name));
            }
        }

        protected DataSet<SimpleModel> GetDataSet(int count, bool createChildren = true)
        {
            return SimpleModelBase.GetDataSet<SimpleModel>(count, x => x.Child, AddRows, createChildren);
        }

        private void AddRows(DataSet<SimpleModel> dataSet, int count)
        {
            for (int i = 0; i < count; i++)
                dataSet.AddRow(x => dataSet._.Id[x] = i);
        }
    }
}
