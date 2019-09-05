namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents database table as SQL FROM clause.
    /// </summary>
    public sealed class DbTableClause : DbFromClause
    {
        internal DbTableClause(Model model, string name)
        {
            Name = name;
            Model = model;
        }

        /// <summary>
        /// Gets the name of database table.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the model of database table.
        /// </summary>
        public Model Model { get; private set; }

        /// <inheritdoc/>
        public override void Accept(DbFromClauseVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <inheritdoc/>
        public override T Accept<T>(DbFromClauseVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        internal override void OnClone(Model model)
        {
            Model = model;
        }
    }
}
