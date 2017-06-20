using System;

namespace DevZest.Data.Presenters
{
    public struct InputError
    {
        public static InputError Empty = new InputError();

        public InputError(string id, string description)
        {
            if (string.IsNullOrEmpty(description))
                throw new ArgumentNullException(nameof(description));

            Id = id;
            Description = description;
        }

        public readonly string Id;

        public readonly string Description;

        public override string ToString()
        {
            return Description;
        }

        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(Description); }
        }
    }
}
