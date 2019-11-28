using DevZest.Data.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapperTests
    {
        [TestMethod]
        public void ModelMapper_AddCustomValidator_CS()
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
            document = mapper.AddCustomValidator("VAL_Xxx", "Description");
            var actual = document.GetTextAsync().Result.ToString();
            var expected =
@"using DevZest.Data;
using DevZest.Data.Annotations;

namespace Test
{
    [CustomValidator(nameof(VAL_Xxx), Description = ""Description"")]
    public class MyModel : Model
    {
        [_CustomValidator]
        private CustomValidatorEntry VAL_Xxx
        {
            get
            {
                string Validate(DataRow dataRow)
                {
                    throw new global::System.NotImplementedException();
                }

                IColumns GetSourceColumns()
                {
                    throw new global::System.NotImplementedException();
                }

                return new CustomValidatorEntry(Validate, GetSourceColumns);
            }
        }
    }
}
";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ModelMapper_AddCustomValidator_VB()
        {
            var src =
@"Imports DevZest.Data

Namespace Test
    Public Class MyModel
        Inherits Model

    End Class
End Namespace
";

            var document = src.CreateDocument(LanguageNames.VisualBasic);
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            document = mapper.AddCustomValidator("VAL_Xxx", "Description");
            var actual = document.GetTextAsync().Result.ToString();
            var expected =
@"Imports DevZest.Data
Imports DevZest.Data.Annotations

Namespace Test
    <CustomValidator(""VAL_Xxx"", Description:=""Description"")>
    Public Class MyModel
        Inherits Model

        <_CustomValidator>
        Private ReadOnly Property VAL_Xxx As CustomValidatorEntry
            Get
                Dim validate = Function(dataRow As DataRow) As String
                                   Throw New Global.System.NotImplementedException()
                               End Function

                Dim getSourceColumns = Function() As IColumns
                                           Throw New Global.System.NotImplementedException()
                                       End Function

                Return New CustomValidatorEntry(validate, getSourceColumns)
            End Get
        End Property
    End Class
End Namespace
";
            Assert.AreEqual(expected, actual);
        }
    }
}
