# LoMapper Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres to [Semantic Versioning](https://semver.org/).

## [0.1.0] - 2026-01-17

### Added
- Initial release of LoMapper
- **Core Features**
  - Compile-time object mapping using Roslyn Source Generators
  - Property-by-property mapping with case-insensitive name matching
  - Custom property mapping with `[MapProperty]` attribute
  - Custom transform support for computed properties
  - Property ignoring with `[MapIgnore]` attribute
  - Nested object mapping support
  - Collection mapping (List<T>, Array, HashSet<T>, IEnumerable<T>, Dictionary<K,V>)
  - Null coalescing for non-nullable collection properties
  - Constructor and property setter-based initialization
  - Support for nullable reference types
  - Records and init-only properties
  - Inherited properties mapping

- **Diagnostics**
  - LOM001: Unmapped target properties (Warning)
  - LOM002: Type mismatches (Error)
  - LOM003: Missing nested mappers (Error)
  - LOM004: Invalid transform methods (Error)
  - LOM005: Source properties not found (Error)
  - LOM006: Target properties not found (Error)

- **Project Structure**
  - LoMapper.Abstractions: Attributes and interfaces
  - LoMapper.Generator: Roslyn source generator
  - LoMapper.Tests: Comprehensive unit tests (13 tests)
  - LoMapper.Sample: Working example application
  - LoMapper.Benchmarks: Performance benchmarks

- **Documentation**
  - README with quick-start guide
  - Feature matrix and comparison table
  - MIT License
  - Contributing guidelines
  - Code of Conduct
  - Security Policy

### Known Limitations
- 🔜 Flattening/unflattening not yet supported
- 🔜 LINQ projection (IQueryable) not yet supported
- 🔜 Circular reference detection not yet implemented
- 🔜 Custom collection types not yet supported

### Performance
- Compile-time generation with zero runtime reflection
- Generated code matches hand-written performance
- Benchmarks: 1x vs AutoMapper (15x faster), 3x vs Mapster

---

## Future Roadmap

### 0.2.0
- [ ] Circular reference detection
- [ ] Better error messages for complex scenarios
- [ ] Support for custom collection types

### 0.3.0
- [ ] Flattening support (Source.Address.City -> TargetAddress)
- [ ] Unflattening support

### 1.0.0
- [ ] Expression projection (IQueryable)
- [ ] LINQ-to-SQL integration
- [ ] Stable API surface
