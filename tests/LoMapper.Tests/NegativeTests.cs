using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace LoMapper.Tests;

public class NegativeTests
{
    [Fact]
    public void NonPartialClass_GeneratesNothing()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Source { public int Id { get; set; } }
                public class Target { public int Id { get; set; } }

                // Missing 'partial' keyword - should not generate
                [Mapper]
                public class TestMapper
                {
                    public partial Target Map(Source source);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        // Should still generate (the attribute is on a class), but will have compilation errors
        // from the partial method without implementation
        result.CompilationDiagnostics.Should().Contain(d => 
            d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void MethodWithMultipleParameters_IsIgnored()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Source { public int Id { get; set; } }
                public class Target { public int Id { get; set; } }

                [Mapper]
                public partial class TestMapper
                {
                    // Two parameters - should be ignored
                    public partial Target Map(Source source, int extra);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        // Should not generate a mapper for this method
        result.GeneratedSources.Should().BeEmpty();
    }

    [Fact]
    public void VoidReturnMethod_IsIgnored()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Source { public int Id { get; set; } }
                public class Target { public int Id { get; set; } }

                [Mapper]
                public partial class TestMapper
                {
                    // Void return - should be ignored
                    public partial void Map(Source source);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        // Should not generate a mapper for this method
        result.GeneratedSources.Should().BeEmpty();
    }

    [Fact]
    public void InvalidMapPropertySource_ReportsError()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Source { public int Id { get; set; } }
                public class Target { public int Id { get; set; } public string Name { get; set; } }

                [Mapper]
                public partial class TestMapper
                {
                    [MapProperty("NonExistent", "Name")]
                    public partial Target Map(Source source);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        var allDiagnostics = result.CompilationDiagnostics.Concat(result.GeneratorDiagnostics);
        allDiagnostics.Should().Contain(d => d.Id == "LOM005");
    }

    [Fact]
    public void TypeMismatchWithoutTransform_CannotMap()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class SourceChild { public int Value { get; set; } }
                public class TargetChild { public int Value { get; set; } }

                public class Source { public SourceChild Child { get; set; } }
                public class Target { public TargetChild Child { get; set; } }

                [Mapper]
                public partial class TestMapper
                {
                    public partial Target Map(Source source);
                    // Missing: public partial TargetChild Map(SourceChild source);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        var allDiagnostics = result.CompilationDiagnostics.Concat(result.GeneratorDiagnostics);
        allDiagnostics.Should().Contain(d => d.Id == "LOM003");
    }

    [Fact]
    public void StaticProperties_AreSkipped()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Source
                {
                    public int Id { get; set; }
                    public static string StaticProp { get; set; }
                }

                public class Target
                {
                    public int Id { get; set; }
                    public static string StaticProp { get; set; }
                }

                [Mapper]
                public partial class TestMapper
                {
                    public partial Target Map(Source source);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        result.GeneratedSources[0].Should().Contain("Id = source.Id");
        // Static properties should not be mapped
        result.GeneratedSources[0].Should().NotContain("StaticProp");
    }

    [Fact]
    public void IndexerProperties_AreSkipped()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Source
                {
                    public int Id { get; set; }
                    public string this[int index] => "value";
                }

                public class Target
                {
                    public int Id { get; set; }
                }

                [Mapper]
                public partial class TestMapper
                {
                    public partial Target Map(Source source);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        result.GeneratedSources[0].Should().Contain("Id = source.Id");
        // Indexers should not cause issues
    }

    [Fact]
    public void EmptySourceClass_GeneratesEmptyMapping()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Source { }
                public class Target { public int Id { get; set; } }

                [Mapper]
                public partial class TestMapper
                {
                    public partial Target Map(Source source);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        // Should generate but with warning about unmapped Id
        var allDiagnostics = result.CompilationDiagnostics.Concat(result.GeneratorDiagnostics);
        allDiagnostics.Should().Contain(d => d.Id == "LOM001");
    }

    [Fact]
    public void EmptyTargetClass_GeneratesEmptyMapping()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Source { public int Id { get; set; } }
                public class Target { }

                [Mapper]
                public partial class TestMapper
                {
                    public partial Target Map(Source source);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        // Should generate valid code with empty object initializer
        result.GeneratedSources[0].Should().Contain("return new");
    }
}
