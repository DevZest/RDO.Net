namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public sealed class PrimaryKeyMismatchBaseConstructor : PrimaryKey
    {
        public PrimaryKeyMismatchBaseConstructor(_Int32 id)
            : base()
        {
        }

        public _Int32 ID
        {
            get { return GetColumn<_Int32>(0); }
        }
    }
}
