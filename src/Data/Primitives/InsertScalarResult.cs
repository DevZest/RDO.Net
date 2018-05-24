
namespace DevZest.Data.Primitives
{
    public struct InsertScalarResult
    {
        public InsertScalarResult(bool success, int? identityValue)
        {
            Success = success;
            IdentityValue = identityValue;
        }

        public readonly bool Success;

        public readonly int? IdentityValue;
    }
}
