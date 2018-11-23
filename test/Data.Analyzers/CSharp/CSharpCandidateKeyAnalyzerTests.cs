using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.CodeAnalysis.CSharp
{
    [TestClass]
    public class CSharpCandidateKeyAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpCandidateKeyAnalyzer();
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

public sealed class PK : CandidateKey
{
    public PK(_Int32 id)
        : base(id)
    {
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void NotSealed()
        {
            var test = @"
using DevZest.Data;

public class PK : CandidateKey
{
    public PK(_Int32 id)
        : base(id)
    {
    }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.CandidateKeyNotSealed,
                Message = Resources.CandidateKeyNotSealed_Message,
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

public sealed class PK : CandidateKey
{
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.CandidateKeyInvalidConstructors,
                Message = Resources.CandidateKeyInvalidConstructors_Message,
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

public sealed class PK : CandidateKey
{
    public PK()
        : base()
    {
    }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.CandidateKeyParameterlessConstructor,
                Message = Resources.CandidateKeyParameterlessConstructor_Message,
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

public sealed class PK : CandidateKey
{
    public PK(int id)
        : base()
    {
    }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.CandidateKeyInvalidConstructorParam,
                Message = string.Format(Resources.CandidateKeyInvalidConstructorParam_Message, "id"),
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

public sealed class PK : CandidateKey
{
    public PK([Asc] [Desc]_Int32 id)
        : base(id)
    {
    }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.CandidateKeySortAttributeConflict,
                Message = string.Format(Resources.CandidateKeySortAttributeConflict_Message),
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

public sealed class PK : CandidateKey
{
    public PK(_Int32 id)
    {
    }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.CandidateKeyMissingBaseConstructor,
                Message = string.Format(Resources.CandidateKeyMissingBaseConstructor_Message),
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

public sealed class PK : CandidateKey
{
    public PK(_Int32 id)
        : base()
    {
    }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.CandidateKeyMismatchBaseConstructor,
                Message = string.Format(Resources.CandidateKeyMismatchBaseConstructor_Message, 1),
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

public sealed class PK : CandidateKey
{
    public PK(_Int32 id1, _Int32 id2)
        : base(id1, id1)
    {
    }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.CandidateKeyMismatchBaseConstructorArgument,
                Message = string.Format(Resources.CandidateKeyMismatchBaseConstructorArgument_Message, "id2"),
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

public sealed class PK : CandidateKey
{
    public PK(_Int32 id)
        : base(id.Asc())
    {
    }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.CandidateKeyMismatchSortAttribute,
                Message = string.Format(Resources.CandidateKeyMismatchSortAttribute_Message, "Unspecified", "Ascending"),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 22) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void InvalidArgument()
        {
            var test = @"
using DevZest.Data;

public class PrimaryKeyInvalidArgument : Model<PrimaryKeyInvalidArgument.PK>
{
    public sealed class PK : CandidateKey
    {
        public PK(_Int32 id)
            : base(id)
        {
        }
    }

    protected sealed override PK CreatePrimaryKey()
    {
        return new PK(1);
    }

    public static readonly Mounter<_Int32> _ID = RegisterColumn((PrimaryKeyInvalidArgument _) => _.ID);

    public _Int32 ID { get; private set; }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.CandidateKeyInvalidArgument,
                Message = string.Format(Resources.CandidateKeyInvalidArgument_Message),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 16, 23) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void ArgumentNaming()
        {
            var test = @"
using DevZest.Data;
using DevZest.Data.Annotations;

public class PrimaryKeyArgumentNaming : Model<PrimaryKeyArgumentNaming.PK>
{
    public sealed class PK : CandidateKey
    {
        public PK(_Int32 id2)
            : base(id2)
        {
        }
    }

    protected sealed override PK CreatePrimaryKey()
    {
        return new PK(ID);
    }

    public static readonly Mounter<_Int32> _ID = RegisterColumn((PrimaryKeyArgumentNaming _) => _.ID);

    [PkColumn]
    public _Int32 ID { get; private set; }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.CandidateKeyArgumentNaming,
                Message = string.Format(Resources.CandidateKeyArgumentNaming_Message, "ID", "id2"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 17, 23) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
    }
}
