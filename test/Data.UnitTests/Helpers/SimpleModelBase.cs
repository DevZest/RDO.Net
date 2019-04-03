using DevZest.Data.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Helpers
{
    public abstract class SimpleModelBase : Model<SimpleModelBase.PK>
    {
        public sealed class PK : CandidateKey
        {
            public PK(_Int32 id)
                : base(id)
            {
            }
        }

        protected SimpleModelBase()
        {
            ParentKey = new PK(ParentId);
        }

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(Id);
        }

        public static readonly Mounter<_Int32> _Id = RegisterColumn((SimpleModelBase x) => x.Id);

        public static readonly Mounter<_Int32> _ParentId = RegisterColumn((SimpleModelBase x) => x.ParentId);

        [Required]
        public _Int32 Id { get; private set; }

        public _Int32 ParentId { get; private set; }

        public PK ParentKey { get; private set; }

        public static DataSet<T> GetDataSet<T>(int count, Func<T, T> childGetter, Action<DataSet<T>, int> addRows, bool createChildren = true)
            where T : SimpleModelBase, new()
        {
            var dataSet = DataSet<T>.Create();
            var model = dataSet._;
            Assert.AreEqual(true, dataSet._.DesignMode);

            addRows(dataSet, count);
            if (createChildren)
            {
                for (int i = 0; i < dataSet.Count; i++)
                {
                    var children = dataSet[i].GetChildDataSet(childGetter(model));
                    addRows(children, count);
                    for (int j = 0; j < children.Count; j++)
                    {
                        var grandChildren = children[j].GetChildDataSet(childGetter(children._));
                        addRows(grandChildren, count);
                    }
                }
            }

            return dataSet;
        }
    }
}
