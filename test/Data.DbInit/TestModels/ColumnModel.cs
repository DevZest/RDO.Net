namespace DevZest.Data.DbInit.TestModels
{
    public abstract class ColumnModel<T> : Model
        where T : Column, new()
    {
        static ColumnModel()
        {
            RegisterColumn((ColumnModel<T> _) => _.Column);
        }

        public T Column { get; private set; }
    }
}
