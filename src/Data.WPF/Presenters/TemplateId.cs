using DevZest.Data.Presenters.Primitives;
using System;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents Id used to load control template resource.
    /// </summary>
    public sealed class TemplateId : ResourceId<ControlTemplate>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TemplateId"/> class.
        /// </summary>
        /// <param name="type">The type that will be used to resolve the control template URI.</param>
        public TemplateId(Type type)
            : base(type)
        {
        }

        /// <inheritdoc/>
        protected override string UriSuffix
        {
            get { return "Templates"; }
        }
    }
}
