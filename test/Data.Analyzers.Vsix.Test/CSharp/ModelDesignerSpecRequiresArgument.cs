using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class ModelDesignerSpecRequiresArgument : Model
    {
        static ModelDesignerSpecRequiresArgument()
        {
            RegisterColumn((ModelDesignerSpecRequiresArgument _) => _.Id);
        }

        [DbColumn]
        public _Int32 Id { get; private set; }
    }
}
