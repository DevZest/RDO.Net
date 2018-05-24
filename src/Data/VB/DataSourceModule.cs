using DevZest.Data;
using DevZest.Data.Primitives;
using Microsoft.VisualBasic.CompilerServices;

[StandardModule]
public sealed class DataSourceModule
{
    public static T ModelOf<T>(DataSet<T> dataSet) where T : Model, new()
    {
        return dataSet.GetModel();
    }

    public static T ModelOf<T>(DbSet<T> dbSet) where T : Model, new()
    {
        return dbSet.GetModel();
    }
}