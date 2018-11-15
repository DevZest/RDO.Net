using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class ModelMemberAttributeRequiresArgument : Model
    {
        static ModelMemberAttributeRequiresArgument()
        {
            RegisterColumn((ModelMemberAttributeRequiresArgument _) => _.Id);
        }

        [DbColumn]
        public _Int32 Id { get; private set; }
    }
}
