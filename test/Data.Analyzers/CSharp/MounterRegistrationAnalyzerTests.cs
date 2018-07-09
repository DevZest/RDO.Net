using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Analyzers.CSharp
{
    [TestClass]
    public class MounterRegistrationAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MounterRegistrationAnalyzer();
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

class SimpleModel : Model
{
    public static readonly Mounter<_Int32> _Column1 = RegisterColumn((SimpleModel x) => x.Column1);
    public static readonly Mounter<_Int32> _Column2;

    static SimpleModel()
    {
        _Column2 = RegisterColumn((SimpleModel x) => x.Column2);
        RegisterColumn((SimpleModel x) => x.Column3);
    }

    public _Int32 Column1 { get; private set; }

    public _Int32 Column2 { get; private set; }

    public _Int32 Column3 { get; private set; }
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void InvalidInvocation_assigned_to_local_variable()
        {
            var test = @"
using DevZest.Data;

class SimpleModel : Model
{
    public static readonly Mounter<_Int32> _Column1 = RegisterColumn((SimpleModel x) => x.Column1);

    static SimpleModel()
    {
        var column1 = RegisterColumn((SimpleModel x) => x.Column1);
    }

    public _Int32 Column1 { get; private set; }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.InvalidRegisterMounterInvocation,
                Message = Resources.InvalidRegisterMounterInvocation_Message,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 23) }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void InvalidInvocation_in_a_separate_method()
        {
            var test = @"
using DevZest.Data;

class SimpleModel : Model
{
    public static readonly Mounter<_Int32> _Column1 = RegisterColumn((SimpleModel x) => x.Column1);

    private static void AnotherMethod()
    {
        _Column1 = RegisterColumn((SimpleModel x) => x.Column1);
    }

    public _Int32 Column1 { get; private set; }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.InvalidRegisterMounterInvocation,
                Message = Resources.InvalidRegisterMounterInvocation_Message,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 20) }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void DuplicateRegistration_in_static_constructor()
        {
            var test = @"
using DevZest.Data;

class SimpleModel : Model
{
    public static readonly Mounter<_Int32> _Column1 = RegisterColumn((SimpleModel x) => x.Column1);

    static SimpleModel()
    {
        RegisterColumn((SimpleModel x) => x.Column1);
    }

    public _Int32 Column1 { get; private set; }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.DuplicateMounterRegistration,
                Message = string.Format(Resources.DuplicateMounterRegistration_Message, "Column1"),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 9) }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void DuplicateRegistration_in_field_initializer()
        {
            var test = @"
using DevZest.Data;

class SimpleModel : Model
{
    static SimpleModel()
    {
        RegisterColumn((SimpleModel x) => x.Column1);
    }

    public static readonly Mounter<_Int32> _Column1 = RegisterColumn((SimpleModel x) => x.Column1);

    public _Int32 Column1 { get; private set; }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.DuplicateMounterRegistration,
                Message = string.Format(Resources.DuplicateMounterRegistration_Message, "Column1"),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 55) }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void MounterNaming_in_field_initializer()
        {
            var test = @"
using DevZest.Data;

class SimpleModel : Model
{
    public static readonly Mounter<_Int32> _Column2 = RegisterColumn((SimpleModel x) => x.Column1);

    public _Int32 Column1 { get; private set; }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.MounterNaming,
                Message = string.Format(Resources.MounterNaming_Message, "_Column2", "Column1", "_Column1"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 44) }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void MounterNaming_in_static_constructor()
        {
            var test = @"
using DevZest.Data;

class SimpleModel : Model
{
    public static readonly Mounter<_Int32> _Column2

    static SimpleModel()
    {
        _Column2 = RegisterColumn((SimpleModel x) => x.Column1);
    }

    public _Int32 Column1 { get; private set; }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.MounterNaming,
                Message = string.Format(Resources.MounterNaming_Message, "_Column2", "Column1", "_Column1"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 9) }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void RegisterColumn_InvalidLocalColumn()
        {
            var test = @"
using DevZest.Data;

class SimpleModel : Model
{
    static SimpleModel()
    {
        RegisterColumn((SimpleModel _) => _.Column1);
    }

    public LocalColumn<int> Column1 { get; private set; }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.InvalidRegisterLocalColumn,
                Message = string.Format(Resources.InvalidRegisterLocalColumn_Message, "Column1"),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 8, 9) }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void RegisterColumn_MissingRegistration()
        {
            var test = @"
using DevZest.Data;

class SimpleModel : Model
{
    public _Int32 Column { get; private set; }

    private _Int32 _computedColumn;
    public _Int32 ComputedColumn
    {
        get { return _computedColumn ?? (_computedColumn = Column * 2); }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.MissingMounterRegistration,
                Message = string.Format(Resources.MissingMounterRegistration_Message, "Column"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 19) }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void RegisterColumn_MissingRegistration_LocalColumn()
        {
            var test = @"
using DevZest.Data;

class SimpleModel : Model
{
    public LocalColumn<int> Column { get; private set; }
}";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.MissingMounterRegistration,
                Message = string.Format(Resources.MissingMounterRegistration_Message, "Column"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 29) }
            };
            VerifyCSharpDiagnostic(test, expected);
        }
    }
}
