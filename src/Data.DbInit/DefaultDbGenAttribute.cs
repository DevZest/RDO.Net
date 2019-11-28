using DevZest.Data.Primitives;
using System;
using System.IO;
using System.Reflection;

namespace DevZest.Data.DbInit
{
    /// <summary>
    /// Defines default database generation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class DefaultDbGenAttribute : Attribute
    {
        /// <summary>
        /// Creates a new instance of <see cref="DefaultDbGenAttribute"/> class.
        /// </summary>
        /// <param name="dbSessionProviderType">The type derives from <see cref="DbSessionProvider{T}"/> class.</param>
        public DefaultDbGenAttribute(Type dbSessionProviderType)
        {
            DbSessionProviderType = dbSessionProviderType ?? throw new ArgumentNullException(nameof(dbSessionProviderType));
        }

        /// <summary>
        /// Gets the type derives from <see cref="DbSessionProvider{T}"/> class.
        /// </summary>
        public Type DbSessionProviderType { get; }

        /// <summary>
        /// Gets or sets a type derives from <see cref="DbInitializer{T}"/> class.
        /// </summary>
        /// <remarks>If not set, <see cref="DbGenerator{T}"/> type wil be used.</remarks>
        public Type DbInitializerType { get; set; }

        /// <summary>
        /// Gets or sets the project path.
        /// </summary>
        public string ProjectPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicates whether the program should show database command execution log.
        /// </summary>
        public bool ShowsDbLog { get; set; }

        private string GetProjectPath()
        {
            var result = Assembly.GetEntryAssembly().Location;
            if (!string.IsNullOrEmpty(ProjectPath))
                result = Path.Combine(result, ProjectPath);
            return result;
        }

        internal int Run()
        {
            return DbGenRunner.Execute(DbSessionProviderType, DbSessionProviderType.SafeGetDbSessionType(), DbInitializerType, GetProjectPath(), ShowsDbLog);
        }
    }
}
