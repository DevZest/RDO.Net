using Xunit;

namespace DevZest.Data.AspNetCore
{
    public class ResourcesTest
    {
        [Fact]
        public void Resource_correctly_embeded()
        {
            Assert.NotNull(UserMessages.ScalarAttribute_ValidationError);
            Assert.NotNull(DiagnosticMessages.DataSetHtmlGenerator_FieldNameCannotBeNullOrEmpty);
        }
    }
}
