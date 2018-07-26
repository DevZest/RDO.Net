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

        [TestMethod]
        public void InvalidConstructorParam()
        {
            var test = @"
using DevZest.Data;

public sealed class PK : PrimaryKey
{
    public PK(int id)
        : base()
    {
    }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.PrimaryKeyInvalidConstructorParam,
                Message = string.Format(Resources.PrimaryKeyInvalidConstructorParam_Message, "id"),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 19) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void SortAttributeConflicts()
        {
            var test = @"
using DevZest.Data;
using DevZest.Data.Annotations;

public sealed class PK : PrimaryKey
{
    public PK([Asc] [Desc]_Int32 id)
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
                Id = DiagnosticIds.PrimaryKeySortAttributeConflict,
                Message = string.Format(Resources.PrimaryKeySortAttributeConflict_Message),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 7, 22) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void MissingBaseConstructor()
        {
            var test = @"
using DevZest.Data;

public sealed class PK : PrimaryKey
{
    public PK(_Int32 id)
    {
    }

    public _Int32 ID
    {
        get { return GetColumn<_Int32>(0); }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.PrimaryKeyMissingBaseConstructor,
                Message = string.Format(Resources.PrimaryKeyMissingBaseConstructor_Message),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 12) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void MismatchBaseConstructor()
        {
            var test = @"
using DevZest.Data;

public sealed class PK : PrimaryKey
{
    public PK(_Int32 id)
        : base()
    {
    }

    public _Int32 ID
    {
        get { return GetColumn<_Int32>(0); }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.PrimaryKeyMismatchBaseConstructor,
                Message = string.Format(Resources.PrimaryKeyMismatchBaseConstructor_Message, 1),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 7, 9) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void MismatchBaseConstructorArgument()
        {
            var test = @"
using DevZest.Data;

public sealed class PK : PrimaryKey
{
    public PK(_Int32 id1, _Int32 id2)
        : base(id1, id1)
    {
    }

    public _Int32 ID1
    {
        get { return GetColumn<_Int32>(0); }
    }

    public _Int32 ID2
    {
        get { return GetColumn<_Int32>(1); }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.PrimaryKeyMismatchBaseConstructorArgument,
                Message = string.Format(Resources.PrimaryKeyMismatchBaseConstructorArgument_Message, "id2"),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 7, 21) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void MismatchSortAttribute()
        {
            var test = @"
using DevZest.Data;

public sealed class PK : PrimaryKey
{
    public PK(_Int32 id)
        : base(id.Asc())
    {
    }

    public _Int32 ID
    {
        get { return GetColumn<_Int32>(0); }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.PrimaryKeyMismatchSortAttribute,
                Message = string.Format(Resources.PrimaryKeyMismatchSortAttribute_Message, "Unspecified", "Ascending"),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 22) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
    }
}
