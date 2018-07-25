namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class PrimaryKeyNotSealed : PrimaryKey
    {
        public PrimaryKeyNotSealed(_Int32 id)
            : base(id)
        {
        }

        public _Int32 ID
        {
            get { return GetColumn<_Int32>(0); }
        }
    }
}
