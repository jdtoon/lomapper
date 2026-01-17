using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace LoMapper.Tests;

public class DiagnosticTests
{
    [Fact]
    public void UnmappedTargetProperty_ReportsWarning()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Source
                {
                    public int Id { get; set; }
                }

                public class Target
                {
                    public int Id { get; set; }
                    public string ExtraProperty { get; set; }
                }

                [Mapper]
                public partial class TestMapper
                {
                    public partial Target Map(Source source);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        // Check both compilation diagnostics and generator diagnostics
        var allDiagnostics = result.CompilationDiagnostics.Concat(result.GeneratorDiagnostics);
        allDiagnostics.Should().Contain(d => d.Id == "LOM001" && d.Severity == DiagnosticSeverity.Warning);
    }

    [Fact]
    public void MissingNestedMapper_ReportsError()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class NestedSource { public string Value { get; set; } }
                public class NestedTarget { public string Value { get; set; } }

                public class Source
                {
                    public NestedSource Child { get; set; }
                }

                public class Target
                {
                    public NestedTarget Child { get; set; }
                }

                [Mapper]
                public partial class TestMapper
                {
                    public partial Target Map(Source source);
                    // Missing: public partial NestedTarget Map(NestedSource source);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        // Check both compilation diagnostics and generator diagnostics
        var allDiagnostics = result.CompilationDiagnostics.Concat(result.GeneratorDiagnostics);
        allDiagnostics.Should().Contain(d => d.Id == "LOM003" && d.Severity == DiagnosticSeverity.Error);
    }
}
