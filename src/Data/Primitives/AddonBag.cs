using DevZest.Data.Addons;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents a container of <see cref="IAddon"/> objects.
    /// </summary>
    /// <remarks>
    /// <para><see cref="AddonBag"/> is the base class of extensible rich metadata objects, such as
    /// <see cref="Model"/> and <see cref="Column"/>. <see cref="IAddon"/> object, on the other hand,
    /// is uniquely identified by <see cref="IAddon.Key"/>, can be added into this metadata object
    /// and later retrieved either by key or by type. For example: <see cref="DbIndex"/>, which implements
    /// <see cref="IAddon"/>, can be added into <see cref="Model"/> identified by its <see cref="DbIndex.Name"/>.</para>
    /// <para>Use extension methods provided in <see cref="AddonBagExtensions"/> class to manipulate <see cref="AddonBag"/> object.
    /// This is a design decision to separate addon operations into a different namespace.</para>
    /// </remarks>
    public abstract partial class AddonBag
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonBag"/> class.
        /// </summary>
        protected AddonBag()
        {
            _addons = new AddonCollection(this);
        }

        private AddonCollection _addons;

        internal ReadOnlyCollection<T> GetAddons<T>()
            where T : class, IAddon
        {
            return _addons.Filter<T>();
        }

        internal T GetAddon<T>()
            where T : class, IAddon
        {
            return GetAddons<T>().SingleOrDefault();
        }

        internal IAddon GetAddon(object key)
        {
            if (!ContainsAddon(key))
                return null;
            return _addons[key];
        }

        internal void Add(IAddon addon)
        {
            Debug.Assert(addon != null);
            _addons.Add(addon);
        }

        internal void AddOrUpdate(IAddon addon)
        {
            Debug.Assert(addon != null);

            var key = addon.Key;
            if (_addons.Contains(key))
                _addons.Remove(key);
            _addons.Add(addon);
        }

        internal bool ContainsAddon(object key)
        {
            Debug.Assert(key != null);
            return _addons.Contains(key);
        }

        internal void RemoveAddon(object key)
        {
            key.VerifyNotNull(nameof(key));

            _addons.Remove(key);
        }

        internal void BrutalRemoveAddon(object key)
        {
            _addons.AllowFrozenChange(true);
            RemoveAddon(key);
            _addons.AllowFrozenChange(false);
        }
    }
}
