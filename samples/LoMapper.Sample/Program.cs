using LoMapper.Sample.Dtos;
using LoMapper.Sample.Entities;
using LoMapper.Sample.Mappers;

Console.WriteLine("=== LoMapper Sample Application ===\n");

// Create sample data
var userEntity = new UserEntity
{
    Id = 1,
    FirstName = "John",
    LastName = "Doe",
    Email = "john.doe@example.com",
    DateOfBirth = new DateTime(1990, 5, 15),
    IsActive = true,
    PrimaryAddress = new AddressEntity
    {
        Id = 100,
        Street = "123 Main St",
        City = "Seattle",
        State = "WA",
        ZipCode = "98101",
        Country = "USA"
    },
    Addresses = new List<AddressEntity>
    {
        new()
        {
            Id = 101,
            Street = "456 Oak Ave",
            City = "Portland",
            State = "OR",
            ZipCode = "97201",
            Country = "USA"
        },
        new()
        {
            Id = 102,
            Street = "789 Pine Blvd",
            City = "San Francisco",
            State = "CA",
            ZipCode = "94102",
            Country = "USA"
        }
    },
    Tags = new List<string> { "premium", "verified", "newsletter" }
};

// Create mapper instance
var mapper = new UserMapper();

// Map entity to DTO
var userDto = mapper.MapUser(userEntity);

// Display results
Console.WriteLine("Source Entity:");
Console.WriteLine($"  Id: {userEntity.Id}");
Console.WriteLine($"  Name: {userEntity.FirstName} {userEntity.LastName}");
Console.WriteLine($"  Email: {userEntity.Email}");
Console.WriteLine($"  DOB: {userEntity.DateOfBirth:yyyy-MM-dd}");
Console.WriteLine($"  IsActive: {userEntity.IsActive}");
Console.WriteLine($"  Primary Address: {userEntity.PrimaryAddress?.Street}, {userEntity.PrimaryAddress?.City}");
Console.WriteLine($"  Address Count: {userEntity.Addresses.Count}");
Console.WriteLine($"  Tags: {string.Join(", ", userEntity.Tags)}");

Console.WriteLine("\nMapped DTO:");
Console.WriteLine($"  Id: {userDto.Id}");
Console.WriteLine($"  FullName: {userDto.FullName}");
Console.WriteLine($"  Email: {userDto.Email}");
Console.WriteLine($"  Age: {userDto.Age}");
Console.WriteLine($"  IsActive: {userDto.IsActive}");
Console.WriteLine($"  Primary Address: {userDto.PrimaryAddress?.Street}, {userDto.PrimaryAddress?.City}");
Console.WriteLine($"  Address Count: {userDto.Addresses.Count}");
Console.WriteLine($"  Tags: {string.Join(", ", userDto.Tags)}");

Console.WriteLine("\n=== Mapping completed successfully! ===");
Console.WriteLine("\nTip: Check the 'obj/GeneratedFiles' folder to see the generated mapper code.");
