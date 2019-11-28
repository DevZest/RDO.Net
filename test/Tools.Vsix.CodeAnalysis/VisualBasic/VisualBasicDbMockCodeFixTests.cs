using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.CodeAnalysis.VisualBasic
{
    [TestClass]
    public class VisualBasicDbMockCodeFixTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DbMockAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new VisualBasicDbMockCodeFixProvider();
        }

        [TestMethod]
        public void MissingFactoryMethod()
        {
            var test =
@"Imports DevZest.Data
Imports DevZest.Data.Primitives

Public Class MyMock
    Inherits DbMock(Of DbSession)

    Protected Overrides Sub Initialize()
    End Sub
End Class";

            var expected =
@"Imports DevZest.Data
Imports DevZest.Data.Primitives
Imports System
Imports System.Threading
Imports System.Threading.Tasks

Public Class MyMock
    Inherits DbMock(Of DbSession)

    Public Shared Function CreateAsync(db As DbSession, Optional progress As IProgress(Of DbInitProgress) = Nothing, Optional ct As CancellationToken = Nothing) As Task(Of DbSession)
        Return New MyMock().MockAsync(db, progress, ct)
    End Function

    Protected Overrides Sub Initialize()
    End Sub
End Class";
            VerifyBasicFix(test, expected);
        }
    }
}
