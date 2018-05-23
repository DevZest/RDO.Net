// Code from: https://github.com/Wintellect/Wintellect.Analyzers/blob/master/Source/Wintellect.Analyzers/Wintellect.Analyzers/Extensions/SymbolExtensions.cs
using Microsoft.CodeAnalysis;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

static class SymbolExtensions
{

    /// <summary>
    /// Determines if the symbol is part of generated or non-user code.
    /// </summary>
    /// <param name="symbol">
    /// The <see cref="ISymbol"/> derived type to check.
    /// </param>
    /// <param name="checkAssembly">
    /// Set to true to check the assembly for the attributes. False to stop at the type.
    /// </param>
    /// <returns>
    /// Returns true if the item, type, or assembly has the GeneratedCode attribute 
    /// applied.
    /// </returns>
    public static bool IsGeneratedOrNonUserCode(this ISymbol symbol, bool checkAssembly = true)
    {
        // The goal here is to see if this ISymbol is part of auto generated code.
        // To do that, I'll walk up the hierarchy of item, type, to module/assembly
        // looking to see if the GeneratedCodeAttribute is set on any of them.
        var attributes = symbol.GetAttributes();
        if (!HasIgnorableAttributes(attributes))
        {
            if (symbol.Kind != SymbolKind.NamedType && HasIgnorableAttributes(symbol.ContainingType.GetAttributes()))
                return true;

            if (checkAssembly)
            {
                attributes = symbol.ContainingAssembly.GetAttributes();
                if (HasIgnorableAttributes(attributes))
                    return true;
            }

            return false;
        }

        return true;
    }

    /// <summary>
    /// Given a list of methods, finds the exact matching method based on parameter types.
    /// </summary>
    /// <param name="methods">
    /// The IEnumerable list of methods to look through.
    /// </param>
    /// <param name="parameters">
    /// The array of parameter types to look for.
    /// </param>
    /// <returns>
    /// Not null is the exact method match, null if there is no match.
    /// </returns>
    public static IMethodSymbol WithParameters(this IEnumerable<IMethodSymbol> methods, Type[] parameters)
    {
        foreach (var currMethod in methods)
        {
            if (currMethod.DoParamatersMatch(parameters))
                return currMethod;
        }
        return null;
    }

    /// <summary>
    /// Returns true if this method is an override of a specific method.
    /// </summary>
    /// <param name="method">
    /// The Roslyn method to check.
    /// </param>
    /// <param name="methodInfo">
    /// The reflection-based method you want to see is being overridden.
    /// </param>
    /// <returns>
    /// True, the Roslyn method is an override of <paramref name="methodInfo"/>.
    /// </returns>
    static public bool IsOverideOf(this IMethodSymbol method, MethodInfo methodInfo)
    {
        var methAssemblyName = methodInfo.DeclaringType.AssemblyQualifiedName;
        if (!method.GetTypesQualifiedAssemblyName().Equals(methAssemblyName))
        {
            if (!method.IsOverride)
                return false;
            return method.OverriddenMethod.IsOverideOf(methodInfo);
        }

        if (!method.Name.Equals(methodInfo.Name))
            return false;

        return method.DoParamatersMatch(methodInfo.GetParameters().Select(pi => pi.ParameterType).ToArray());
    }

    /// <summary>
    /// Returns the specific method of a class/structure with the types as parameters.
    /// </summary>
    /// <remarks>
    /// Unfortunately, Roslyn only supports the INamedTypeSymbol.GetMethods that looks up hard coded strings. This 
    /// extension method does a smarter look up by requiring you to also specify the parameters.
    /// </remarks>
    /// <param name="namedTypeSymbol">
    /// The type being extended.
    /// </param>
    /// <param name="methodName">
    /// The name of the method you want.
    /// </param>
    /// <param name="parameters">
    /// The parameter types as an array. To look up a parameter-less method, pass an empty array.
    /// </param>
    /// <returns>
    /// The matching IMethodSymbol or null if that exact method is not found.
    /// </returns>
    static public IMethodSymbol GetSpecificMethod(this INamedTypeSymbol namedTypeSymbol, string methodName, Type[] parameters)
    {
        return namedTypeSymbol.GetMembers(methodName).OfType<IMethodSymbol>().WithParameters(parameters);
    }

    /// <summary>
    /// Finds the first method either on the class/structure or it's based classes.
    /// </summary>
    /// <param name="namedTypeSymbol">
    /// The type being extended.
    /// </param>
    /// <param name="methodName">
    /// The name of the method you want.
    /// </param>
    /// <param name="parameters">
    /// The parameter types as an array. To look up a parameter-less method, pass an empty array.
    /// </param>
    /// <returns>
    /// The matching IMethodSymbol or null if that exact method is not found.
    /// </returns>
    static public IMethodSymbol FirstMethodOfSelfOrBaseType(this INamedTypeSymbol namedTypeSymbol, string methodName, Type[] parameters)
    {
        var method = namedTypeSymbol.GetSpecificMethod(methodName, parameters);
        if (method != null)
            return method;

        namedTypeSymbol = namedTypeSymbol.BaseType;
        if (namedTypeSymbol == null)
            return null;
        return namedTypeSymbol.FirstMethodOfSelfOrBaseType(methodName, parameters);
    }

    /// <summary>
    /// Returns true if the methods parameters match those of the type array in order.
    /// </summary>
    /// <param name="method">
    /// The method to check.
    /// </param>
    /// <param name="parameters">
    /// The parameter types to match.
    /// </param>
    /// <returns>
    /// True if the parameters match the types, false otherwise.
    /// </returns>
    public static bool DoParamatersMatch(this IMethodSymbol method, Type[] parameters)
    {
        var methodParamCount = method.Parameters.Length;
        if (methodParamCount != parameters.Length)
            return false;

        for (var i = 0; i < methodParamCount; i++)
        {
            if (!method.Parameters[i].IsType(parameters[i]))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Determines if the INamedTypeSymbol is derived from the specific reflection interface.
    /// </summary>
    /// <param name="namedType">
    /// The type being extended.
    /// </param>
    /// <param name="type">
    /// The reflection type to check.
    /// </param>
    /// <returns>
    /// True if <paramref name="type"/> is in the inheritance chain.
    /// </returns>
    /// <remarks>
    /// This method does no checking if <paramref name="type"/> is actually an interface.
    /// </remarks>
    public static bool IsDerivedFromInterface(this INamedTypeSymbol namedType, Type type)
    {
        foreach (var iFace in namedType.AllInterfaces)
        {
            if (iFace.IsType(type))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if the IParameterSymbol matches the specified reflection type.
    /// </summary>
    /// <param name="parameter">
    /// The type being extended.
    /// </param>
    /// <param name="type">
    /// The reflection type.
    /// </param>
    /// <returns>
    /// True if they match, false otherwise.
    /// </returns>
    public static bool IsType(this IParameterSymbol parameter, Type type)
    {
        return parameter.GetTypesQualifiedAssemblyName().Equals(type.AssemblyQualifiedName, StringComparison.Ordinal);
    }

    /// <summary>
    /// Returns true if the INamedTypeSymbols matches the specified reflection type.
    /// </summary>
    /// <param name="namedType"></param>
    /// The INamedTypeSymbols in question
    /// <param name="type">
    /// The reflection type.
    /// </param>
    /// <returns>
    /// True if they match, false otherwise.
    /// </returns>
    public static bool IsType(this INamedTypeSymbol namedType, Type type)
    {
        return namedType.GetTypesQualifiedAssemblyName().Equals(type.AssemblyQualifiedName, StringComparison.Ordinal);
    }

    /// <summary>
    /// For the INamedTypeSymbol returns the assembly qualified name from Roslyn.
    /// </summary>
    /// <param name="namedType">
    /// The item being extended.
    /// </param>
    /// <returns>
    /// The qualified assembly name this type comes from.
    /// </returns>
    public static string GetTypesQualifiedAssemblyName(this INamedTypeSymbol namedType)
    {
        return BuildQualifiedAssemblyName(null, namedType.ToDisplayString(), namedType.ContainingAssembly);
    }

    /// <summary>
    /// For the IParameterSymbol returns the assembly qualified name from Roslyn.
    /// </summary>
    /// <param name="parameter">
    /// The item being extended.
    /// </param>
    /// <returns>
    /// The qualified assembly name this type comes from.
    /// </returns>
    public static string GetTypesQualifiedAssemblyName(this IParameterSymbol parameter)
    {
        return BuildQualifiedAssemblyName(parameter.Type.ContainingNamespace.Name, parameter.Type.Name, parameter.Type.ContainingAssembly);
    }

    /// <summary>
    /// For the IMethodSymbol returns the assembly qualified name from Roslyn.
    /// </summary>
    /// <param name="parameter">
    /// The item being extended.
    /// </param>
    /// <returns>
    /// The qualified assembly name this type comes from.
    /// </returns>
    public static string GetTypesQualifiedAssemblyName(this IMethodSymbol method)
    {
        return BuildQualifiedAssemblyName(method.ContainingType.ContainingNamespace.Name, method.ContainingType.Name, method.ContainingType.ContainingAssembly);
    }

    private static string BuildQualifiedAssemblyName(string nameSpace, string typeName, IAssemblySymbol assemblySymbol)
    {
        var symbolType = String.IsNullOrEmpty(nameSpace) ? typeName : String.Format("{0}.{1}", nameSpace, typeName);
        var symbolAssemblyQualiedName = symbolType + ", " + new AssemblyName(assemblySymbol.Identity.GetDisplayName(true));
        return symbolAssemblyQualiedName;
    }

    private static bool HasIgnorableAttributes(ImmutableArray<AttributeData> attributes)
    {
        for (var i = 0; i < attributes.Length; i++)
        {
            var attr = attributes[i];
            if ((attr.AttributeClass.IsType(typeof(GeneratedCodeAttribute)) ||
                (attr.AttributeClass.IsType(typeof(DebuggerNonUserCodeAttribute)))))
                return true;
        }

        return false;
    }
}

