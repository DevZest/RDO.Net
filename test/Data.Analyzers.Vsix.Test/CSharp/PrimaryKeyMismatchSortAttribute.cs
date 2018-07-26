using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public sealed class PrimaryKeyMismatchSortAttribute : PrimaryKey
    {
        public PrimaryKeyMismatchSortAttribute([Asc]_Int32 id1, _Int32 id2)
            : base(id1, id2.Desc())
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
