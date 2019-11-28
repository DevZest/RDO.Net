using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    [TestClass]
    public class ClassMapperTests
    {
        [TestMethod]
        public void AddMemberAttribute_first_CS()
        {
            var src =
@"using DevZest.Data;

namespace Test
{
    public class MyModel : Model
    {
        public static readonly Mounter<_Int32> _ID = RegisterColumn((MyModel _) => _.ID);

        public _Int32 ID { get; private set; }
    }
}
";

            var document = src.CreateDocument();
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            var property = mapper.ModelType.GetMembers().OfType<IPropertySymbol>().SingleOrDefault();
            var attribute = mapper.Compilation.GetKnownType("DevZest.Data.Annotations.DbColumnAttribute");
            var result = mapper.AddMemberAttribute(property, attribute, KnownTypes.ModelDesignerSpecAttribute).Result;

            var resultDocument = result.Document;
            var expected =
@"using DevZest.Data;
using DevZest.Data.Annotations;

namespace Test
{
    public class MyModel : Model
    {
        public static readonly Mounter<_Int32> _ID = RegisterColumn((MyModel _) => _.ID);

        [DbColumn()]
        public _Int32 ID { get; private set; }
    }
}
";
            var resultText = resultDocument.GetTextAsync().Result;
            Assert.AreEqual(expected, resultText.ToString());

            var resultTextSpan = result.TextSpan.Value;
            Assert.AreEqual(resultText.GetPosition(10, 19), resultTextSpan.Start);
        }

        [TestMethod]
        public void AddMemberAttribute_second_CS()
        {
            var src =
@"using DevZest.Data;
using DevZest.Data.Annotations;

namespace Test
{
    public class MyModel : Model
    {
        public static readonly Mounter<_Int32> _ID = RegisterColumn((MyModel _) => _.ID);

        [DbColumn()]
        public _Int32 ID { get; private set; }
    }
}
";

            var document = src.CreateDocument();
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            var property = mapper.ModelType.GetMembers().OfType<IPropertySymbol>().SingleOrDefault();
            var attribute = mapper.Compilation.GetKnownType("DevZest.Data.Annotations.RequiredAttribute");
            var result = mapper.AddMemberAttribute(property, attribute, KnownTypes.ModelDesignerSpecAttribute).Result;

            var resultDocument = result.Document;
            var expected =
@"using DevZest.Data;
using DevZest.Data.Annotations;

namespace Test
{
    public class MyModel : Model
    {
        public static readonly Mounter<_Int32> _ID = RegisterColumn((MyModel _) => _.ID);

        [DbColumn()]
        [Required]
        public _Int32 ID { get; private set; }
    }
}
";
            var resultText = resultDocument.GetTextAsync().Result;
            Assert.AreEqual(expected, resultText.ToString());

            Assert.AreEqual(null, result.TextSpan);
        }

        [TestMethod]
        public void RemoveMemberAttribute_CS()
        {
            var src =
@"using DevZest.Data;
using DevZest.Data.Annotations;

namespace Test
{
    public class MyModel : Model
    {
        public static readonly Mounter<_Int32> _ID = RegisterColumn((MyModel _) => _.ID);

        [DbColumn()]
        public _Int32 ID { get; private set; }
    }
}
";

            var document = src.CreateDocument();
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            var property = mapper.ModelType.GetMembers().OfType<IPropertySymbol>().SingleOrDefault();
            var attribute = property.GetAttributes().Single();
            var result = mapper.RemoveMemberAttributeAsync(attribute).Result;

            var expected =
@"using DevZest.Data;
using DevZest.Data.Annotations;

namespace Test
{
    public class MyModel : Model
    {
        public static readonly Mounter<_Int32> _ID = RegisterColumn((MyModel _) => _.ID);

        public _Int32 ID { get; private set; }
    }
}
";
            Assert.AreEqual(expected, result.GetTextAsync().Result.ToString());
        }

        [TestMethod]
        public void AddMemberAttribute_first_VB()
        {
            var src =
@"Imports DevZest.Data

Namespace Test
    Public Class MyModel
        Inherits Model

        Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As MyModel) x.ID)

        Private m_ID As _Int32
        Public Property ID As _Int32
            Get
                Return m_ID
            End Get
            Private Set
                m_ID = Value
            End Set
        End Property
    End Class
End Namespace
";

            var document = src.CreateDocument(LanguageNames.VisualBasic);
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            var property = mapper.ModelType.GetMembers().OfType<IPropertySymbol>().SingleOrDefault();
            var attribute = mapper.Compilation.GetKnownType("DevZest.Data.Annotations.DbColumnAttribute");
            var result = mapper.AddMemberAttribute(property, attribute, KnownTypes.ModelDesignerSpecAttribute).Result;

            var resultDocument = result.Document;
            var expected =
@"Imports DevZest.Data
Imports DevZest.Data.Annotations

Namespace Test
    Public Class MyModel
        Inherits Model

        Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As MyModel) x.ID)

        Private m_ID As _Int32
        <DbColumn()>
        Public Property ID As _Int32
            Get
                Return m_ID
            End Get
            Private Set
                m_ID = Value
            End Set
        End Property
    End Class
End Namespace
";
            var resultText = resultDocument.GetTextAsync().Result;
            Assert.AreEqual(expected, resultText.ToString());

            var resultTextSpan = result.TextSpan.Value;
            Assert.AreEqual(resultText.GetPosition(11, 19), resultTextSpan.Start);
        }

        [TestMethod]
        public void AddMemberAttribute_second_VB()
        {
            var src =
@"Imports DevZest.Data
Imports DevZest.Data.Annotations

Namespace Test
    Public Class MyModel
        Inherits Model

        Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As MyModel) x.ID)

        Private m_ID As _Int32
        <DbColumn()>
        Public Property ID As _Int32
            Get
                Return m_ID
            End Get
            Private Set
                m_ID = Value
            End Set
        End Property
    End Class
End Namespace
";

            var document = src.CreateDocument(LanguageNames.VisualBasic);
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            var property = mapper.ModelType.GetMembers().OfType<IPropertySymbol>().SingleOrDefault();
            var attribute = mapper.Compilation.GetKnownType("DevZest.Data.Annotations.RequiredAttribute");
            var result = mapper.AddMemberAttribute(property, attribute, KnownTypes.ModelDesignerSpecAttribute).Result;

            var resultDocument = result.Document;
            var expected =
@"Imports DevZest.Data
Imports DevZest.Data.Annotations

Namespace Test
    Public Class MyModel
        Inherits Model

        Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As MyModel) x.ID)

        Private m_ID As _Int32
        <DbColumn()>
        <Required>
        Public Property ID As _Int32
            Get
                Return m_ID
            End Get
            Private Set
                m_ID = Value
            End Set
        End Property
    End Class
End Namespace
";
            var resultText = resultDocument.GetTextAsync().Result;
            Assert.AreEqual(expected, resultText.ToString());
            Assert.AreEqual(null, result.TextSpan);
        }

        [TestMethod]
        public void RemoveMemberAttribute_VB()
        {
            var src =
@"Imports DevZest.Data
Imports DevZest.Data.Annotations

Namespace Test
    Public Class MyModel
        Inherits Model

        Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As MyModel) x.ID)

        Private m_ID As _Int32
        <DbColumn()>
        Public Property ID As _Int32
            Get
                Return m_ID
            End Get
            Private Set
                m_ID = Value
            End Set
        End Property
    End Class
End Namespace
";

            var document = src.CreateDocument(LanguageNames.VisualBasic);
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            var property = mapper.ModelType.GetMembers().OfType<IPropertySymbol>().SingleOrDefault();
            var attribute = property.GetAttributes().Single();
            var result = mapper.RemoveMemberAttributeAsync(attribute).Result;

            var expected =
@"Imports DevZest.Data
Imports DevZest.Data.Annotations

Namespace Test
    Public Class MyModel
        Inherits Model

        Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As MyModel) x.ID)

        Private m_ID As _Int32
        Public Property ID As _Int32
            Get
                Return m_ID
            End Get
            Private Set
                m_ID = Value
            End Set
        End Property
    End Class
End Namespace
";
            Assert.AreEqual(expected, result.GetTextAsync().Result.ToString());
        }

        [TestMethod]
        public void GetMessageResourceType()
        {
            var src =
@"using DevZest.Data;
using DevZest.Data.Annotations;

[assembly: MessageResource(typeof(Test.MyModel))]

namespace Test
{
    public class MyModel : Model
    {
    }
}
";

            var document = src.CreateDocument();
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            var resourceType = mapper.GetMessageResourceType();
            Assert.AreEqual("MyModel", resourceType.Name);
        }

        [TestMethod]
        public void GetMessageResourceType_without_MessageResourceAttribute()
        {
            var src =
@"using DevZest.Data;

namespace Test
{
    public class MyModel : Model
    {
    }
}
";

            var document = src.CreateDocument();
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            var resourceType = mapper.GetMessageResourceType();
            Assert.IsNull(resourceType);
        }
    }
}
