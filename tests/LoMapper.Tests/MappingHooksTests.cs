using Xunit;
using System.Linq;

namespace LoMapper.Tests;

public class MappingHooksTests
{
    [Fact]
    public void BeforeMap_CallsMethodBeforeMapping()
    {
        var source = @"
using LoMapper;

namespace Test
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class Target
    {
        public int Value { get; set; }
    }

    [Mapper]
    public partial class TestMapper
    {
        [BeforeMap(nameof(ValidateSource))]
        public partial Target Map(Source source);

        private void ValidateSource(Source source) { }
    }
}";

        var result = GeneratorTestHelper.RunGenerator(source);

        // Should compile without errors
        var errors = result.CompilationDiagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        Assert.False(errors.Any());

        // Generated code should contain the BeforeMap call
        var generatedCode = string.Join("", result.GeneratedSources);
        Assert.Contains("ValidateSource(source);", generatedCode);
    }

    [Fact]
    public void AfterMap_CallsMethodAfterMapping()
    {
        var source = @"
using LoMapper;

namespace Test
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class Target
    {
        public int Value { get; set; }
    }

    [Mapper]
    public partial class TestMapper
    {
        [AfterMap(nameof(MarkProcessed))]
        public partial Target Map(Source source);

        private void MarkProcessed(Target target) { }
    }
}";

        var result = GeneratorTestHelper.RunGenerator(source);

        // Should compile without errors
        var errors = result.CompilationDiagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        Assert.False(errors.Any());

        // Generated code should contain result variable and AfterMap call
        var generatedCode = string.Join("", result.GeneratedSources);
        Assert.Contains("var result = new", generatedCode);
        Assert.Contains("MarkProcessed(result);", generatedCode);
        Assert.Contains("return result;", generatedCode);
    }

    [Fact]
    public void BeforeAndAfterMap_CallsBothHooks()
    {
        var source = @"
using LoMapper;

namespace Test
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class Target
    {
        public int Value { get; set; }
    }

    [Mapper]
    public partial class TestMapper
    {
        [BeforeMap(nameof(BeforeMapping))]
        [AfterMap(nameof(AfterMapping))]
        public partial Target Map(Source source);

        private void BeforeMapping(Source source) { }
        private void AfterMapping(Target target) { }
    }
}";

        var result = GeneratorTestHelper.RunGenerator(source);

        // Should compile without errors
        var errors = result.CompilationDiagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        Assert.False(errors.Any());

        // Generated code should contain both hooks in correct order
        var generatedCode = string.Join("", result.GeneratedSources);
        Assert.Contains("BeforeMapping(source);", generatedCode);
        Assert.Contains("AfterMapping(result);", generatedCode);

        // BeforeMapping should appear before the mapping, AfterMapping after
        var beforeIndex = generatedCode.IndexOf("BeforeMapping(source);");
        var newIndex = generatedCode.IndexOf("new Test.Target");
        if (newIndex < 0) newIndex = generatedCode.IndexOf("new global::Test.Target");
        var afterIndex = generatedCode.IndexOf("AfterMapping(result);");

        Assert.True(beforeIndex >= 0 && newIndex >= 0 && beforeIndex < newIndex, "BeforeMap should be called before object creation");
        Assert.True(newIndex < afterIndex, "AfterMap should be called after object creation");
    }

    [Fact]
    public void AfterMap_WithConstructorMapping_GeneratesCorrectCode()
    {
        var source = @"
using LoMapper;

namespace Test
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; } = """";
    }

    public class Target
    {
        public Target(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }
        public string Name { get; }
    }

    [Mapper]
    public partial class TestMapper
    {
        [AfterMap(nameof(Validate))]
        public partial Target Map(Source source);

        private void Validate(Target target) { }
    }
}";

        var result = GeneratorTestHelper.RunGenerator(source);

        // Should compile without errors
        var errors = result.CompilationDiagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        Assert.False(errors.Any());

        // Generated code should handle AfterMap with constructor correctly
        var generatedCode = string.Join("", result.GeneratedSources);
        Assert.True(generatedCode.Contains("var result = new Test.Target(") || generatedCode.Contains("var result = new global::Test.Target("));
        Assert.Contains("Validate(result);", generatedCode);
        Assert.Contains("return result;", generatedCode);
    }

    [Fact]
    public void MappingHooks_WithoutHooks_GeneratesNormalCode()
    {
        var source = @"
using LoMapper;

namespace Test
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class Target
    {
        public int Value { get; set; }
    }

    [Mapper]
    public partial class TestMapper
    {
        public partial Target Map(Source source);
    }
}";

        var result = GeneratorTestHelper.RunGenerator(source);

        // Should compile without errors
        var errors = result.CompilationDiagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        Assert.False(errors.Any());

        // Generated code should use direct return (not var result)
        var generatedCode = string.Join("", result.GeneratedSources);
        Assert.True(generatedCode.Contains("return new Test.Target") || generatedCode.Contains("return new global::Test.Target"));
        Assert.DoesNotContain("var result =", generatedCode);
    }
}
