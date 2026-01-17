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
/// </summary>
[Mapper]
public partial class UserMapper
{
    /// <summary>
    /// Maps a UserEntity to UserDto.
    /// Demonstrates custom property mapping and transforms.
    /// </summary>
    [MapProperty(nameof(UserEntity.FirstName), nameof(UserDto.FullName), Transform = nameof(FormatFullName))]
    [MapProperty(nameof(UserEntity.DateOfBirth), nameof(UserDto.Age), Transform = nameof(CalculateAge))]
    public partial UserDto MapUser(UserEntity entity);

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
}
