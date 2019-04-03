namespace DevZest.Data.AspNetCore
{
    public class DataSetContainer
    {
        public DataSetContainer(DataSet dataSet)
        {
            DataSet = dataSet;
        }

        public virtual DataSet DataSet { get; }
    }

    public class ScalarDataSetContainer : DataSetContainer
    {
        public ScalarDataSetContainer(DataSet dataSet)
            : base(dataSet)
        {
        }

        [Scalar]
        public override DataSet DataSet
        {
            get { return base.DataSet; }
        }
    }
}
