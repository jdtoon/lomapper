using System;

namespace LoMapper;

/// <summary>
/// Marks a partial class as a mapper. The source generator will implement
/// all partial methods defined in the class with mapping logic.
/// </summary>
/// <example>
/// <code>
/// [Mapper]
/// public partial class UserMapper
/// {
///     public partial UserDto Map(UserEntity entity);
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class MapperAttribute : Attribute
{
}
