using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public sealed class PrimaryKeySortAttributeConflict : CandidateKey
    {
        public PrimaryKeySortAttributeConflict([Asc] [Desc]_Int32 id)
            : base(id)
        {
        }
    }
}
