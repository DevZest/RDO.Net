namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class DuplicateMounterRegistration : Model
    {
        protected static readonly Mounter<_Int32> _Column1 = RegisterColumn((DuplicateMounterRegistration _) => _.Column1);

        static DuplicateMounterRegistration()
        {
            RegisterColumn((DuplicateMounterRegistration _) => _.Column1);
        }

        public _Int32 Column1 { get; private set; }
    }
}
