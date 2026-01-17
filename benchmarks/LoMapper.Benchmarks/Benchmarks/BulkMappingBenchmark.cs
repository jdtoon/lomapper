using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Mapster;
using LoMapper.Benchmarks.Mappers;
using LoMapper.Benchmarks.Models;

namespace LoMapper.Benchmarks.Benchmarks;

/// <summary>
/// Benchmarks mapping large collections of objects.
/// This tests throughput and memory allocation patterns.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class BulkMappingBenchmark
{
    private List<SimpleEntity> _entities = null!;
    private BenchmarkMapper _loMapper = null!;

    [Params(100, 1000, 10000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _entities = Enumerable.Range(1, Count)
            .Select(i => new SimpleEntity
            {
                Id = i,
                Name = $"Product {i}",
                Description = $"Description for product {i}",
                Price = 10.00m + i,
                Quantity = i * 10,
                CreatedAt = DateTime.UtcNow.AddDays(-i),
                IsAvailable = i % 2 == 0
            })
            .ToList();

        _loMapper = new BenchmarkMapper();
        MapsterConfig.Configure();
        
        // Warm up AutoMapper
        _ = AutoMapperConfig.Mapper.Map<List<SimpleDto>>(_entities);
    }

    [Benchmark(Baseline = true)]
    public List<SimpleDto> Manual()
    {
        return _entities.Select(ManualMapper.MapSimple).ToList();
    }

    [Benchmark]
    public List<SimpleDto> LoMapper()
    {
        return _entities.Select(_loMapper.MapSimple).ToList();
    }

    [Benchmark]
    public List<SimpleDto> AutoMapper()
    {
        return AutoMapperConfig.Mapper.Map<List<SimpleDto>>(_entities);
    }

    [Benchmark]
    public List<SimpleDto> Mapster()
    {
        return _entities.Adapt<List<SimpleDto>>();
    }
}
