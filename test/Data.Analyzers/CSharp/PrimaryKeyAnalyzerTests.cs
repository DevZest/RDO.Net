using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.CodeAnalysis.CSharp
{
    [TestClass]
    public class PrimaryKeyAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new PrimaryKeyAnalyzer();
        }

        //No diagnostics expected to show up
        [TestMethod]
        public void EmptySource()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void NoError()
        {
            var test = @"
using DevZest.Data;

public sealed class PK : PrimaryKey
{
    public PK(_Int32 id)
        : base(id)
    {
    }

    public _Int32 ID
    {
        return GetColumn<_Int32>(0);
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void NotSealed()
        {
            var test = @"
using DevZest.Data;

public class PK : PrimaryKey
{
    public PK(_Int32 id)
        : base(id)
    {
    }

    public _Int32 ID
    {
        return GetColumn<_Int32>(0);
    }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.PrimaryKeyNotSealed,
                Message = Resources.PrimaryKeyNotSealed_Message,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 4, 14) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void InvalidConstructors()
        {
            var test = @"
using DevZest.Data;

public sealed class PK : PrimaryKey
{
    public _Int32 ID
    {
        return GetColumn<_Int32>(0);
    }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.PrimaryKeyInvalidConstructors,
                Message = Resources.PrimaryKeyInvalidConstructors_Message,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 4, 21) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void ParameterlessConstructor()
        {
            var test = @"
using DevZest.Data;

public sealed class PK : PrimaryKey
{
    public PK()
        : base()
    {
    }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.PrimaryKeyParameterlessConstructor,
                Message = Resources.PrimaryKeyParameterlessConstructor_Message,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 12) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
    }
}
