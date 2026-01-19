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

## [0.4.0] - 2026-01-19

### Added
- **Lifecycle hooks** with `[BeforeMap]` and `[AfterMap]` attributes, including constructor-aware code generation and ordering guarantees (before mapping/after mapping)
- **Circular reference detection** diagnostic LOM010 to prevent cyclic mapper graphs at build time
- **Samples** updated with hook-driven mapper (`MapUserWithHooks`) and audit trail output
- **Documentation** refreshed for hooks, circular detection, and diagnostics table updates
 - **Benchmarks** re-run and published for bulk mapping scenarios

### Changed
- Improved constructor parameter matching for hook-enabled mappers
- Tightened diagnostics coverage and tests around new features
 - Documentation and sample text refined to reflect hook/circular features

### Tests
- Expanded hook and circular-detection test suites (51 tests total)

---

## [0.3.0] - 2026-01-19

### Added
- **Flattening Support**
  - New `[FlattenProperty]` attribute for mapping nested properties to flat properties
  - Deep nesting support: Traverse nested object chains (e.g., `Address.City.Country.Code`)
  - Null-safe code generation with `?.` operator
  - Type-compatible source and target properties with compile-time validation
  - Combine flattening with `[MapProperty]` and `[MapIgnore]`
  - Works with both reference and value types

- **New Diagnostics**
  - LOM007: Invalid flatten property path (property doesn't exist)
  - LOM008: Flatten target property not found on type
  - LOM009: Type mismatch in flatten mapping (incompatible types)

- **Tests**
  - 10 comprehensive flatten tests covering all scenarios

### Changed
- Updated README with flatten feature documentation and examples
- Updated feature comparison table: flatten now marked as supported in v0.3
- Updated roadmap to reflect completion of flatten feature
 - Sample mapper and docs aligned with PrimaryAddress usage and FullName mapping

---

## [0.2.0] - 2026-01-18

### Changed
- Release workflow updates (publish NuGet packages directly to GitHub releases and adjust triggers)
- Documentation refresh for performance comparison
- Version bump to 0.1.1 in project files for pipeline alignment

### Fixed
- CI/release pipeline configuration and branch references

### Notes
- Feature work originally planned for 0.2.0 was deferred and shipped in later versions (flattening in 0.3.0; lifecycle hooks and circular detection in 0.4.0).

---

## Future Roadmap

### 0.5.0
- [ ] Support for custom collection types
- [ ] Better error messages for complex scenarios

### 1.0.0
- [ ] Expression projection (IQueryable)
- [ ] LINQ-to-SQL integration
- [ ] Stable API surface
