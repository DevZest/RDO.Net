using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies the database primary key.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [ModelDesignerSpec(addonTypes: null, validOnTypes: new Type[] { typeof(CandidateKey) })]
    public sealed class DbPrimaryKeyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DbPrimaryKeyAttribute"/>.
        /// </summary>
        /// <param name="name">The name of the database primary key.</param>
        public DbPrimaryKeyAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name of the database primary key.
        /// </summary>
        public string Name { get;  }

        /// <summary>
        /// Gets the description of the database primary key.
        /// </summary>
        public string Description { get; set; }
    }
}
