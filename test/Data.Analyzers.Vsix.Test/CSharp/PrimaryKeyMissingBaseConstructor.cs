namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public sealed class PrimaryKeyMissingBaseConstructor : PrimaryKey
    {
        public PrimaryKeyMissingBaseConstructor(_Int32 id)
        {
        }

        public _Int32 ID
        {
            get { return GetColumn<_Int32>(0); }
        }
    }
}
