using System;

namespace DevZest.Data.DbInit
{
    /// <summary>
    /// Indicates <see cref="DbSessionProvider{T}"/> implementation will return an empty database.
    /// </summary>
    /// <remarks>Database generation requires an empty database. DbInit tools will seek this attribute to determine if 
    /// the <see cref="DbSessionProvider{T}"/> will provide an empty database to avoid table already exists error.</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class EmptyDbAttribute : Attribute
    {
    }
}
