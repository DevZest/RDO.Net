using DevZest.Data.Primitives;

namespace DevZest.Data.Addons
{
    /// <summary>
    /// Represents addon objects which can be contained by <see cref="AddonBag"/>.
    /// </summary>
    public interface IAddon
    {
        /// <summary>
        /// Gets the key which uniquely identifies the addon in the <see cref="AddonBag"/>.
        /// </summary>
        /// <remarks>
        /// Implementors should return immutable, non <see langword="null"/> value.
        /// Add addon objects with the same key into <see cref="AddonBag"/> will throw an exception.
        /// </remarks>
        object Key { get; }
    }
}
