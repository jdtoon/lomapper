# Contributing to LoMapper

Thank you for your interest in contributing to LoMapper! We welcome bug reports, feature requests, and pull requests.

## Getting Started

1. **Fork** the repository on GitHub
2. **Clone** your fork locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/lomapper.git
   cd lomapper
   ```
3. **Create a branch** for your feature or fix:
   ```bash
   git checkout -b feature/my-awesome-feature
   ```

## Development Setup

- .NET 8.0 SDK or later
- Visual Studio 2022, VS Code with C# Extensions, or JetBrains Rider

Build and test:
```bash
dotnet build
dotnet test
```

## Code Guidelines

### Style & Conventions
- Use **C# 11+ features** (nullable reference types, init properties, etc.)
- Follow **Microsoft C# Coding Conventions**
- Use `#pragma` directives sparingly and document why
- Aim for **100% nullable reference type compliance** (`#nullable enable`)

### Testing
- **Unit tests** for all new features
- **Edge case tests** for unusual scenarios
- **Negative tests** for error conditions
- Run benchmarks before/after performance-critical changes

```bash
dotnet test
cd benchmarks/LoMapper.Benchmarks
dotnet run -c Release
```

### Commit Messages
- Use clear, descriptive messages
- Reference issues: "Fixes #123" or "Relates to #456"
- Example: `feat: add support for required properties in mappings`

### Documentation
- Update README.md if adding user-facing features
- Add XML doc comments to public APIs
- Include code examples in comments

## Pull Request Process

1. **Sync with main** before submitting:
   ```bash
   git fetch origin
   git rebase origin/main
   ```
2. **Push to your fork** and **create a PR** on GitHub
3. **PR Description** should:
   - Explain the **what** and **why**
   - Reference related issues
   - Include test results or benchmarks
   - Note any breaking changes

4. **CI/CD** must pass (build, tests, analyzer checks)
5. **Code review** — at least one maintainer approval required

## Feature Ideas

We're particularly interested in:
- 🎯 Performance improvements
- 🛡️ Better diagnostics and error messages
- 📚 Additional collection types support (custom, LINQ types)
- 🔄 Flattening/unflattening support
- 🎬 Expression projection support

See [Issues](https://github.com/lomapper/lomapper/issues) for the full roadmap.

## Reporting Bugs

Use GitHub Issues with:
- **Clear title** describing the problem
- **Minimal reproduction** code
- **.NET version** and **OS**
- **Expected vs actual** behavior
- **Error stack trace** if applicable

Example:
```
Title: Nested mappings with null collections crash generator

Environment:
- LoMapper 0.1.0
- .NET 8.0 / Windows 11
- VS 2022

Steps:
1. Create class with nullable collection property
2. Apply [Mapper] attribute
3. Run build

Error: ArgumentNullException in MappingCodeEmitter.cs:123
```

## Questions?

- 💬 Start a GitHub Discussion for questions
- 📧 Reach out to maintainers

Thank you for contributing! 🙏
