namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public sealed class PrimaryKeyMismatchBaseConstructor : PrimaryKey
    {
        public PrimaryKeyMismatchBaseConstructor(_Int32 id)
            : base()
        {
        }
    }
}
