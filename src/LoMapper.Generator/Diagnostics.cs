using Microsoft.CodeAnalysis;

namespace LoMapper.Generator;

/// <summary>
/// Diagnostic descriptors for LoMapper compile-time errors and warnings.
/// </summary>
internal static class Diagnostics
{
    private const string Category = "LoMapper";

    /// <summary>
    /// LOM001: Target property has no matching source property (Warning).
    /// </summary>
    public static readonly DiagnosticDescriptor UnmappedTargetProperty = new(
        id: "LOM001",
        title: "Unmapped target property",
        messageFormat: "Target property '{0}' on type '{1}' has no matching source property on type '{2}'. Use [MapProperty] to specify a mapping or [MapIgnore] to suppress this warning.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// LOM002: Property type mismatch (Error).
    /// </summary>
    public static readonly DiagnosticDescriptor TypeMismatch = new(
        id: "LOM002",
        title: "Property type mismatch",
        messageFormat: "Cannot map property '{0}' from type '{1}' to type '{2}'. Types are incompatible and no transform was specified.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// LOM003: Missing nested mapper method (Error).
    /// </summary>
    public static readonly DiagnosticDescriptor MissingNestedMapper = new(
        id: "LOM003",
        title: "Missing nested mapper",
        messageFormat: "Property '{0}' requires mapping from '{1}' to '{2}', but no mapper method was found. Add a partial method: partial {2} Map({1} source);",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// LOM004: Invalid transform method (Error).
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidTransformMethod = new(
        id: "LOM004",
        title: "Invalid transform method",
        messageFormat: "Transform method '{0}' not found or has invalid signature. Expected: {1} {0}({2})",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// LOM005: Source property not found (Error).
    /// </summary>
    public static readonly DiagnosticDescriptor SourcePropertyNotFound = new(
        id: "LOM005",
        title: "Source property not found",
        messageFormat: "Source property '{0}' specified in [MapProperty] was not found on type '{1}'.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// LOM006: Target property not found (Error).
    /// </summary>
    public static readonly DiagnosticDescriptor TargetPropertyNotFound = new(
        id: "LOM006",
        title: "Target property not found",
        messageFormat: "Target property '{0}' specified in [MapProperty] or [MapIgnore] was not found on type '{1}'.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// LOM007: Invalid nested property path in flatten (Error).
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidFlattenPath = new(
        id: "LOM007",
        title: "Invalid flatten property path",
        messageFormat: "Nested property path '{0}' specified in [FlattenProperty] is invalid on type '{1}'. Property '{2}' was not found.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// LOM008: Target property not found in flatten (Error).
    /// </summary>
    public static readonly DiagnosticDescriptor FlattenTargetNotFound = new(
        id: "LOM008",
        title: "Flatten target property not found",
        messageFormat: "Target property '{0}' specified in [FlattenProperty] was not found on type '{1}'.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// LOM009: Type mismatch in flatten mapping (Error).
    /// </summary>
    public static readonly DiagnosticDescriptor FlattenTypeMismatch = new(
        id: "LOM009",
        title: "Flatten type mismatch",
        messageFormat: "Cannot flatten property '{0}' from type '{1}' to type '{2}' specified in [FlattenProperty]. Types are incompatible.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
