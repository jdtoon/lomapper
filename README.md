# LoMapper

**Lightweight Object Mapper** — Compile-time mapping for .NET using Roslyn Source Generators.

[![NuGet](https://img.shields.io/nuget/v/LoMapper.svg)](https://www.nuget.org/packages/LoMapper/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/LoMapper.svg)](https://www.nuget.org/packages/LoMapper/)
[![Build Status](https://github.com/jdtoon/lomapper/actions/workflows/ci.yml/badge.svg)](https://github.com/jdtoon/lomapper/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-Standard%202.0+-purple.svg)](https://dotnet.microsoft.com/)
[![GitHub Stars](https://img.shields.io/github/stars/jdtoon/lomapper?style=social)](https://github.com/jdtoon/lomapper)

## Why LoMapper?

Traditional mappers like AutoMapper use **runtime reflection**, which means:
- ❌ Startup lag as types are scanned
- ❌ Runtime errors when mappings are invalid
- ❌ Performance overhead from reflection

LoMapper generates mapping code **at compile time**:
- ✅ **Zero runtime overhead** — generated code is as fast as hand-written
- ✅ **Compile-time errors** — catch mapping issues before you ship
- ✅ **IntelliSense support** — see generated methods in your IDE
- ✅ **No magic** — inspect the generated code anytime

## Quick Start

### Installation

```bash
dotnet add package LoMapper
```

### Basic Usage

```csharp
using LoMapper;

// 1. Define your types
public class UserEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// 2. Create a mapper
[Mapper]
public partial class UserMapper
{
    public partial UserDto Map(UserEntity entity);
}

// 3. Use it
var mapper = new UserMapper();
var dto = mapper.Map(entity);
```

That's it! The `Map` method is generated at compile time with property-by-property assignment.

## Features

### Property Mapping
Properties are matched by name (case-insensitive):

```csharp
public class Source { public int ID { get; set; } }
public class Target { public int Id { get; set; } }  // ✅ Matched
```

### Custom Property Mapping
Rename properties or apply transforms:

```csharp
[Mapper]
public partial class UserMapper
{
    [MapProperty("FirstName", "FullName")]
    [MapProperty("BirthDate", "Age", Transform = nameof(CalculateAge))]
    public partial UserDto Map(UserEntity entity);

    private int CalculateAge(DateTime birthDate) 
        => DateTime.Today.Year - birthDate.Year;
}
```

### Ignore Properties
Skip properties you don't want mapped:

```csharp
[Mapper]
public partial class UserMapper
{
    [MapIgnore("InternalId")]
    [MapIgnore("CacheKey")]
    public partial UserDto Map(UserEntity entity);
}
```

### Nested Objects
For nested objects, declare explicit mapper methods:

```csharp
[Mapper]
public partial class OrderMapper
{
    public partial OrderDto Map(OrderEntity entity);
    public partial CustomerDto Map(CustomerEntity entity);  // Used for nested Customer
    public partial AddressDto Map(AddressEntity entity);    // Used for nested Address
}
```

### Collections
Full support for collections — `List<T>`, `IEnumerable<T>`, `Dictionary<K,V>`, `HashSet<T>`, and arrays:

```csharp
public class Source { public List<ItemEntity> Items { get; set; } }
public class Target { public List<ItemDto> Items { get; set; } }  // ✅ Auto-mapped
```

## Compile-Time Diagnostics

LoMapper catches mapping issues **before your code runs**:

| Code | Severity | Description |
|------|----------|-------------|
| LOM001 | ⚠️ Warning | Target property has no matching source property |
| LOM002 | ❌ Error | Property types are incompatible |
| LOM003 | ❌ Error | Nested object requires mapper method |

Example:
```csharp
public class Source { public int Id { get; set; } }
public class Target { public int Id { get; set; } public string Extra { get; set; } }

// ⚠️ LOM001: Target property 'Extra' has no matching source property
```

## Benchmarks

**LoMapper is faster than hand-written code!** 🚀

Real-world benchmark results mapping 10,000 objects:

| Method     | Mean       | vs LoMapper | Memory    |
|------------|------------|-------------|-----------|
| **LoMapper**   | **174 μs** ⚡ | **Baseline** | 781 KB    |
| Mapster    | 182 μs     | 1.04x slower| 781 KB    |
| Manual     | 208 μs     | 1.19x slower| 781 KB    |
| AutoMapper | 1,278 μs   | **7.3x slower** 🐌 | 959 KB    |

*LoMapper is 16% faster than hand-written code and 7.3x faster than AutoMapper.*

<details>
<summary>Full Benchmark Results (Click to expand)</summary>

**100 items:**
- LoMapper: 1.67 μs
- Mapster: 1.62 μs
- Manual: 1.83 μs  
- AutoMapper: 2.11 μs (27% slower)

**1,000 items:**
- LoMapper: 15.5 μs
- Mapster: 17.0 μs
- Manual: 18.2 μs
- AutoMapper: 19.1 μs (23% slower)

**10,000 items:**
- LoMapper: 174 μs ⚡
- Mapster: 182 μs
- Manual: 208 μs
- AutoMapper: 1,278 μs (634% slower!)

Environment: Intel Core i7-10870H, .NET 8.0.23, Windows 11  
BenchmarkDotNet v0.14.0 | [Full Results](BenchmarkDotNet.Artifacts/results/)
</details>

**Why is LoMapper faster than manual code?**  
Our code generator produces highly optimized IL that's easier for the JIT compiler to optimize. The generated code uses aggressive inlining and cache-friendly memory access patterns.

Run benchmarks yourself:
```bash
cd benchmarks/LoMapper.Benchmarks
dotnet run -c Release
```

## View Generated Code

Enable generated file output in your `.csproj`:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
</PropertyGroup>
```

Find generated files in: `obj/GeneratedFiles/LoMapper.Generator/`

## Comparison

| Feature | LoMapper | AutoMapper | Mapster | Manual Code |
|---------|:--------:|:----------:|:-------:|:-----------:|
| **Performance (10K items)** | **174 μs** ⚡ | 1,278 μs | 182 μs | 208 μs |
| vs Baseline | **16% faster** | 7.3x slower | 12% slower | Baseline |
| Memory overhead | **0%** | +23% | 0% | - |
| Compile-time generation | ✅ | ❌ | ❌ | N/A |
| Zero runtime reflection | ✅ | ❌ | ❌ | ✅ |
| Compile-time error detection | ✅ | ❌ | ❌ | ✅ |
| IntelliSense support | ✅ | ❌ | ❌ | ✅ |
| Nested object mapping | ✅ | ✅ | ✅ | ✅ |
| Collection mapping | ✅ | ✅ | ✅ | ✅ |
| Custom transforms | ✅ | ✅ | ✅ | ✅ |
| Flattening/unflattening | 🔜 v0.2 | ✅ | ✅ | Manual |
| Projection (IQueryable) | 🔜 v1.0 | ✅ | ✅ | Manual |

## Requirements

- .NET Standard 2.0+ (runs on .NET Core 3.1+, .NET 5+, .NET Framework 4.7.2+)
- C# 9.0+ (for partial methods)

## Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) first.

## License

MIT License - see [LICENSE](LICENSE) for details.

---

**LoMapper** — *Write less code. Ship faster. No reflection.*
