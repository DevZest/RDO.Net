using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class MounterRegistration_Missing : Model
    {
        public _String Column { get; private set; }

        public int NotAColumn { get; private set; }
    }
}
