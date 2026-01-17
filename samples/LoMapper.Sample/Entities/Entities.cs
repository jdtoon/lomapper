namespace LoMapper.Sample.Entities;

/// <summary>
/// Database entity representing a user.
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

/// <summary>
/// Database entity representing an address.
/// </summary>
public class AddressEntity
{
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
