using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies the database index.
    /// </summary>
    [CrossReference(typeof(_DbIndexAttribute))]
    [ModelDeclarationSpec(true, typeof(ColumnSort[]))]
    public sealed class DbIndexAttribute : DbIndexBaseAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DbIndexAttribute"/>.
        /// </summary>
        /// <param name="name">The name of the database index.</param>
        public DbIndexAttribute(string name)
            : base(name)
        {
        }

        /// <inheritdoc />
        protected override void Wireup(Model model, string dbName, ColumnSort[] sortOrder)
        {
            model.Index(dbName, Description, IsUnique, IsCluster, IsValidOnTable, IsValidOnTempTable, sortOrder);
        }

        /// <summary>
        /// Gets or sets a value inidicates whether this is a unique database index.
        /// </summary>
        public bool IsUnique { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicates whether this index applies to permanent table. The default value is <see langword="true"/>.
        /// </summary>
        public bool IsValidOnTable { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicates whether this index applies to temporary table. The default value is <see langword="false"/>.
        /// </summary>
        public bool IsValidOnTempTable { get; set; } = false;
    }
}
