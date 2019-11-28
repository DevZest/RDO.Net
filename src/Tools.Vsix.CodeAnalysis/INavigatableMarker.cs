namespace DevZest.Data.CodeAnalysis
{
    public interface INavigatableMarker
    {
        bool Matches(INavigatable navigatable);
        bool NavigationSuggested { get; }
        bool ShouldExpand { get; }
    }
}
