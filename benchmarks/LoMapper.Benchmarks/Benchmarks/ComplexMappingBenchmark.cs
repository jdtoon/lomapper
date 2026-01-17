using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Mapster;
using LoMapper.Benchmarks.Mappers;
using LoMapper.Benchmarks.Models;

namespace LoMapper.Benchmarks.Benchmarks;

/// <summary>
/// Benchmarks complex object mapping with nested objects and collections.
/// This tests real-world mapping scenarios.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ComplexMappingBenchmark
{
    private UserEntity _entity = null!;
    private BenchmarkMapper _loMapper = null!;

    [GlobalSetup]
    public void Setup()
    {
        _entity = new UserEntity
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
                new() { Id = 101, Street = "456 Oak Ave", City = "Portland", State = "OR", ZipCode = "97201", Country = "USA" },
                new() { Id = 102, Street = "789 Pine Blvd", City = "San Francisco", State = "CA", ZipCode = "94102", Country = "USA" }
            },
            Tags = new List<string> { "premium", "verified", "newsletter" }
        };

        _loMapper = new BenchmarkMapper();
        MapsterConfig.Configure();
        
        // Warm up AutoMapper
        _ = AutoMapperConfig.Mapper.Map<UserDto>(_entity);
    }

    [Benchmark(Baseline = true)]
    public UserDto Manual()
    {
        return ManualMapper.MapUser(_entity);
    }

    [Benchmark]
    public UserDto LoMapper()
    {
        return _loMapper.MapUser(_entity);
    }

    [Benchmark]
    public UserDto AutoMapper()
    {
        return AutoMapperConfig.Mapper.Map<UserDto>(_entity);
    }

    [Benchmark]
    public UserDto Mapster()
    {
        return _entity.Adapt<UserDto>();
    }
}
