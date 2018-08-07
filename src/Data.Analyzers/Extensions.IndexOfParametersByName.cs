using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static int IndexOf(this ImmutableArray<IParameterSymbol> parameters, string name)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].Name == name)
                    return i;
            }

            return -1;
        }
    }
}
