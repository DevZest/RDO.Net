using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class InvalidModelMemberAttribute : Model
    {
        static InvalidModelMemberAttribute()
        {
            RegisterColumn((InvalidModelMemberAttribute _) => _.Id);
            RegisterColumn((InvalidModelMemberAttribute _) => _.Name);
        }

        [CreditCard]
        public _Int32 Id { get; private set; }

        [Identity(1, 1)]
        public _String Name { get; private set; }
    }
}
