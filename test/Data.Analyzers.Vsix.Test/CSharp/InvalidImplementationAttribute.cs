using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class InvalidImplementationAttribute : Model
    {
        [_DbIndex]
        public _Boolean CK_AlwaysTrue
        {
            get { return _Boolean.Const(true); }
        }
    }
}
