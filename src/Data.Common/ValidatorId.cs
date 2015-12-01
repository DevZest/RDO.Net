using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public struct ValidatorId
    {
        internal static ValidatorId Deserialize(string value)
        {
            return new ValidatorId(value);
        }

        public ValidatorId(Type ownerType, string name)
        {
            Check.NotNull(ownerType, nameof(ownerType));
            Check.NotEmpty(name, nameof(name));

            _fullName = ownerType.Namespace + "." + ownerType.Name + "." + name;
        }

        private ValidatorId(string fullName)
        {
            _fullName = fullName;
        }

        private readonly string _fullName;

        public override string ToString()
        {
            return _fullName;
        }

        public override bool Equals(Object obj)
        {
            return obj is ValidatorId && this == (ValidatorId)obj;
        }

        public override int GetHashCode()
        {
            return _fullName == null ? 0 : _fullName.GetHashCode();
        }

        public static bool operator ==(ValidatorId x, ValidatorId y)
        {
            return x._fullName == y._fullName;
        }

        public static bool operator !=(ValidatorId x, ValidatorId y)
        {
            return !(x == y);
        }
    }
}
