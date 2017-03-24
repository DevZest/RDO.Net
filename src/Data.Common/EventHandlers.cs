namespace DevZest.Data
{
    public delegate void ChildModelsInitialized(Model model);

    public delegate void DataRowAddEventHandler(DataRow dataRow);

    public delegate void DataRowRemovedEventHandler(DataRow dataRow, DataSet baseDataSet, int ordinal, DataSet dataSet, int index);

    public delegate void DataRowUpdatedEventHandler(DataRow dataRow, IColumnSet columns);
}
