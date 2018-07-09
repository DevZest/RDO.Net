namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class InvalidRegisterMounterInvocation : Model
    {
        static InvalidRegisterMounterInvocation()
        {
            var _column1 = RegisterColumn((InvalidRegisterMounterInvocation x) => x.Column1);
        }

        private static void AnotherMethod()
        {
            RegisterColumn((InvalidRegisterMounterInvocation x) => x.Column2);
        }

        public _Int32 Column1 { get; private set; }

        public _Int32 Column2 { get; private set; }
    }
}
