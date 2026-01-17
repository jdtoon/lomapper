using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace LoMapper.Tests;

public class BasicMappingTests
{
    [Fact]
    public void SimplePropertyMapping_GeneratesCorrectCode()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
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

                [Mapper]
                public partial class UserMapper
                {
                    public partial UserDto Map(UserEntity entity);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        result.GeneratedSources[0].Should().Contain("public partial");
        result.GeneratedSources[0].Should().Contain("UserDto Map(");
        result.GeneratedSources[0].Should().Contain("Id = entity.Id");
        result.GeneratedSources[0].Should().Contain("Name = entity.Name");
        result.GeneratedSources[0].Should().Contain("Email = entity.Email");
    }

    [Fact]
    public void CaseInsensitivePropertyMatching_Works()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Source
                {
                    public int ID { get; set; }
                    public string NAME { get; set; }
                }

                public class Target
                {
                    public int Id { get; set; }
                    public string Name { get; set; }
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
        result.GeneratedSources[0].Should().Contain("Id = source.ID");
        result.GeneratedSources[0].Should().Contain("Name = source.NAME");
    }

    [Fact]
    public void NullInput_ReturnsNull()
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
                    public partial Target Map(Source source);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources[0].Should().Contain("if (source is null) return null!");
    }
}
