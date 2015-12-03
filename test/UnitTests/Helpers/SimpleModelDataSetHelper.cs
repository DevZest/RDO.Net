using System;

namespace DevZest.Data.Helpers
{
    public abstract class SimpleModelDataSetHelper
    {
        protected class SimpleModel : SimpleModelBase
        {
            public static readonly Accessor<SimpleModel, SimpleModel> ChildAccessor = RegisterChildModel((SimpleModel x) => x.Child,
                x => x.ParentKey);

            public static ValidatorId ValidatorId = new ValidatorId(typeof(SimpleModel), "IdMustBeEven");

            public SimpleModel()
            {
                Validators.Add(Model.Validator.Create(ValidatorId, ValidationLevel.Error, Id % 2 == 0, "The Id must be even.", Id));
            }

            public SimpleModel Child { get; private set; }
        }

        protected DataSet<SimpleModel> GetDataSet(int count)
        {
            return SimpleModelBase.GetDataSet<SimpleModel>(count, x => x.Child, AddRows);
        }

        private void AddRows(DataSet<SimpleModel> dataSet, int count)
        {
            var model = dataSet._;
            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet.AddRow();
                model.Id[dataRow] = i;
            }
        }
    }
}
