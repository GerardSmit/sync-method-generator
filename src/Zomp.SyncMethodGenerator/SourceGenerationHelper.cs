﻿namespace Zomp.SyncMethodGenerator;

/// <summary>
/// Contains routines to construct a generated source file with a synchronized method.
/// </summary>
public static class SourceGenerationHelper
{
    internal const string CreateSyncVersionAttributeSource = """
// <auto-generated/>
namespace Zomp.SyncMethodGenerator
{
    /// <summary>
    /// An attribute that can be used to automatically generate a synchronous version of an async method. Must be used in a partial class.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method)]
    internal class CreateSyncVersionAttribute : System.Attribute
    {
    }
}
""";

    internal static string GenerateExtensionClass(MethodToGenerate methodToGenerate)
    {
        static string GetKeyword(SyntaxKind sk) => sk switch
        {
            SyntaxKind.PublicKeyword => "public",
            SyntaxKind.InternalKeyword => "internal",
            SyntaxKind.PrivateKeyword => "private",
            SyntaxKind.SealedKeyword => "sealed",
            SyntaxKind.ProtectedKeyword => "protected",
            SyntaxKind.StaticKeyword => "static",
            SyntaxKind.AbstractKeyword => "abstract",
            _ => throw new InvalidOperationException($"{sk} is not supported"),
        };

        // Handle namespaces
        var sbBegin = new StringBuilder();
        var sbEnd = new StringBuilder();
        var i = 0;

        if (!methodToGenerate.IsNamespaceFileScoped)
        {
            foreach (var @namespace in methodToGenerate.Namespaces)
            {
                var indent = new string(' ', 4 * i);
                sbBegin.Append($$"""
{{indent}}namespace {{@namespace}}
{{indent}}{

""");
                sbEnd.Insert(0, $$"""
{{indent}}}

""");
                ++i;
            }
        }

        // Handle classes
        foreach (var @class in methodToGenerate.Classes)
        {
            var indent = new string(' ', 4 * i);

            var modifiers = string.Join(string.Empty, @class.Modifiers.Select(z => GetKeyword((SyntaxKind)z) + " "));
            var classDeclarationLine = $"{modifiers}partial class {@class.ClassName}{(@class.TypeParameterListSyntax.IsEmpty ? string.Empty
                : "<" + string.Join(", ", @class.TypeParameterListSyntax) + ">")}";

            sbBegin.Append($$"""
{{indent}}{{classDeclarationLine}}
{{indent}}{

""");
            sbEnd.Insert(0, $$"""
{{indent}}}

""");
            ++i;
        }

        var beforeNamespace = $"""
// <auto-generated/>{(methodToGenerate.DisableNullable ? string.Empty : """

#nullable enable
""")}
""";

        return methodToGenerate.IsNamespaceFileScoped ? $$"""
{{beforeNamespace}}
namespace {{methodToGenerate.Namespaces.First()}};
{{sbBegin}}{{new string(' ', 4 * i)}}{{methodToGenerate.Implementation.Trim()}}
{{sbEnd}}
"""
            : $$"""
{{beforeNamespace}}
{{sbBegin}}{{new string(' ', 4 * i)}}{{methodToGenerate.Implementation.Trim()}}
{{sbEnd}}
""";
    }
}
