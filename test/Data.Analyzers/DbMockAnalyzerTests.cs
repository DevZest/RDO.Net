using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.CodeAnalysis
{
    [TestClass]
    public class DbMockAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DbMockAnalyzer();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DbMockAnalyzer();
        }

        [TestMethod]
        public void MissingStaticFactory_CS()
        {
            var test =
@"using DevZest.Data.Primitives;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class MyMock : DbMock<DbSession>
    {
        protected override void Initialize()
        {
        }
    }
}
";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.MissingDbMockFactoryMethod,
                Message = Resources.MissingDbMockFactoryMethod_Message,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 5, 18) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void NoMissingStaticFactory_CS()
        {
            var test =
@"using DevZest.Data.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class MyMockWithoutWarning : DbMock<DbSession>
    {
        public static Task<DbSession> CreateAsync(DbSession db, IProgress<DbGenerationProgress> progress = null, CancellationToken ct = default(CancellationToken))
        {
            return new MyMockWithoutWarning().MockAsync(db, progress, ct);
        }

        protected override void Initialize()
        {
        }
    }
}
";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void MissingStaticFactory_VB()
        {
            var test =
@"Imports DevZest.Data
Imports DevZest.Data.Primitives

Public Class MyMock
    Inherits DbMock(Of DbSession)

    Protected Overrides Sub Initialize()
    End Sub
End Class
";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.MissingDbMockFactoryMethod,
                Message = Resources.MissingDbMockFactoryMethod_Message,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 4, 14) }
            };

            VerifyBasicDiagnostic(test, expected);
        }

        [TestMethod]
        public void NoMissingStaticFactory_VB()
        {
            var test =
@"Imports System
Imports System.Threading
Imports System.Threading.Tasks
Imports DevZest.Data.Primitives

Public Class MyMockWithoutWarning
    Inherits DbMock(Of DbSession)

    Public Shared Function CreateAsync(db As DbSession, Optional progress As IProgress(Of DbGenerationProgress) = Nothing, Optional ct As CancellationToken = Nothing) As Task(Of DbSession)
        Return New MyMockWithoutWarning().MockAsync(db, progress, ct)
    End Function

    Protected Overrides Sub Initialize()
    End Sub
End Class

";

            VerifyBasicDiagnostic(test);
        }

    }
}
