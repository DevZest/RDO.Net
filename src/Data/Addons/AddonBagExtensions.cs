using System;
using DevZest.Data.Primitives;
using System.Collections.ObjectModel;

namespace DevZest.Data.Addons
{
    /// <summary>
    /// Extension methods for <see cref="AddonBag"/>.
    /// </summary>
    public static class AddonBagExtensions
    {
        /// <summary>
        /// Gets the collection of addons in the <see cref="AddonBag" /> by type.
        /// </summary>
        /// <typeparam name="T">The type of the addon.</typeparam>
        /// <param name="addonBag">The <see cref="AddonBag"/>.</param>
        /// <returns>The collection of addons with specified type.</returns>
        public static ReadOnlyCollection<T> GetAddons<T>(this AddonBag addonBag)
            where T : class, IAddon
        {
            return addonBag.GetAddons<T>();
        }

        /// <summary>
        /// Get the single or default addon in the <see cref="AddonBag"/> by type.
        /// </summary>
        /// <typeparam name="T">The type of the addon.</typeparam>
        /// <param name="addonBag">The <see cref="AddonBag"/>.</param>
        /// <returns>The addon with specified type, <see langword="null"/> if no addon found.</returns>
        /// <exception cref="InvalidOperationException">More than one addon of the specified type found.</exception>
        public static T GetAddon<T>(this AddonBag addonBag)
            where T : class, IAddon
        {
            return addonBag.GetAddon<T>();
        }

        /// <summary>
        /// Gets the addon by key.
        /// </summary>
        /// <param name="addonBag">The <see cref="AddonBag"/>.</param>
        /// <param name="key">The key which uniquely identifies the addon.</param>
        /// <returns>The addon with specified key. <see langword="null" /> if no addon with specified key found.</returns>
        public static IAddon GetAddon(this AddonBag addonBag, object key)
        {
            return addonBag.GetAddon(key);
        }

        /// <summary>
        /// Adds addon into <see cref="AddonBag"/>.
        /// </summary>
        /// <param name="addonBag">The <see cref="AddonBag"/>.</param>
        /// <param name="addon">The addon to be added.</param>
        /// <exception cref="NullReferenceException"><paramref name="addon"/> is <see langword="null"/></exception>
        /// <exception cref="InvalidOperationException">Duplicate <see cref="IAddon.Key"/> exists.</exception>
        public static void Add(this AddonBag addonBag, IAddon addon)
        {
            addon.VerifyNotNull(nameof(addon));
            addonBag.Add(addon);
        }

        /// <summary>
        /// Adds or update addon in <see cref="AddonBag"/>.
        /// </summary>
        /// <param name="addonBag">The <see cref="AddonBag"/>.</param>
        /// <param name="addon">The addon to be added or updated with by key.</param>
        /// <exception cref="NullReferenceException"><paramref name="addon"/> is <see langword="null"/></exception>
        public static void AddOrUpdate(this AddonBag addonBag, IAddon addon)
        {
            addon.VerifyNotNull(nameof(addon));
            addonBag.AddOrUpdate(addon);
        }

        /// <summary>
        /// Determines if <see cref="AddonBag"/> contains specified addon key.
        /// </summary>
        /// <param name="addonBag">The <see cref="AddonBag"/>.</param>
        /// <param name="key">The key of addon object.</param>
        /// <returns><see langword="true" /> if <see cref="AddonBag"/> contains specified addon key, otherwise <see langword="false"/>.</returns>
        /// <exception cref="NullReferenceException"><paramref name="key"/> is <see langword="null"/>.</exception>
        public static bool ContainsAddon(this AddonBag addonBag, object key)
        {
            key.VerifyNotNull(nameof(key));
            return addonBag.ContainsAddon(key);
        }

        /// <summary>
        /// Removes addon from <see cref="AddonBag"/> by key.
        /// </summary>
        /// <param name="addonBag">The <see cref="AddonBag"/>.</param>
        /// <param name="key">The addon key.</param>
        /// <exception cref="NullReferenceException"><paramref name="key"/> is <see langword="null"/>.</exception>
        public static void RemoveAddon(this AddonBag addonBag, object key)
        {
            addonBag.RemoveAddon(key);
        }
    }
}
