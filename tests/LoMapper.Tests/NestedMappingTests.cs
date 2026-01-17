using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace LoMapper.Tests;

public class NestedMappingTests
{
    [Fact]
    public void NestedObject_UsesExplicitMapperMethod()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class AddressEntity
                {
                    public string Street { get; set; }
                    public string City { get; set; }
                }

                public class AddressDto
                {
                    public string Street { get; set; }
                    public string City { get; set; }
                }

                public class UserEntity
                {
                    public int Id { get; set; }
                    public AddressEntity Address { get; set; }
                }

                public class UserDto
                {
                    public int Id { get; set; }
                    public AddressDto Address { get; set; }
                }

                [Mapper]
                public partial class UserMapper
                {
                    public partial UserDto MapUser(UserEntity entity);
                    public partial AddressDto MapAddress(AddressEntity entity);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        
        // User mapping should call address mapper
        result.GeneratedSources[0].Should().Contain("MapAddress(entity.Address)");
        
        // Address mapping should have its own implementation
        result.GeneratedSources[0].Should().Contain("AddressDto MapAddress(");
    }

    [Fact]
    public void NestedObject_WithNullCheck_GeneratesCorrectCode()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Inner { public string Value { get; set; } }
                public class InnerDto { public string Value { get; set; } }

                public class Outer { public Inner Child { get; set; } }
                public class OuterDto { public InnerDto Child { get; set; } }

                [Mapper]
                public partial class TestMapper
                {
                    public partial OuterDto MapOuter(Outer source);
                    public partial InnerDto MapInner(Inner source);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        // Should include null check for nested object
        result.GeneratedSources[0].Should().Contain("source.Child is null ? null! : MapInner(source.Child)");
    }
}
