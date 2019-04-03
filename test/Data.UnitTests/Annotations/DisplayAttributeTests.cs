using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class DisplayAttributeTests
    {
        private static class Strings
        {
            public const string LocalizableName = "Localizable Name";
            public const string LocalizableShortName = "Localizable ShortName";
            public const string LocalizableDescription = "Localizable Description";
            public const string LocalizablePrompt = "Localizable Prompt";

            internal static string Name
            {
                get { return LocalizableName; }
            }

            internal static string ShortName
            {
                get { return LocalizableShortName; }
            }

            public static string Description
            {
                get { return LocalizableDescription; }
            }

            public static string Prompt
            {
                get { return LocalizablePrompt; }
            }
        }

        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Text1);
                RegisterColumn((TestModel _) => _.Text2);
            }

            [Display(Name=nameof(Strings.Name), ShortName = nameof(Strings.ShortName), Description = nameof(Strings.Description), Prompt = nameof(Strings.Prompt))]
            public _String Text1 { get; private set; }

            [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Name), ShortName = nameof(Strings.ShortName), Description = nameof(Strings.Description), Prompt = nameof(Strings.Prompt))]
            public _String Text2 { get; private set; }
        }

        [TestMethod]
        public void DisplayAttribute()
        {
            var _ = new TestModel();

            Assert.AreEqual(_.Text1.DisplayName, nameof(Strings.Name));
            Assert.AreEqual(_.Text1.DisplayShortName, nameof(Strings.ShortName));
            Assert.AreEqual(_.Text1.DisplayDescription, nameof(Strings.Description));
            Assert.AreEqual(_.Text1.DisplayPrompt, nameof(Strings.Prompt));

            Assert.AreEqual(_.Text2.DisplayName, Strings.LocalizableName);
            Assert.AreEqual(_.Text2.DisplayShortName, Strings.LocalizableShortName);
            Assert.AreEqual(_.Text2.DisplayDescription, Strings.LocalizableDescription);
            Assert.AreEqual(_.Text2.DisplayPrompt, Strings.LocalizablePrompt);
        }
    }
}
