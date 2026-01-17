namespace LoMapper.Sample.Dtos;

/// <summary>
/// Data transfer object for user information.
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public AddressDto? PrimaryAddress { get; set; }
    public List<AddressDto> Addresses { get; set; } = new();
    public string[] Tags { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Data transfer object for address information.
/// </summary>
public class AddressDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string FullLocation { get; set; } = string.Empty;
}
