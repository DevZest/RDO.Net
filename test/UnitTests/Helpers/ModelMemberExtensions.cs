using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Helpers
{
    internal static class ModelMemberExtensions
    {
        public static void Verify(this ModelMember modelProperty, Model model, Type declaringType, string name)
        {
            Assert.AreEqual(model, modelProperty.GetParentModel());
            Assert.AreEqual(declaringType, modelProperty.DeclaringType);
            Assert.AreEqual(name, modelProperty.Name);
        }
    }
}
