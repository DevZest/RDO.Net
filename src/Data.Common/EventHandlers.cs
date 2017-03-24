namespace DevZest.Data
{
    public delegate void ModelEventHandler(Model model);

    public delegate void DataRowEventHandler(DataRow dataRow);

    public delegate void DataRowRemovedEventHandler(DataRow dataRow, DataSet baseDataSet, int ordinal, DataSet dataSet, int index);

    public delegate void DataRowUpdatedEventHandler(DataRow dataRow, IColumnSet columns);
}
