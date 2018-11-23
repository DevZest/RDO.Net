namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public sealed class PrimaryKeyMissingBaseConstructor : CandidateKey
    {
        public PrimaryKeyMissingBaseConstructor(_Int32 id)
        {
        }
    }
}
