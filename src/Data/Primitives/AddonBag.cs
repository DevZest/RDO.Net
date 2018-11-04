using System.Collections.ObjectModel;
using System.Linq;

namespace DevZest.Data.Primitives
{
    public abstract partial class AddonBag
    {
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
            addon.VerifyNotNull(nameof(addon));

            _addons.Add(addon);
        }

        internal void AddOrUpdate(IAddon addon)
        {
            addon.VerifyNotNull(nameof(addon));

            var key = addon.Key;
            if (_addons.Contains(key))
                _addons.Remove(key);
            _addons.Add(addon);
        }

        internal bool ContainsAddon(object key)
        {
            key.VerifyNotNull(nameof(key));

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
