using System;

namespace LoMapper;

/// <summary>
/// Configures custom mapping for a property. Apply to the partial mapper method
/// to rename properties or specify a custom transform method.
/// </summary>
/// <example>
/// <code>
/// [Mapper]
/// public partial class UserMapper
/// {
///     [MapProperty(nameof(UserEntity.FirstName), nameof(UserDto.Name))]
///     [MapProperty(nameof(UserEntity.BirthDate), nameof(UserDto.Age), Transform = nameof(CalculateAge))]
///     public partial UserDto Map(UserEntity entity);
///     
///     private int CalculateAge(DateTime birthDate) => /* ... */;
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class MapPropertyAttribute : Attribute
{
    /// <summary>
    /// The name of the source property.
    /// </summary>
    public string SourceProperty { get; }

    /// <summary>
    /// The name of the target property.
    /// </summary>
    public string TargetProperty { get; }

    /// <summary>
    /// Optional name of a method in the mapper class to transform the value.
    /// The method must accept the source property type and return the target property type.
    /// </summary>
    public string? Transform { get; set; }

    /// <summary>
    /// Creates a property mapping configuration.
    /// </summary>
    /// <param name="sourceProperty">The name of the source property (use nameof()).</param>
    /// <param name="targetProperty">The name of the target property (use nameof()).</param>
    public MapPropertyAttribute(string sourceProperty, string targetProperty)
    {
        SourceProperty = sourceProperty ?? throw new ArgumentNullException(nameof(sourceProperty));
        TargetProperty = targetProperty ?? throw new ArgumentNullException(nameof(targetProperty));
    }
}
