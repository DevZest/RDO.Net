namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class MounterRegistration_InvalidLocalColumn : Model
    {
        static MounterRegistration_InvalidLocalColumn()
        {
            RegisterColumn((MounterRegistration_InvalidLocalColumn _) => _.Column1);
        }

        public LocalColumn<string> Column1 { get; private set; }
    }
}
