using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    [CheckConstraint("CK_AlwaysTrue", "CK")]
    public class MissingImplementation : Model
    {
    }
}
