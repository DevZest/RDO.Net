namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public sealed class PrimaryKeyMismatchBaseConstructorArgument : PrimaryKey
    {
        public PrimaryKeyMismatchBaseConstructorArgument(_Int32 id1, _Int32 id2)
            : base(id2, id1)
        {
        }

        public _Int32 ID1
        {
            get { return GetColumn<_Int32>(0); }
        }

        public _Int32 ID2
        {
            get { return GetColumn<_Int32>(1); }
        }
    }
}
