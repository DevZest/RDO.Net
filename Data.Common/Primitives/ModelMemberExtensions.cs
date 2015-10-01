
namespace DevZest.Data.Primitives
{
    public static class ModelMemberExtensions
    {
        public static Model GetParentModel(this ModelMember modelMember)
        {
            return modelMember.ParentModel;
        }
    }
}
