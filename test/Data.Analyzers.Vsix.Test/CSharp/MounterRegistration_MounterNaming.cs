namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class MounterRegistration_MounterNaming : Model
    {
        public static readonly Mounter<_Int32> _Column1 = RegisterColumn((MounterRegistration_MounterNaming _) => _.Column2);
        public static readonly Mounter<_Int32> _Column2;

        static MounterRegistration_MounterNaming()
        {
            _Column2 = RegisterColumn((MounterRegistration_MounterNaming _) => _.Column1);
        }

        public _Int32 Column1 { get; private set; }

        public _Int32 Column2 { get; private set; }
    }
}
