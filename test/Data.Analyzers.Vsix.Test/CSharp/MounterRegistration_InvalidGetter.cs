namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class MounterRegistration_InvalidGetter : Model
    {
        private class AnotherModel : Model
        {
            public _Int32 Column1 { get; private set; }
        }

        static MounterRegistration_InvalidGetter()
        {
            RegisterColumn((MounterRegistration_InvalidGetter _) => _.Column1);
            RegisterColumn((AnotherModel _) => _.Column1);
        }

        public _Int32 Column1 { get; }
    }
}
