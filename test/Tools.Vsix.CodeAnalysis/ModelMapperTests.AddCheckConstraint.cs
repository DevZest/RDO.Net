using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapperTests
    {
        [TestMethod]
        public void ModelMapper_AddCheckConstraint_CS()
        {
            var src =
@"using DevZest.Data;
using DevZest.Data.Annotations;

[assembly: MessageResource(typeof(Test.Resources.Messages))]

namespace Test
{
    public class MyModel : Model
    {
    }
}

namespace Test.Resources
{
    public static class Messages
    {
        public static string CK_MyModel
        {
            get { return ""Check message for MyModel.""; }
        }
    }
}
";

            var document = src.CreateDocument();
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(160, 0));
            var messageResourceType = mapper.GetMessageResourceType();
            var messageResourceProperty = messageResourceType.GetMembers("CK_MyModel").OfType<IPropertySymbol>().Single();
            document = mapper.AddCheckConstraint("CK_MyModel", "Description", messageResourceType, messageResourceProperty, null);
            var actual = document.GetTextAsync().Result.ToString();
            var expected =
@"using DevZest.Data;
using DevZest.Data.Annotations;
using Test.Resources;

[assembly: MessageResource(typeof(Test.Resources.Messages))]

namespace Test
{
    [CheckConstraint(nameof(CK_MyModel), typeof(Messages), nameof(Messages.CK_MyModel), Description = ""Description"")]
    public class MyModel : Model
    {
        [_CheckConstraint]
        private _Boolean CK_MyModel
        {
            get
            {
                throw new global::System.NotImplementedException();
            }
        }
    }
}

namespace Test.Resources
{
    public static class Messages
    {
        public static string CK_MyModel
        {
            get { return ""Check message for MyModel.""; }
        }
    }
}
";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ModelMapper_AddCheckConstraint_VB()
        {
            var src =
@"Imports DevZest.Data
Imports DevZest.Data.Annotations

<assembly: MessageResource(GetType(Test.Resources.Messages))>

Namespace Test
    Public Class MyModel
        Inherits Model

    End Class
End Namespace

Namespace Test.Resources
    Public Shared Class Messages
        Public Shared ReadOnly Property CK_MyModel As String
            Get
                Return ""Check message for MyModel.""
            End Get
        End Property
    End Class
End Namespace
";

            var document = src.CreateDocument(LanguageNames.VisualBasic);
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(160, 0));
            var messageResourceType = mapper.GetMessageResourceType();
            var messageResourceProperty = messageResourceType.GetMembers("CK_MyModel").OfType<IPropertySymbol>().Single();
            document = mapper.AddCheckConstraint("CK_MyModel", "Description", messageResourceType, messageResourceProperty, null);
            var actual = document.GetTextAsync().Result.ToString();
            var expected =
@"Imports DevZest.Data
Imports DevZest.Data.Annotations
Imports Test.Resources

<assembly: MessageResource(GetType(Test.Resources.Messages))>

Namespace Test
    <CheckConstraint(""CK_MyModel"", GetType(Messages), NameOf(Messages.CK_MyModel), Description:=""Description"")>
    Public Class MyModel
        Inherits Model

        <_CheckConstraint>
        Private ReadOnly Property CK_MyModel As _Boolean
            Get
                Throw New Global.System.NotImplementedException()
            End Get
        End Property
    End Class
End Namespace

Namespace Test.Resources
    Public Shared Class Messages
        Public Shared ReadOnly Property CK_MyModel As String
            Get
                Return ""Check message for MyModel.""
            End Get
        End Property
    End Class
End Namespace
";
            Assert.AreEqual(expected, actual);
        }
    }
}
