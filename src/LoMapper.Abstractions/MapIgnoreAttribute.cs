using System;

namespace LoMapper;

/// <summary>
/// Specifies that a target property should be ignored during mapping.
/// Apply to the partial mapper method to exclude specific properties.
/// </summary>
/// <example>
/// <code>
/// [Mapper]
/// public partial class UserMapper
/// {
///     [MapIgnore(nameof(UserDto.InternalId))]
///     [MapIgnore(nameof(UserDto.CachedValue))]
///     public partial UserDto Map(UserEntity entity);
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class MapIgnoreAttribute : Attribute
{
    /// <summary>
    /// The name of the target property to ignore.
    /// </summary>
    public string TargetProperty { get; }

    /// <summary>
    /// Creates an ignore configuration for a target property.
    /// </summary>
    /// <param name="targetProperty">The name of the target property to ignore (use nameof()).</param>
    public MapIgnoreAttribute(string targetProperty)
    {
        TargetProperty = targetProperty ?? throw new ArgumentNullException(nameof(targetProperty));
    }
}
