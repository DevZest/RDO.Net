namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public sealed class PrimaryKeyMismatchBaseConstructor : CandidateKey
    {
        public PrimaryKeyMismatchBaseConstructor(_Int32 id)
            : base()
        {
        }
    }
}
