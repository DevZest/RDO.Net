using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public sealed class PrimaryKeyMismatchSortAttribute : CandidateKey
    {
        public PrimaryKeyMismatchSortAttribute([Asc]_Int32 id1, _Int32 id2)
            : base(id1, id2.Desc())
        {
        }
    }
}
