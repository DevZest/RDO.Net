
namespace DevZest.Data.Primitives
{
    public struct InsertScalarResult
    {
        public InsertScalarResult(bool success, long? identityValue)
        {
            Success = success;
            IdentityValue = identityValue;
        }

        public readonly bool Success;

        public readonly long? IdentityValue;
    }
}
