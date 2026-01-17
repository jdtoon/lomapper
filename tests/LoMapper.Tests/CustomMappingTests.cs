using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace LoMapper.Tests;

public class CustomMappingTests
{
    [Fact]
    public void MapPropertyAttribute_RenamesProperty()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Source
                {
                    public string FirstName { get; set; }
                    public string LastName { get; set; }
                }

                public class Target
                {
                    public string FullName { get; set; }
                    public string Surname { get; set; }
                }

                [Mapper]
                public partial class TestMapper
                {
                    [MapProperty("FirstName", "FullName")]
                    [MapProperty("LastName", "Surname")]
                    public partial Target Map(Source source);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        result.GeneratedSources[0].Should().Contain("FullName = source.FirstName");
        result.GeneratedSources[0].Should().Contain("Surname = source.LastName");
    }

    [Fact]
    public void MapPropertyAttribute_WithTransform_CallsMethod()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Source
                {
                    public string FirstName { get; set; }
                    public string LastName { get; set; }
                }

                public class Target
                {
                    public string DisplayName { get; set; }
                }

                [Mapper]
                public partial class TestMapper
                {
                    [MapProperty("FirstName", "DisplayName", Transform = "FormatName")]
                    public partial Target Map(Source source);
                    
                    private string FormatName(string name) => name.ToUpper();
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        result.GeneratedSources[0].Should().Contain("DisplayName = FormatName(source.FirstName)");
    }

    [Fact]
    public void MapIgnoreAttribute_SkipsProperty()
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
                    public string InternalValue { get; set; }
                }

                [Mapper]
                public partial class TestMapper
                {
                    [MapIgnore("InternalValue")]
                    public partial Target Map(Source source);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        result.GeneratedSources[0].Should().Contain("Id = source.Id");
        result.GeneratedSources[0].Should().NotContain("InternalValue");
    }
}
