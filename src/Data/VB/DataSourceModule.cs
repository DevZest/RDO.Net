using DevZest.Data;
using DevZest.Data.Primitives;
using Microsoft.VisualBasic.CompilerServices;

/// <summary>
/// Provides ModelOf() operator for VB.Net because _ is not a valid identifier in VB.
/// </summary>
[StandardModule]
public sealed class DataSourceModule
{
    /// <exclude />
    public static T ModelOf<T>(DataSet<T> dataSet) where T : class, IModelReference, new()
    {
        return dataSet.GetModel();
    }

    /// <exclude />
    public static T ModelOf<T>(DbSet<T> dbSet) where T : class, IModelReference, new()
    {
        return dbSet.GetModel();
    }
}