namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class MounterRegistration_InvalidInvocation : Model
    {
        static MounterRegistration_InvalidInvocation()
        {
            var _column1 = RegisterColumn((MounterRegistration_InvalidInvocation x) => x.Column1);
        }

        private static void AnotherMethod()
        {
            RegisterColumn((MounterRegistration_InvalidInvocation x) => x.Column2);
        }

        public _Int32 Column1 { get; private set; }

        public _Int32 Column2 { get; private set; }
    }
}
