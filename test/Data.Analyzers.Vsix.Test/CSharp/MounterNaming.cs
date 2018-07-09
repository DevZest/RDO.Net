namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class MounterNaming : Model
    {
        protected static readonly Mounter<_Int32> _Column1 = RegisterColumn((MounterNaming _) => _.Column2);
        protected static readonly Mounter<_Int32> _Column2;

        static MounterNaming()
        {
            _Column2 = RegisterColumn((MounterNaming _) => _.Column1);
        }

        public _Int32 Column1 { get; private set; }

        public _Int32 Column2 { get; private set; }
    }
}
