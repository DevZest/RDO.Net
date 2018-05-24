using System;

namespace DevZest.Data.Primitives
{
    public sealed class DbTableClause : DbFromClause
    {
        internal DbTableClause(Model model, string name)
        {
            Name = name;
            Model = model;
        }

        public string Name { get; private set; }

        public Model Model { get; private set; }

        public override void Accept(DbFromClauseVisitor visitor)
        {
            visitor.Visit(this);
        }

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
