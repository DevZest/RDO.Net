using System;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents a SQL FROM clause.
    /// </summary>
    public abstract class DbFromClause
    {
        /// <summary>
        /// Dispatches to the specific visit method for this node type.
        /// </summary>
        /// <param name="visitor">The vistor.</param>
        public abstract void Accept(DbFromClauseVisitor visitor);

        /// <summary>
        /// Dispatches to the specific visit method for this node type.
        /// </summary>
        /// <param name="visitor">The vistor.</param>
        public abstract T Accept<T>(DbFromClauseVisitor<T> visitor);

        internal virtual SubQueryEliminator SubQueryEliminator
        {
            get { return null; }
        }

        internal DbFromClause Clone(Model model)
        {
            var result = (DbFromClause)MemberwiseClone();
            result.OnClone(model);
            return result;
        }

        internal abstract void OnClone(Model model);
    }
}
