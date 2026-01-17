using LoMapper.Benchmarks.Models;

namespace LoMapper.Benchmarks.Mappers;

/// <summary>
/// Hand-written manual mapping - the baseline for "optimal" performance.
/// </summary>
public static class ManualMapper
{
    public static UserDto MapUser(UserEntity entity)
    {
        if (entity is null) return null!;

        return new UserDto
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            DateOfBirth = entity.DateOfBirth,
            IsActive = entity.IsActive,
            PrimaryAddress = entity.PrimaryAddress is null ? null : MapAddress(entity.PrimaryAddress),
            Addresses = entity.Addresses?.Select(MapAddress).ToList() ?? new List<AddressDto>(),
            Tags = entity.Tags?.ToList() ?? new List<string>()
        };
    }

    public static AddressDto MapAddress(AddressEntity entity)
    {
        if (entity is null) return null!;

        return new AddressDto
        {
            Id = entity.Id,
            Street = entity.Street,
            City = entity.City,
            State = entity.State,
            ZipCode = entity.ZipCode,
            Country = entity.Country
        };
    }

    public static SimpleDto MapSimple(SimpleEntity entity)
    {
        if (entity is null) return null!;

        return new SimpleDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price,
            Quantity = entity.Quantity,
            CreatedAt = entity.CreatedAt,
            IsAvailable = entity.IsAvailable
        };
    }
}
