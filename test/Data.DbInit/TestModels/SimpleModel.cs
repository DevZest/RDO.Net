namespace DevZest.Data.DbInit.TestModels
{
    public class SimpleModel : Model
    {
        static SimpleModel()
        {
            RegisterColumn((SimpleModel _) => _.Id);
        }

        public _Int32 Id { get; private set; }
    }
}
