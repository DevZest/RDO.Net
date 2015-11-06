using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Helpers
{
    public abstract class SimpleModelBase : Model<SimpleModelKey>
    {
        protected SimpleModelBase()
        {
            _primaryKey = new SimpleModelKey(Id);
            ParentKey = new SimpleModelKey(ParentId);
        }

        public static readonly Accessor<SimpleModelBase, _Int32> IdAccessor = RegisterColumn((SimpleModelBase x) => x.Id);

        public static readonly Accessor<SimpleModelBase, _Int32> ParentIdAccessor = RegisterColumn((SimpleModelBase x) => x.ParentId);

        [Required]
        public _Int32 Id { get; private set; }

        private SimpleModelKey _primaryKey;
        public sealed override SimpleModelKey PrimaryKey
        {
            get { return _primaryKey; }
        }

        public _Int32 ParentId { get; private set; }

        public SimpleModelKey ParentKey { get; private set; }

        public static DataSet<T> GetDataSet<T>(int count, Func<T, T> childGetter, Action<DataSet<T>, int> addRows)
            where T : SimpleModelBase, new()
        {
            var dataSet = DataSet<T>.New();
            var model = dataSet._;
            Assert.AreEqual(true, dataSet._.DesignMode);

            addRows(dataSet, count);
            for (int i = 0; i < dataSet.Count; i++)
            {
                var children = dataSet[i].Children(childGetter(model));
                addRows(children, count);
                for (int j = 0; j < children.Count; j++)
                {
                    var grandChildren = children[j].Children(childGetter(children._));
                    addRows(grandChildren, count);
                }
            }

            return dataSet;
        }
    }
}
