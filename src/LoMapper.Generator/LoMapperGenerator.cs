using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace LoMapper.Generator;

/// <summary>
/// Roslyn incremental source generator for LoMapper.
/// Generates mapping method implementations for classes marked with [Mapper].
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class LoMapperGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the incremental generator by setting up syntax and semantic analysis pipelines.
    /// </summary>
    /// <param name="context">The initialization context provided by Roslyn.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all partial classes with [Mapper] attribute
        var mapperClasses = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "LoMapper.MapperAttribute",
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, ct) => GetMapperInfo(ctx, ct))
            .Where(static m => m is not null)!;

        // Combine with compilation for type resolution
        var compilationAndMappers = context.CompilationProvider
            .Combine(mapperClasses.Collect());

        // Generate source
        context.RegisterSourceOutput(compilationAndMappers, static (spc, source) =>
        {
            var (compilation, mappers) = source;
            Execute(compilation, mappers, spc);
        });
    }

    private static MapperClassInfo? GetMapperInfo(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken)
    {
        if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
            return null;

        var classSyntax = (ClassDeclarationSyntax)context.TargetNode;

        // Get all partial methods in the class
        var methods = new List<MapperMethodInfo>();
        foreach (var member in classSymbol.GetMembers())
        {
            if (member is IMethodSymbol method &&
                method.IsPartialDefinition &&
                method.Parameters.Length == 1 &&
                !method.ReturnsVoid)
            {
                var methodInfo = GetMethodInfo(method, classSyntax, context.SemanticModel);
                if (methodInfo is not null)
                {
                    methods.Add(methodInfo);
                }
            }
        }

        if (methods.Count == 0)
            return null;

        return new MapperClassInfo(
            classSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : classSymbol.ContainingNamespace.ToDisplayString(),
            classSymbol.Name,
            classSymbol.DeclaredAccessibility,
            methods.ToImmutableArray());
    }

    private static MapperMethodInfo? GetMethodInfo(
        IMethodSymbol method,
        ClassDeclarationSyntax classSyntax,
        SemanticModel semanticModel)
    {
        var sourceType = method.Parameters[0].Type;
        var targetType = method.ReturnType;

        // Get [MapProperty], [MapIgnore], [FlattenProperty], [BeforeMap], and [AfterMap] attributes
        var propertyMappings = new List<PropertyMappingConfig>();
        var ignoredProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var flattenMappings = new List<FlatteningConfig>();
        string? beforeMapMethod = null;
        string? afterMapMethod = null;

        foreach (var attr in method.GetAttributes())
        {
            var attrName = attr.AttributeClass?.ToDisplayString();

            if (attrName == "LoMapper.MapPropertyAttribute" &&
                attr.ConstructorArguments.Length >= 2)
            {
                var sourceProp = attr.ConstructorArguments[0].Value as string;
                var targetProp = attr.ConstructorArguments[1].Value as string;
                string? transform = null;

                foreach (var namedArg in attr.NamedArguments)
                {
                    if (namedArg.Key == "Transform")
                    {
                        transform = namedArg.Value.Value as string;
                    }
                }

                if (!string.IsNullOrEmpty(sourceProp) && !string.IsNullOrEmpty(targetProp))
                {
                    propertyMappings.Add(new PropertyMappingConfig(sourceProp!, targetProp!, transform));
                }
            }
            else if (attrName == "LoMapper.MapIgnoreAttribute" &&
                     attr.ConstructorArguments.Length >= 1)
            {
                var targetProp = attr.ConstructorArguments[0].Value as string;
                if (!string.IsNullOrEmpty(targetProp))
                {
                    ignoredProperties.Add(targetProp!);
                }
            }
            else if (attrName == "LoMapper.FlattenPropertyAttribute" &&
                     attr.ConstructorArguments.Length >= 2)
            {
                var sourcePath = attr.ConstructorArguments[0].Value as string;
                var targetProp = attr.ConstructorArguments[1].Value as string;

                if (!string.IsNullOrEmpty(sourcePath) && !string.IsNullOrEmpty(targetProp))
                {
                    flattenMappings.Add(new FlatteningConfig(sourcePath!, targetProp!));
                }
            }
            else if (attrName == "LoMapper.BeforeMapAttribute" &&
                     attr.ConstructorArguments.Length >= 1)
            {
                beforeMapMethod = attr.ConstructorArguments[0].Value as string;
            }
            else if (attrName == "LoMapper.AfterMapAttribute" &&
                     attr.ConstructorArguments.Length >= 1)
            {
                afterMapMethod = attr.ConstructorArguments[0].Value as string;
            }
        }

        return new MapperMethodInfo(
            method.Name,
            method.DeclaredAccessibility,
            method.Parameters[0].Name,
            sourceType,
            targetType,
            propertyMappings.ToImmutableArray(),
            ignoredProperties.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase),
            flattenMappings.ToImmutableArray(),
            beforeMapMethod,
            afterMapMethod);
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<MapperClassInfo?> mappers,
        SourceProductionContext context)
    {
        foreach (var mapper in mappers)
        {
            if (mapper is null) continue;

            // Build a map of all mapper methods for circular reference detection
            var mapperMethodLookup = mapper.Methods.ToDictionary(
                m => (m.SourceType, m.TargetType),
                m => m,
                new TypePairComparer());

            // Check for circular references in mapper methods
            foreach (var method in mapper.Methods)
            {
                var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
                var path = new List<string>();
                DetectCircularReferences(method.TargetType, visited, path, context, mapperMethodLookup);
            }

            var emitter = new MappingCodeEmitter(compilation, context, mapper);
            var source = emitter.Emit();

            if (!string.IsNullOrEmpty(source))
            {
                context.AddSource($"{mapper.ClassName}.g.cs", source);
            }
        }
    }

    private static void DetectCircularReferences(
        ITypeSymbol targetType,
        HashSet<ITypeSymbol> visited,
        List<string> path,
        SourceProductionContext context,
        Dictionary<(ITypeSymbol, ITypeSymbol), MapperMethodInfo> mapperMethodLookup)
    {
        // Check if we've already visited this type (indicates a cycle)
        if (visited.Contains(targetType))
        {
            var cyclePath = string.Join(" → ", path) + $" → {targetType.Name}";
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.CircularReference,
                Location.None,
                cyclePath));
            return;
        }

        visited.Add(targetType);
        path.Add(targetType.Name);

        // Get the mapper method for this target type to access ignored properties
        var currentMapper = GetMapperForTargetType(targetType, mapperMethodLookup);

        // Get properties of target type
        var properties = targetType.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.GetMethod != null && !p.IsStatic);

        foreach (var targetProp in properties)
        {
            // Skip if this property is ignored in the current mapper
            if (currentMapper != null && currentMapper.IgnoredProperties.Contains(targetProp.Name))
            {
                continue;
            }

            var propType = targetProp.Type;

            // Skip built-in types
            if (IsBuiltInType(propType))
                continue;

            // For collections, check the element type
            if (IsCollectionType(propType))
            {
                var elementType = GetCollectionElementType(propType);
                if (elementType != null && !IsBuiltInType(elementType))
                {
                    // Check if there's a mapper for this element type
                    if (HasMapperForType(elementType, mapperMethodLookup))
                    {
                        DetectCircularReferences(elementType, new HashSet<ITypeSymbol>(visited, SymbolEqualityComparer.Default), new List<string>(path), context, mapperMethodLookup);
                    }
                }
                continue;
            }

            // Check if there's a mapper method for this property type
            // If not, we don't need to check for cycles since it won't be mapped
            if (HasMapperForType(propType, mapperMethodLookup))
            {
                DetectCircularReferences(propType, new HashSet<ITypeSymbol>(visited, SymbolEqualityComparer.Default), new List<string>(path), context, mapperMethodLookup);
            }
        }

        path.RemoveAt(path.Count - 1);
        visited.Remove(targetType);
    }

    private static bool HasMapperForType(ITypeSymbol type, Dictionary<(ITypeSymbol, ITypeSymbol), MapperMethodInfo> mapperMethodLookup)
    {
        return mapperMethodLookup.Keys.Any(k => SymbolEqualityComparer.Default.Equals(k.Item2, type));
    }

    private static MapperMethodInfo? GetMapperForTargetType(ITypeSymbol targetType, Dictionary<(ITypeSymbol, ITypeSymbol), MapperMethodInfo> mapperMethodLookup)
    {
        var pair = mapperMethodLookup.Keys.FirstOrDefault(k => SymbolEqualityComparer.Default.Equals(k.Item2, targetType));
        return pair != default ? mapperMethodLookup[pair] : null;
    }

    private class TypePairComparer : IEqualityComparer<(ITypeSymbol, ITypeSymbol)>
    {
        public bool Equals((ITypeSymbol, ITypeSymbol) x, (ITypeSymbol, ITypeSymbol) y)
        {
            return SymbolEqualityComparer.Default.Equals(x.Item1, y.Item1) &&
                   SymbolEqualityComparer.Default.Equals(x.Item2, y.Item2);
        }

        public int GetHashCode((ITypeSymbol, ITypeSymbol) obj)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + SymbolEqualityComparer.Default.GetHashCode(obj.Item1);
                hash = hash * 31 + SymbolEqualityComparer.Default.GetHashCode(obj.Item2);
                return hash;
            }
        }
    }

    private static bool IsBuiltInType(ITypeSymbol type)
    {
        var fullName = type.ToDisplayString();
        return type.SpecialType != SpecialType.None ||
               type.TypeKind == TypeKind.Enum ||
               fullName.StartsWith("System.") && (
                   fullName == "System.String" ||
                   fullName == "System.DateTime" ||
                   fullName == "System.DateTimeOffset" ||
                   fullName == "System.TimeSpan" ||
                   fullName == "System.Guid" ||
                   fullName == "System.Decimal" ||
                   fullName.StartsWith("System.Nullable<"));
    }

    private static bool IsCollectionType(ITypeSymbol type)
    {
        var fullName = type.ToDisplayString();
        return fullName.Contains("System.Collections") ||
               fullName.StartsWith("System.Collections.Generic.List<") ||
               fullName.StartsWith("System.Collections.Generic.IEnumerable<") ||
               fullName.StartsWith("System.Collections.Generic.ICollection<") ||
               fullName.StartsWith("System.Collections.Generic.HashSet<") ||
               fullName.StartsWith("System.Collections.Generic.Dictionary<") ||
               type is IArrayTypeSymbol;
    }

    private static ITypeSymbol? GetCollectionElementType(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol arrayType)
            return arrayType.ElementType;

        if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            var typeArgs = namedType.TypeArguments;
            if (typeArgs.Length > 0)
                return typeArgs[0];
        }

        return null;
    }
}
