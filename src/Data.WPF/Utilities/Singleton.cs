using System.ComponentModel;

namespace DevZest.Data.Windows.Utilities
{
    static partial class Singleton
    {
        internal static readonly DataErrorsChangedEventArgs DataErrorsChangedEventArgs = new DataErrorsChangedEventArgs(string.Empty);
    }
}
