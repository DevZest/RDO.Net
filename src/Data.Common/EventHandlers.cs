namespace DevZest.Data
{
    public delegate void ModelEventHandler(Model model);

    public delegate void DataRowInsertEventHandler(DataRow dataRow);

    public delegate void DataRowRemoveEventHandler(DataRow dataRow, DataSet baseDataSet, int ordinal, DataSet dataSet, int index);

    public delegate void DataRowUpdateEventHandler(DataRow dataRow, IColumnSet columns);
}
