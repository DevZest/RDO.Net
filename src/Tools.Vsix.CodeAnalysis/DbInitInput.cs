namespace DevZest.Data.CodeAnalysis
{
    public class DbInitInput : Model
    {
        static DbInitInput()
        {
            RegisterLocalColumn((DbInitInput _) => _.Title);
            RegisterLocalColumn((DbInitInput _) => _.IsPassword);
            RegisterLocalColumn((DbInitInput _) => _.Value);
            RegisterLocalColumn((DbInitInput _) => _.EnvironmentVariableName);
            RegisterLocalColumn((DbInitInput _) => _.Order);
        }

        public LocalColumn<string> Title { get; private set; }

        public LocalColumn<bool> IsPassword { get; private set; }

        public LocalColumn<string> Value { get; private set; }

        public LocalColumn<string> EnvironmentVariableName { get; private set; }

        public LocalColumn<int> Order { get; private set; }
    }
}
