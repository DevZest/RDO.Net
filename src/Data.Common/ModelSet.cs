using DevZest.Data.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DevZest.Data
{
    internal class ModelSet : List<Model>, IModelSet
    {
        private class EmptyModelSet : IModelSet
        {
            public bool Contains(Model model)
            {
                return false;
            }

            public int Count
            {
                get { return 0; }
            }

            public Model this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public IEnumerator<Model> GetEnumerator()
            {
                yield break;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                yield break;
            }
        }

        public static readonly IModelSet Empty = new EmptyModelSet();

        public ModelSet()
        {
        }

        public ModelSet(IModelSet modelSet)
            : base(modelSet)
        {
        }

        private sealed class SourceModelResolver : DbFromClauseVisitor
        {
            public SourceModelResolver(ModelSet sourceModelSet)
            {
                _sourceModelSet = sourceModelSet;
            }

            private ModelSet _sourceModelSet;

            public override void Visit(DbUnionStatement union)
            {
                union.Query1.Accept(this);
                union.Query2.Accept(this);
            }

            public override void Visit(DbJoinClause join)
            {
                join.Left.Accept(this);
                join.Right.Accept(this);
            }

            public override void Visit(DbSelectStatement select)
            {
                _sourceModelSet.Add(select.Model);
            }

            public override void Visit(DbTableClause table)
            {
                _sourceModelSet.Add(table.Model);
            }
        }

        public void Add(DbFromClause dbFromClause)
        {
            dbFromClause.Accept(new SourceModelResolver(this));
        }
    }
}
