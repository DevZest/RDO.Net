using DevZest.Data;
using DevZest.Data.Primitives;
using Microsoft.VisualBasic.CompilerServices;

[StandardModule]
public sealed class DataSourceModule
{
    public static T ModelOf<T>(DataSet<T> dataSet) where T : class, IModelReference, new()
    {
        return dataSet.GetModel();
    }

    public static T ModelOf<T>(DbSet<T> dbSet) where T : class, IModelReference, new()
    {
        return dbSet.GetModel();
    }
}