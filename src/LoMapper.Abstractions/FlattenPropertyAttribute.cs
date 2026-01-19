using System;

namespace LoMapper;

/// <summary>
/// Configures flattening of a nested property into a flat target property.
/// Apply to the partial mapper method to map nested object properties to flat properties.
/// </summary>
/// <example>
/// <code>
/// [Mapper]
/// public partial class UserMapper
/// {
///     [FlattenProperty(nameof(UserEntity.Address.City), nameof(UserDto.AddressCity))]
///     [FlattenProperty(nameof(UserEntity.Address.ZipCode), nameof(UserDto.AddressZipCode))]
///     public partial UserDto Map(UserEntity entity);
/// }
/// </code>
/// </example>
/// <remarks>
/// The source property path can use dot notation to traverse nested objects (e.g., "Address.City").
/// The target property must be a flat property on the target type.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class FlattenPropertyAttribute : Attribute
{
    /// <summary>
    /// The path to the source property, optionally using dot notation for nested access (e.g., "Address.City").
    /// </summary>
    public string SourcePropertyPath { get; }

    /// <summary>
    /// The name of the target flat property.
    /// </summary>
    public string TargetProperty { get; }

    /// <summary>
    /// Creates a flatten mapping configuration.
    /// </summary>
    /// <param name="sourcePropertyPath">The path to the source property (use nameof() or dot-separated path like "Address.City").</param>
    /// <param name="targetProperty">The name of the target flat property (use nameof()).</param>
    public FlattenPropertyAttribute(string sourcePropertyPath, string targetProperty)
    {
        SourcePropertyPath = sourcePropertyPath ?? throw new ArgumentNullException(nameof(sourcePropertyPath));
        TargetProperty = targetProperty ?? throw new ArgumentNullException(nameof(targetProperty));
    }
}
