using DevZest.Data.Primitives;
using System.Collections.ObjectModel;

namespace DevZest.Data.Addons
{
    public static class AddonBagExtensions
    {
        public static ReadOnlyCollection<T> GetAddons<T>(this AddonBag addonBag)
            where T : class, IAddon
        {
            return addonBag.GetAddons<T>();
        }

        public static T GetAddon<T>(this AddonBag addonBag)
            where T : class, IAddon
        {
            return addonBag.GetAddon<T>();
        }

        public static IAddon GetAddon(this AddonBag addonBag, object key)
        {
            return addonBag.GetAddon(key);
        }

        public static void Add(this AddonBag addonBag, IAddon addon)
        {
            addonBag.Add(addon);
        }

        public static void AddOrUpdate(this AddonBag addonBag, IAddon addon)
        {
            addonBag.AddOrUpdate(addon);
        }

        public static bool ContainsAddon(this AddonBag addonBag, object key)
        {
            return addonBag.ContainsAddon(key);
        }

        public static void RemoveAddon(this AddonBag addonBag, object key)
        {
            addonBag.RemoveAddon(key);
        }
    }
}
