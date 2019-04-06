using DevZest.Data;
using DevZest.Data.Primitives;
using Microsoft.VisualBasic.CompilerServices;

[StandardModule]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
// Provides ModelOf() operator for VB.Net because _ is not a valid identifier in VB.
public sealed class DataSourceModule
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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