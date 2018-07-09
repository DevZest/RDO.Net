namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class InvalidRegisterLocalColumn : Model
    {
        static InvalidRegisterLocalColumn()
        {
            RegisterColumn((InvalidRegisterLocalColumn _) => _.Column1);
        }

        public LocalColumn<string> Column1 { get; private set; }
    }
}
