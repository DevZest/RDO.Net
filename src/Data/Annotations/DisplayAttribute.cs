using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [ModelMemberAttributeSpec(null, false, typeof(Column))]
    public class DisplayAttribute : ColumnAttribute
    {
        private Type _resourceType;
        private LocalizableString _shortName = new LocalizableString(nameof(ShortName));
        private LocalizableString _name = new LocalizableString(nameof(Name));
        private LocalizableString _description = new LocalizableString(nameof(Description));
        private LocalizableString _prompt = new LocalizableString(nameof(Prompt));

        /// <summary>Gets or sets a value that is used for the grid column label.</summary>
        /// <returns>A value that is for the grid column label.</returns>
        public string ShortName
        {
            get { return _shortName.Value; }
            set
            {
                if (_shortName.Value != value)
                    _shortName.Value = value;
            }
        }

        /// <summary>Gets or sets a value that is used for display in the UI.</summary>
        /// <returns>A value that is used for display in the UI.</returns>
        public string Name
        {
            get { return _name.Value; }
            set
            {
                if (_name.Value != value)
                    _name.Value = value;
            }
        }

        /// <summary>Gets or sets a value that is used to display a description in the UI.</summary>
        /// <returns>The value that is used to display a description in the UI.</returns>
        public string Description
        {
            get { return _description.Value; }
            set
            {
                if (_description.Value != value)
                    _description.Value = value;
            }
        }

        /// <summary>Gets or sets a value that will be used to set the watermark for prompts in the UI.</summary>
        /// <returns>A value that will be used to display a watermark in the UI.</returns>
        public string Prompt
        {
            get { return _prompt.Value; }
            set
            {
                if (_prompt.Value != value)
                    _prompt.Value = value;
            }
        }

        /// <summary>Gets or sets the type that contains the resources for the
        /// <see cref="P:DevZest.Data.Annotations.DisplayAttribute.ShortName" />,
        /// <see cref="P:DevZest.Data.Annotations.DisplayAttribute.Name" />,
        /// <see cref="P:DevZest.Data.Annotations.DisplayAttribute.Prompt" />, and
        /// <see cref="P:DevZest.Data.Annotations.DisplayAttribute.Description" /> properties.
        /// </summary>
        /// <returns>The type of the resource that contains the
        /// <see cref="P:DevZest.Data.Annotations.DisplayAttribute.ShortName" />,
        /// <see cref="P:DevZest.Data.Annotations.DisplayAttribute.Name" />,
        /// <see cref="P:DevZest.Data.Annotations.DisplayAttribute.Prompt" />, and
        /// <see cref="P:DevZest.Data.Annotations.DisplayAttribute.Description" /> properties.</returns>
        public Type ResourceType
        {
            get { return this._resourceType; }
            set
            {
                if (_resourceType != value)
                {
                    _resourceType = value;
                    _shortName.ResourceType = value;
                    _name.ResourceType = value;
                    _description.ResourceType = value;
                    _prompt.ResourceType = value;
                }
            }
        }

        /// <summary>Initializes a new instance of the <see cref="DisplayAttribute" /> class.</summary>
        public DisplayAttribute()
        {
        }

        protected override void Wireup(Column column)
        {
            var shortNameGetter = _shortName.LocalizableValueGetter;
            if (shortNameGetter != null)
                column.SetDisplayShortName(shortNameGetter);

            var nameGetter = _name.LocalizableValueGetter;
            if (nameGetter != null)
                column.SetDisplayName(nameGetter);

            var descriptionGetter = _description.LocalizableValueGetter;
            if (descriptionGetter != null)
                column.SetDisplayDescription(descriptionGetter);

            var promptGetter = _prompt.LocalizableValueGetter;
            if (promptGetter != null)
                column.SetDisplayPrompt(promptGetter);
        }
    }
}
