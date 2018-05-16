namespace DevZest.Data
{
    public abstract class Ref<T> : ColumnContainer
        where T : PrimaryKey
    {
        protected abstract T CreatePrimaryKey();

        private T _key;
        public T Key
        {
            get { return _key ?? (_key = CreatePrimaryKey()); }
        }
    }
}
