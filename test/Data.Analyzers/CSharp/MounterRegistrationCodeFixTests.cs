using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Analyzers.CSharp
{
    [TestClass]
    public class MounterRegistrationCodeFixTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MounterRegistrationAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new MounterRegistrationCodeFixProvider();
        }

        [TestMethod]
        public void GenerateStaticConstructor()
        {
            var test = @"
using DevZest.Data;

class SimpleModel : Model
{
    public _Int32 Column1 { get; private set; }
}";
            var expected = @"
using DevZest.Data;

class SimpleModel : Model
{
    public static readonly Mounter<_Int32> _Column1;

    static SimpleModel()
    {
        _Column1 = RegisterColumn((SimpleModel _) => _.Column1);
    }

    public _Int32 Column1 { get; private set; }
}";
            VerifyCSharpFix(test, expected, 0);
        }

        [TestMethod]
        public void GenerateRegisterColumn()
        {
            var test = @"
using DevZest.Data;

class SimpleModel : Model
{
    public static readonly Mounter<_Int32> _Column1;

    static SimpleModel()
    {
        _Column1 = RegisterColumn((SimpleModel _) => _.Column1);
    }

    public _Int32 Column1 { get; private set; }

    public _Int32 Column2 { get; private set; }
}";

            var expected = @"
using DevZest.Data;

class SimpleModel : Model
{
    public static readonly Mounter<_Int32> _Column1;

    static SimpleModel()
    {
        _Column1 = RegisterColumn((SimpleModel _) => _.Column1);
        RegisterColumn((SimpleModel _) => _.Column2);
    }

    public _Int32 Column1 { get; private set; }

    public _Int32 Column2 { get; private set; }
}";
            VerifyCSharpFix(test, expected, 1);
        }
    }
}
