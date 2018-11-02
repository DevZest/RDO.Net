using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    [CheckConstraint(nameof(CK_AlwaysTrue), "CK")]
    public class MissingImplementationAttribute : Model
    {
        private _Boolean CK_AlwaysTrue
        {
            get { return _Boolean.Const(true); }
        }
    }
}
