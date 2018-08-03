namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class ProjectionColumnNaming : Model
    {
        protected static readonly Mounter<_Int32> _Column1 = RegisterColumn((ProjectionColumnNaming _) => _.Column1);

        public _Int32 Column1 { get; private set; }

        public class Lookup : Projection
        {
            static Lookup()
            {
                Register((Lookup _) => _.Column2, _Column1);
            }

            public _Int32 Column2 { get; private set; }
        }
    }
}
