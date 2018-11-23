namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class PrimaryKeyNotSealed : CandidateKey
    {
        public PrimaryKeyNotSealed(_Int32 id)
            : base(id)
        {
        }
    }
}
