using DevZest.Data.Annotations;
using System;
using System.Text;

namespace DevZest.Data.Helpers
{
    public abstract class SimpleModelDataSetHelper
    {
        [ModelValidator(nameof(ValidateId))]
        [Computation(nameof(ComputeInheritedValue), ComputationMode.Inherit)]
        protected class SimpleModel : SimpleModelBase
        {
            public static readonly Mounter<_Int32> _InheritedValue = RegisterColumn((SimpleModel _) => _.InheritedValue);

            public static readonly Mounter<_Int32> _ChildCount = RegisterColumn((SimpleModel _) => _.ChildCount);

            public static readonly Mounter<SimpleModel> _Child = RegisterChildModel((SimpleModel _) => _.Child, x => x.ParentKey);

            static SimpleModel()
            {
                RegisterLocalColumn((SimpleModel _) => _.LocalColumn);
            }

            private const string ERR_MESSAGE = "The Id must be even.";

            private DataValidationError ValidateId(DataRow dataRow)
            {
                return (Id[dataRow] % 2) == 0 ? null : new DataValidationError(ERR_MESSAGE, Id);
            }

            protected override void OnChildDataSetsCreated()
            {
                ChildCount.ComputedAs(Child.Id.CountRows(), false);
                base.OnChildDataSetsCreated();
            }

            public _Int32 InheritedValue { get; private set; }

            private void ComputeInheritedValue()
            {
                Child.InheritedValue.ComputedAs(InheritedValue);
            }

            public _Int32 ChildCount { get; private set; }

            public SimpleModel Child { get; private set; }

            public LocalColumn<int> LocalColumn { get; private set; }

            public StringBuilder StartLog(int depth)
            {
                var log = new StringBuilder();
                for (var _ = this; (depth--) >= 0; _ = _.Child)
                {
                    _.DataRowInserting += (sender, e) => LogDataRowInserting(log, e.DataRow);
                    _.BeforeDataRowInserted += (sender, e) => LogBeforeDataRowInserted(log, e.DataRow);
                    _.AfterDataRowInserted += (sender, e) => LogAfterDataRowInserted(log, e.DataRow);
                    _.DataRowRemoving += (sender, e) => LogDataRowRemoving(log, e.DataRow);
                    _.DataRowRemoved += (sender, e) => LogDataRowRemoved(log, e.BaseDataSet, e.Ordinal);
                    _.ValueChanged += (sender, e) => {
                        foreach (var column in e.Columns)
                            LogValueChanged(log, e.DataRow, column);
                    };
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

        protected DataSet<SimpleModel> GetDataSet(int count, Action<SimpleModel> modelInitializer, bool createChildren = false)
        {
            return SimpleModelBase.GetDataSet<SimpleModel>(count, x => x.Child, (dataSet, num) => AddRows(dataSet, num, modelInitializer), createChildren);
        }

        private void AddRows(DataSet<SimpleModel> dataSet, int count, Action<SimpleModel> modelInitializer)
        {
            modelInitializer(dataSet._);
            AddRows(dataSet, count);
        }

        private void AddRows(DataSet<SimpleModel> dataSet, int count)
        {
            for (int i = 0; i < count; i++)
                dataSet.AddRow(x => dataSet._.Id[x] = i);
        }
    }
}
