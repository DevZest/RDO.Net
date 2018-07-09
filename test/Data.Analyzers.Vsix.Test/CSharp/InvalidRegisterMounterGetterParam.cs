namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class InvalidRegisterMounterGetterParam : Model
    {
        private class AnotherModel : Model
        {
            public _Int32 Column1 { get; private set; }
        }

        static InvalidRegisterMounterGetterParam()
        {
            RegisterColumn((InvalidRegisterMounterGetterParam _) => _.Column1);
            RegisterColumn((AnotherModel _) => _.Column1);
        }

        public _Int32 Column1 { get; }
    }
}
