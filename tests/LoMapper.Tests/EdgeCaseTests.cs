using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace LoMapper.Tests;

public class EdgeCaseTests
{
    [Fact]
    public void RecordTypes_AreMappedCorrectly()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public record SourceRecord(int Id, string Name);
                
                public record TargetRecord
                {
                    public int Id { get; init; }
                    public string Name { get; init; }
                }

                [Mapper]
                public partial class TestMapper
                {
                    public partial TargetRecord Map(SourceRecord source);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        result.GeneratedSources[0].Should().Contain("Id = source.Id");
        result.GeneratedSources[0].Should().Contain("Name = source.Name");
    }

    [Fact]
    public void InitOnlyProperties_AreMapped()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Source
                {
                    public int Id { get; set; }
                    public string Value { get; set; }
                }

                public class Target
                {
                    public int Id { get; init; }
                    public string Value { get; init; }
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
        result.GeneratedSources[0].Should().Contain("Value = source.Value");
    }

    [Fact]
    public void InheritedProperties_AreMapped()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class BaseSource
                {
                    public int Id { get; set; }
                }

                public class DerivedSource : BaseSource
                {
                    public string Name { get; set; }
                }

                public class BaseTarget
                {
                    public int Id { get; set; }
                }

                public class DerivedTarget : BaseTarget
                {
                    public string Name { get; set; }
                }

                [Mapper]
                public partial class TestMapper
                {
                    public partial DerivedTarget Map(DerivedSource source);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        // Should map both base and derived properties
        result.GeneratedSources[0].Should().Contain("Id = source.Id");
        result.GeneratedSources[0].Should().Contain("Name = source.Name");
    }

    [Fact]
    public void NullableReferenceTypes_AreHandled()
    {
        var source = """
            #nullable enable
            using LoMapper;

            namespace TestNamespace
            {
                public class Source
                {
                    public string? NullableValue { get; set; }
                    public string NonNullableValue { get; set; } = "";
                }

                public class Target
                {
                    public string? NullableValue { get; set; }
                    public string NonNullableValue { get; set; } = "";
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
        result.GeneratedSources[0].Should().Contain("NullableValue = source.NullableValue");
        result.GeneratedSources[0].Should().Contain("NonNullableValue = source.NonNullableValue");
    }

    [Fact]
    public void MultipleMapperMethods_InSameClass()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class User { public int Id { get; set; } }
                public class UserDto { public int Id { get; set; } }
                public class Order { public int Id { get; set; } }
                public class OrderDto { public int Id { get; set; } }

                [Mapper]
                public partial class AppMapper
                {
                    public partial UserDto MapUser(User user);
                    public partial OrderDto MapOrder(Order order);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        result.GeneratedSources[0].Should().Contain("UserDto MapUser(");
        result.GeneratedSources[0].Should().Contain("OrderDto MapOrder(");
    }

    [Fact]
    public void ReadOnlyProperties_AreSkipped()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Source
                {
                    public int Id { get; set; }
                    public string Computed => "computed";
                }

                public class Target
                {
                    public int Id { get; set; }
                    public string ReadOnly { get; } = "readonly";
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
        // ReadOnly property should not appear in assignments (no setter)
        result.GeneratedSources[0].Should().NotContain("ReadOnly =");
    }

    [Fact]
    public void EnumProperties_AreMapped()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public enum Status { Active, Inactive }
                
                public class Source { public Status Status { get; set; } }
                public class Target { public Status Status { get; set; } }

                [Mapper]
                public partial class TestMapper
                {
                    public partial Target Map(Source source);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        result.GeneratedSources[0].Should().Contain("Status = source.Status");
    }

    [Fact]
    public void PrivateSetters_AreSkipped()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Source
                {
                    public int Id { get; set; }
                    public string Name { get; set; }
                }

                public class Target
                {
                    public int Id { get; set; }
                    public string Name { get; private set; }
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
        // Private setter should not be assigned
        result.GeneratedSources[0].Should().NotContain("Name = source.Name");
    }

    [Fact]
    public void GlobalNamespace_IsSupported()
    {
        var source = """
            using LoMapper;

            public class Source { public int Id { get; set; } }
            public class Target { public int Id { get; set; } }

            [Mapper]
            public partial class TestMapper
            {
                public partial Target Map(Source source);
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        result.GeneratedSources[0].Should().Contain("Id = source.Id");
        // Should not have namespace declaration for global namespace
        result.GeneratedSources[0].Should().NotContain("namespace");
    }
}
