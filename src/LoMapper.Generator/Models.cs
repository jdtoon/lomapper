using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace LoMapper.Generator;

/// <summary>
/// Information about a mapper class marked with [Mapper].
/// </summary>
internal sealed record MapperClassInfo(
    string? Namespace,
    string ClassName,
    Accessibility Accessibility,
    ImmutableArray<MapperMethodInfo> Methods);

/// <summary>
/// Information about a partial mapping method.
/// </summary>
internal sealed record MapperMethodInfo(
    string MethodName,
    Accessibility Accessibility,
    string ParameterName,
    ITypeSymbol SourceType,
    ITypeSymbol TargetType,
    ImmutableArray<PropertyMappingConfig> PropertyMappings,
    ImmutableHashSet<string> IgnoredProperties,
    ImmutableArray<FlatteningConfig> FlattenMappings);

/// <summary>
/// Configuration for a single property mapping from [MapProperty].
/// </summary>
internal sealed record PropertyMappingConfig(
    string SourceProperty,
    string TargetProperty,
    string? Transform);

/// <summary>
/// Configuration for a flattening mapping from [FlattenProperty].
/// </summary>
internal sealed record FlatteningConfig(
    string SourcePropertyPath,
    string TargetProperty);
