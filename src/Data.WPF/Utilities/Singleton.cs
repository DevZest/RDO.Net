using System.ComponentModel;

namespace DevZest
{
    static partial class Singleton
    {
        internal static readonly DataErrorsChangedEventArgs DataErrorsChangedEventArgs = new DataErrorsChangedEventArgs(string.Empty);
    }
}
