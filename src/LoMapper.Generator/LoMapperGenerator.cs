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

        // Get [MapProperty] attributes
        var propertyMappings = new List<PropertyMappingConfig>();
        var ignoredProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
        }

        return new MapperMethodInfo(
            method.Name,
            method.DeclaredAccessibility,
            method.Parameters[0].Name,
            sourceType,
            targetType,
            propertyMappings.ToImmutableArray(),
            ignoredProperties.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase));
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<MapperClassInfo?> mappers,
        SourceProductionContext context)
    {
        foreach (var mapper in mappers)
        {
            if (mapper is null) continue;

            var emitter = new MappingCodeEmitter(compilation, context, mapper);
            var source = emitter.Emit();

            if (!string.IsNullOrEmpty(source))
            {
                context.AddSource($"{mapper.ClassName}.g.cs", source);
            }
        }
    }
}
