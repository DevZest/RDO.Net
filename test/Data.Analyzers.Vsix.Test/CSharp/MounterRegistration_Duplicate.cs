namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class MounterRegistration_Duplicate : Model
    {
        public static readonly Mounter<_Int32> _Column1 = RegisterColumn((MounterRegistration_Duplicate _) => _.Column1);

        static MounterRegistration_Duplicate()
        {
            RegisterColumn((MounterRegistration_Duplicate _) => _.Column1);
        }

        public _Int32 Column1 { get; private set; }
    }
}
