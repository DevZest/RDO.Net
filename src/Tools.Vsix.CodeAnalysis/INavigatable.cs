using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    public interface INavigatable
    {
        Location Location { get; }
        Location LocalLocation { get; }
        INavigatableMarker CreateMarker(bool navigationSuggested = true);
    }
}
