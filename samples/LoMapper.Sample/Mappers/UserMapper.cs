using System.Collections.Generic;
using System.Linq;
using LoMapper.Sample.Dtos;
using LoMapper.Sample.Entities;

namespace LoMapper.Sample.Mappers;

/// <summary>
/// Demonstrates LoMapper features:
/// - Basic property mapping
/// - Custom property renaming with [MapProperty]
/// - Custom transform methods
/// - Nested object mapping
/// - Collection mapping (List to List, List to Array)
/// - Ignored properties with [MapIgnore]
/// - Flattening nested properties with [FlattenProperty]
/// - Lifecycle hooks with [BeforeMap] and [AfterMap]
/// </summary>
[Mapper]
public partial class UserMapper
{
    private readonly List<string> _auditTrail = new();

    public IReadOnlyList<string> AuditTrail => _auditTrail;

    /// <summary>
    /// Maps a UserEntity to UserDto.
    /// Demonstrates custom property mapping and transforms.
    /// </summary>
    [MapProperty(nameof(UserEntity.FirstName), nameof(UserDto.FullName), Transform = nameof(FormatFullName))]
    [MapProperty(nameof(UserEntity.DateOfBirth), nameof(UserDto.Age), Transform = nameof(CalculateAge))]
    public partial UserDto MapUser(UserEntity entity);

    /// <summary>
    /// Same as MapUser but demonstrates lifecycle hooks for validation and audit stamping.
    /// </summary>
    [MapProperty(nameof(UserEntity.FirstName), nameof(UserDto.FullName), Transform = nameof(FormatFullName))]
    [MapProperty(nameof(UserEntity.DateOfBirth), nameof(UserDto.Age), Transform = nameof(CalculateAge))]
    [BeforeMap(nameof(ValidateUser))]
    [AfterMap(nameof(AuditUserMap))]
    public partial UserDto MapUserWithHooks(UserEntity entity);

    /// <summary>
    /// Maps a UserEntity to a flattened UserProfileDto.
    /// Demonstrates flattening nested address properties into the top-level DTO.
    /// The [FlattenProperty] attributes extract PrimaryAddress.City and PrimaryAddress.Country
    /// into separate City and Country properties on the DTO.
    /// </summary>
    [MapProperty(nameof(UserEntity.FirstName), nameof(UserProfileDto.FullName))]
    [FlattenProperty("PrimaryAddress.City", nameof(UserProfileDto.City))]
    [FlattenProperty("PrimaryAddress.Country", nameof(UserProfileDto.Country))]
    public partial UserProfileDto MapUserFlattened(UserEntity entity);

    /// <summary>
    /// Maps an AddressEntity to AddressDto.
    /// Demonstrates custom transforms for combining properties.
    /// </summary>
    [MapProperty(nameof(AddressEntity.City), nameof(AddressDto.FullLocation), Transform = nameof(FormatLocation))]
    public partial AddressDto MapAddress(AddressEntity entity);

    // Transform: Combine first and last name
    // Note: In a real scenario, you'd need access to LastName too.
    // This is a simplified example showing the transform capability.
    private string FormatFullName(string firstName) => firstName;

    // Transform: Calculate age from date of birth
    private int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }

    // Transform: Format full location string
    private string FormatLocation(string city) => city;

    private void ValidateUser(UserEntity entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Email))
        {
            _auditTrail.Add("Validation failed: email is required");
            throw new InvalidOperationException("Email is required before mapping");
        }

        _auditTrail.Add($"Validated {entity.Email}");
    }

    private void AuditUserMap(UserDto dto)
    {
        var tags = dto.Tags ?? Array.Empty<string>();
        dto.Tags = tags.Concat(new[] { "mapped-with-hooks" }).ToArray();
        _auditTrail.Add($"AfterMap hook ran for {dto.Email}");
    }
}
