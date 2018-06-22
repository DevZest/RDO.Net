namespace DevZest.Data
{
    public abstract class Ref<T> : ColumnGroup
        where T : PrimaryKey
    {
        protected abstract T GetForeignKey();

        private T _foreignKey;
        public T ForeignKey
        {
            get { return _foreignKey ?? (_foreignKey = GetForeignKey()); }
        }
    }

}
