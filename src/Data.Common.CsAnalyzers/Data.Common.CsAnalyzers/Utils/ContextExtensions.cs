// Code from: https://github.com/Wintellect/Wintellect.Analyzers/blob/master/Source/Wintellect.Analyzers/Wintellect.Analyzers/Extensions/ContextExtensions.cs
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.IO;

static class ContextExtensions
{
    public static bool IsGeneratedOrNonUserCode(this SyntaxNodeAnalysisContext context)
    {
        if (context.Node.IsGeneratedOrNonUserCode())
            return true;

        return context.SemanticModel?.SyntaxTree.IsGeneratedOrNonUserCode() ?? false;
    }

    public static Boolean IsGeneratedOrNonUserCode(this SyntaxTree tree)
    {
        return IsGeneratedCodeFilename(tree.FilePath);
    }

    public static bool IsGeneratedOrNonUserCode(this SymbolAnalysisContext context)
    {
        // Check the symbol first as the attributes are more likely than the filename.
        if (context.Symbol.IsGeneratedOrNonUserCode())
            return true;

        // Loop through all places where this Symbol could be declared. This accounts for
        // partial classes and the like.
        for (Int32 i = 0; i < context.Symbol.DeclaringSyntaxReferences.Length; i++)
        {
            SyntaxReference currRef = context.Symbol.DeclaringSyntaxReferences[i];

            // Check the tree itself which hits the filename.
            if (currRef?.SyntaxTree.IsGeneratedOrNonUserCode() == true)
                return true;
        }

        return false;
    }

    // This code is lifted from: 
    // https://github.com/dotnet/roslyn/blob/master/src/Workspaces/Core/Portable/GeneratedCodeRecognition/GeneratedCodeRecognitionServiceFactory.cs
    private static bool IsGeneratedCodeFilename(String fileName)
    {
        if (fileName.StartsWith("TemporaryGeneratedFile_", StringComparison.OrdinalIgnoreCase))
            return true;

        String extension = Path.GetExtension(fileName);
        if (extension.Length != 0)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);

            if (fileName.EndsWith("AssemblyInfo", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".designer", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".generated", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".g", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".g.i", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".AssemblyAttributes", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
