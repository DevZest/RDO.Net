using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.CodeAnalysis.CSharp
{
    [TestClass]
    public class CSharpDbMockCodeFixTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DbMockAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpDbMockCodeFixProvider();
        }

        [TestMethod]
        public void MissingFactoryMethod()
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
}";

            var expected =
@"using DevZest.Data.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class MyMock : DbMock<DbSession>
    {
        public static Task<DbSession> CreateAsync(DbSession db, IProgress<DbInitProgress> progress = null, CancellationToken ct = default(CancellationToken))
        {
            return new MyMock().MockAsync(db, progress, ct);
        }

        protected override void Initialize()
        {
        }
    }
}";
            VerifyCSharpFix(test, expected);
        }

    }
}
