using DevZest.Data;
using Microsoft.VisualBasic.CompilerServices;

/// <summary>
/// Provides EntityOf() operator for VB.Net because _ is not a valid identifier in VB.
/// </summary>
[StandardModule]
public sealed class EntityOfModule
{
    /// <exclude />
    public static T EntityOf<T>(DataSet<T> dataSet) where T : class, IEntity, new()
    {
        return dataSet._;
    }

    /// <exclude />
    public static T EntityOf<T>(DbSet<T> dbSet) where T : class, IEntity, new()
    {
        return dbSet._;
    }
}