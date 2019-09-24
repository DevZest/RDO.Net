using DevZest.Data.Presenters.Primitives;
using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents Id used to load style resource.
    /// </summary>
    public sealed class StyleId : ResourceId<Style>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="StyleId"/> class.
        /// </summary>
        /// <param name="type">The type that will be used to resolve the style URI.</param>
        public StyleId(Type type)
            : base(type)
        {
        }

        /// <inheritdoc/>
        protected override string UriSuffix
        {
            get { return "Styles"; }
        }
    }
}
