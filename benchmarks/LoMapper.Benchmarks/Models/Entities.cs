namespace LoMapper.Benchmarks.Models;

/// <summary>
/// Source entity for benchmarking - simulates a database entity.
/// </summary>
public class UserEntity
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public AddressEntity? PrimaryAddress { get; set; }
    public List<AddressEntity> Addresses { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}

public class AddressEntity
{
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

/// <summary>
/// Simple flat entity for basic mapping benchmarks.
/// </summary>
public class SimpleEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsAvailable { get; set; }
}
