using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace LoMapper.Tests;

public class CollectionMappingTests
{
    [Fact]
    public void ListToList_SameType_GeneratesSelectToList()
    {
        var source = """
            using LoMapper;
            using System.Collections.Generic;

            namespace TestNamespace
            {
                public class Source
                {
                    public List<string> Tags { get; set; }
                }

                public class Target
                {
                    public List<string> Tags { get; set; }
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
        result.GeneratedSources[0].Should().Contain("Tags = source.Tags");
    }

    [Fact]
    public void ListToArray_GeneratesSelectToArray()
    {
        var source = """
            using LoMapper;
            using System.Collections.Generic;

            namespace TestNamespace
            {
                public class Source
                {
                    public List<int> Numbers { get; set; }
                }

                public class Target
                {
                    public int[] Numbers { get; set; }
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
        result.GeneratedSources[0].Should().Contain("ToArray()");
    }

    [Fact]
    public void NestedObjectCollection_UsesMapperMethod()
    {
        var source = """
            using LoMapper;
            using System.Collections.Generic;

            namespace TestNamespace
            {
                public class AddressEntity
                {
                    public string City { get; set; }
                }

                public class AddressDto
                {
                    public string City { get; set; }
                }

                public class UserEntity
                {
                    public List<AddressEntity> Addresses { get; set; }
                }

                public class UserDto
                {
                    public List<AddressDto> Addresses { get; set; }
                }

                [Mapper]
                public partial class TestMapper
                {
                    public partial UserDto MapUser(UserEntity entity);
                    public partial AddressDto MapAddress(AddressEntity entity);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        result.GeneratedSources[0].Should().Contain("MapAddress(x)");
        result.GeneratedSources[0].Should().Contain("ToList()");
    }
}
