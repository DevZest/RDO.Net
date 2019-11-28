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
        public void ModelMapper_AddComputation_CS()
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
            document = mapper.AddComputation("Compute", "Description", ComputationMode.Aggregate);
            var actual = document.GetTextAsync().Result.ToString();
            var expected =
@"using DevZest.Data;
using DevZest.Data.Annotations;

namespace Test
{
    [Computation(nameof(Compute), ComputationMode.Aggregate, Description = ""Description"")]
    public class MyModel : Model
    {
        [_Computation]
        private void Compute()
        {
            throw new global::System.NotImplementedException();
        }
    }
}
";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ModelMapper_AddComputation_VB()
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
            document = mapper.AddComputation("Compute", "Description", ComputationMode.Aggregate);
            var actual = document.GetTextAsync().Result.ToString();
            var expected =
@"Imports DevZest.Data
Imports DevZest.Data.Annotations

Namespace Test
    <Computation(""Compute"", ComputationMode.Aggregate, Description:=""Description"")>
    Public Class MyModel
        Inherits Model

        <_Computation>
        Private Sub Compute()
            Throw New Global.System.NotImplementedException()
        End Sub
    End Class
End Namespace
";
            Assert.AreEqual(expected, actual);
        }
    }
}
