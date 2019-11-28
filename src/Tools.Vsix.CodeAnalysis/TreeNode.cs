using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    public abstract class TreeNode : INavigatable
    {
        public abstract string Name { get; }

        public abstract Location Location { get; }

        public abstract Location LocalLocation { get; }

        public bool HasLocalLocation
        {
            get { return LocalLocation != null; }
        }

        public virtual bool IsEnabled
        {
            get { return HasLocalLocation; }
        }

        public virtual bool IsFolder
        {
            get { return false; }
        }

        public abstract INavigatableMarker CreateMarker(bool navigationSuggested = true);
    }
}
