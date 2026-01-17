using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using LoMapper.Benchmarks.Benchmarks;

// Run all benchmarks
var config = DefaultConfig.Instance
    .WithOptions(ConfigOptions.JoinSummary);

Console.WriteLine("=== LoMapper Benchmark Suite ===");
Console.WriteLine();
Console.WriteLine("Comparing: LoMapper vs AutoMapper vs Mapster vs Manual");
Console.WriteLine();

#if DEBUG
Console.WriteLine("WARNING: Running in DEBUG mode. Results will not be accurate.");
Console.WriteLine("Please run in RELEASE mode: dotnet run -c Release");
Console.WriteLine();
#endif

var summary = BenchmarkSwitcher
    .FromAssembly(typeof(Program).Assembly)
    .Run(args, config);

// If no args provided, run all benchmarks
if (args.Length == 0)
{
    Console.WriteLine();
    Console.WriteLine("To run specific benchmarks:");
    Console.WriteLine("  dotnet run -c Release -- --filter *Simple*");
    Console.WriteLine("  dotnet run -c Release -- --filter *Complex*");
    Console.WriteLine("  dotnet run -c Release -- --filter *Bulk*");
}
