﻿using DevZest.Data.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.CodeAnalysis
{
    [TestClass]
    public class ModelAttributeAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ModelAttributeAnalyzer();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ModelAttributeAnalyzer();
        }

        [TestMethod]
        public void InvalidImplementationAttribute_CS()
        {
            var test =
@"using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class InvalidImplementationAttribute : Model
    {
        [_DbIndex]
        public _Boolean CK_AlwaysTrue
        {
            get { return _Boolean.Const(true); }
        }
    }
}
";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.InvalidImplementationAttribute,
                Message = string.Format(Resources.InvalidImplementationAttribute_Message, typeof(_DbIndexAttribute), Resources.Property, typeof(ColumnSort[]), null),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 7, 10) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void MissingModelAttribute_CS()
        {
            var test =
@"using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class MissingModelAttribute : Model
    {
        [_CheckConstraint]
        private _Boolean CK_AlwaysTrue
        {
            get { return _Boolean.Const(true); }
        }
    }
}
";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.MissingModelAttribute,
                Message = string.Format(Resources.MissingModelAttribute_Message, typeof(CheckConstraintAttribute), "CK_AlwaysTrue"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 7, 10) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void DuplicateModelAttribute_CS()
        {
            var test =
@"using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    [CheckConstraint(nameof(CK_AlwaysTrue), ""CK"")]
    [CheckConstraint(nameof(CK_AlwaysTrue), ""CK"")]
    public class DuplicateModelAttribute : Model
    {
        [_CheckConstraint]
        private _Boolean CK_AlwaysTrue
        {
            get { return _Boolean.Const(true); }
        }
    }
}
";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.DuplicateModelAttribute,
                Message = string.Format(Resources.DuplicateModelAttribute_Message, typeof(CheckConstraintAttribute), "CK_AlwaysTrue"),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 6) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void MissingImplementation_CS()
        {
            var test =
@"using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    [CheckConstraint(""CK_AlwaysTrue"", ""CK"")]
    public class MissingImplementationAttribute : Model
    {
    }
}
";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.MissingImplementation,
                Message = string.Format(Resources.MissingImplementation_Message, Resources.Property, "CK_AlwaysTrue", typeof(_Boolean), null),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 5, 6) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void MissingImplementationAttribute_CS()
        {
            var test =
@"using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    [CheckConstraint(nameof(CK_AlwaysTrue), ""CK"")]
    public class MissingImplementationAttribute : Model
    {
        private _Boolean CK_AlwaysTrue
        {
            get { return _Boolean.Const(true); }
        }
    }
}
";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.MissingImplementationAttribute,
                Message = string.Format(Resources.MissingImplementationAttribute_Message, typeof(_CheckConstraintAttribute)),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 8, 26) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void InvalidImplementationAttribute_VB()
        {
            var test =
@"Imports DevZest.Data
Imports DevZest.Data.Annotations

Public Class InvalidImplementationAttribute
    Inherits Model

    <_DbIndex>
    Private ReadOnly Property CK_AlwaysTrue As _Boolean
        Get
            Return _Boolean.Const(True)
        End Get
    End Property
End Class";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.InvalidImplementationAttribute,
                Message = string.Format(Resources.InvalidImplementationAttribute_Message, typeof(_DbIndexAttribute), Resources.Property, "DevZest.Data.ColumnSort()", null),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 7, 6) }
            };

            VerifyBasicDiagnostic(test, expected);
        }
    }

    // Not necessary to repeat other VB diagnostic tests because ModelAttributeAnalyzer is language agnostic.
}