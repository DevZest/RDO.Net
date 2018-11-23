namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public sealed class PrimaryKeyMismatchBaseConstructorArgument : CandidateKey
    {
        public PrimaryKeyMismatchBaseConstructorArgument(_Int32 id1, _Int32 id2)
            : base(id2, id1)
        {
        }
    }
}
