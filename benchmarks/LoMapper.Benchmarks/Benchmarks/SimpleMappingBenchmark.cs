using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Mapster;
using LoMapper.Benchmarks.Mappers;
using LoMapper.Benchmarks.Models;

namespace LoMapper.Benchmarks.Benchmarks;

/// <summary>
/// Benchmarks simple flat object mapping (7 properties, no nesting).
/// This tests raw mapping performance without collection/nested object overhead.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class SimpleMappingBenchmark
{
    private SimpleEntity _entity = null!;
    private BenchmarkMapper _loMapper = null!;

    [GlobalSetup]
    public void Setup()
    {
        _entity = new SimpleEntity
        {
            Id = 1,
            Name = "Test Product",
            Description = "A sample product for benchmarking",
            Price = 99.99m,
            Quantity = 100,
            CreatedAt = DateTime.UtcNow,
            IsAvailable = true
        };

        _loMapper = new BenchmarkMapper();
        MapsterConfig.Configure();
        
        // Warm up AutoMapper
        _ = AutoMapperConfig.Mapper.Map<SimpleDto>(_entity);
    }

    [Benchmark(Baseline = true)]
    public SimpleDto Manual()
    {
        return ManualMapper.MapSimple(_entity);
    }

    [Benchmark]
    public SimpleDto LoMapper()
    {
        return _loMapper.MapSimple(_entity);
    }

    [Benchmark]
    public SimpleDto AutoMapper()
    {
        return AutoMapperConfig.Mapper.Map<SimpleDto>(_entity);
    }

    [Benchmark]
    public SimpleDto Mapster()
    {
        return _entity.Adapt<SimpleDto>();
    }
}
