using FluentAssertions;
using Xunit;

namespace LoMapper.Tests;

public class FlatteningTests
{
    [Fact]
    public void FlattenProperty_WithExplicitAttribute_GeneratesCorrectCode()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Address
                {
                    public string City { get; set; }
                    public string ZipCode { get; set; }
                }

                public class UserEntity
                {
                    public int Id { get; set; }
                    public string Name { get; set; }
                    public Address Address { get; set; }
                }

                public class UserDto
                {
                    public int Id { get; set; }
                    public string Name { get; set; }
                    public string AddressCity { get; set; }
                    public string AddressZipCode { get; set; }
                }

                [Mapper]
                public partial class UserMapper
                {
                    [FlattenProperty("Address.City", nameof(UserDto.AddressCity))]
                    [FlattenProperty("Address.ZipCode", nameof(UserDto.AddressZipCode))]
                    public partial UserDto Map(UserEntity entity);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        result.GeneratedSources[0].Should().Contain("public partial");
        result.GeneratedSources[0].Should().Contain("UserDto Map(");
        // Check for flat property assignments with null-safe navigation
        result.GeneratedSources[0].Should().Contain("AddressCity = entity.Address?.City ?? null!");
        result.GeneratedSources[0].Should().Contain("AddressZipCode = entity.Address?.ZipCode ?? null!");
    }

    [Fact]
    public void FlattenProperty_WithNullableNestedObject_GeneratesNullSafeCode()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Address
                {
                    public string City { get; set; }
                }

                public class UserEntity
                {
                    public int Id { get; set; }
                    public Address? Address { get; set; }
                }

                public class UserDto
                {
                    public int Id { get; set; }
                    public string? AddressCity { get; set; }
                }

                [Mapper]
                public partial class UserMapper
                {
                    [FlattenProperty("Address.City", nameof(UserDto.AddressCity))]
                    public partial UserDto Map(UserEntity entity);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        // Should generate null-safe navigation with null coalescing
        result.GeneratedSources[0].Should().Contain("AddressCity = entity.Address?.City ?? null!");
    }

    [Fact]
    public void FlattenProperty_WithValueTypeTarget_GeneratesDefaultValue()
    {
        var source = """
            using LoMapper;
            using System;

            namespace TestNamespace
            {
                public class PersonInfo
                {
                    public int Age { get; set; }
                }

                public class PersonEntity
                {
                    public string Name { get; set; }
                    public PersonInfo Info { get; set; }
                }

                public class PersonDto
                {
                    public string Name { get; set; }
                    public int PersonAge { get; set; }
                }

                [Mapper]
                public partial class PersonMapper
                {
                    [FlattenProperty("Info.Age", nameof(PersonDto.PersonAge))]
                    public partial PersonDto Map(PersonEntity entity);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        // For value types, should use default(int) instead of null!
        result.GeneratedSources[0].Should().Contain("PersonAge = entity.Info?.Age ?? default(int)");
    }

    [Fact]
    public void FlattenProperty_InvalidSourcePath_ReportsDiagnostic()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class UserEntity
                {
                    public int Id { get; set; }
                }

                public class UserDto
                {
                    public int Id { get; set; }
                    public string NonExistentProperty { get; set; }
                }

                [Mapper]
                public partial class UserMapper
                {
                    [FlattenProperty("NonExistent.Property", nameof(UserDto.NonExistentProperty))]
                    public partial UserDto Map(UserEntity entity);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratorDiagnostics.Should().Contain(d => d.Id == "LOM007");
    }

    [Fact]
    public void FlattenProperty_TargetPropertyNotFound_ReportsDiagnostic()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Address
                {
                    public string City { get; set; }
                }

                public class UserEntity
                {
                    public Address Address { get; set; }
                }

                public class UserDto
                {
                    public int Id { get; set; }
                }

                [Mapper]
                public partial class UserMapper
                {
                    [FlattenProperty(nameof(UserEntity.Address.City), "NonExistentTarget")]
                    public partial UserDto Map(UserEntity entity);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratorDiagnostics.Should().Contain(d => d.Id == "LOM008");
    }

    [Fact]
    public void FlattenProperty_TypeMismatch_ReportsDiagnostic()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Address
                {
                    public int ZipCode { get; set; }
                }

                public class UserEntity
                {
                    public Address Address { get; set; }
                }

                public class UserDto
                {
                    public string AddressZipCode { get; set; } // string, not int
                }

                [Mapper]
                public partial class UserMapper
                {
                    [FlattenProperty("Address.ZipCode", nameof(UserDto.AddressZipCode))]
                    public partial UserDto Map(UserEntity entity);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        // Should report type mismatch (int -> string without explicit conversion)
        result.GeneratorDiagnostics.Should().Contain(d => d.Id == "LOM009");
    }

    [Fact]
    public void FlattenProperty_DeepNesting_Works()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Country
                {
                    public string Code { get; set; }
                }

                public class City
                {
                    public string Name { get; set; }
                    public Country Country { get; set; }
                }

                public class Address
                {
                    public City City { get; set; }
                }

                public class UserEntity
                {
                    public Address Address { get; set; }
                }

                public class UserDto
                {
                    public string CityCountryCode { get; set; }
                }

                [Mapper]
                public partial class UserMapper
                {
                    [FlattenProperty("Address.City.Country.Code", nameof(UserDto.CityCountryCode))]
                    public partial UserDto Map(UserEntity entity);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        result.GeneratedSources[0].Should().Contain("CityCountryCode = entity.Address?.City?.Country?.Code ?? null!");
    }

    [Fact]
    public void FlattenProperty_WithRegularMapping_Works()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Address
                {
                    public string City { get; set; }
                }

                public class UserEntity
                {
                    public int Id { get; set; }
                    public string Name { get; set; }
                    public Address Address { get; set; }
                }

                public class UserDto
                {
                    public int Id { get; set; }
                    public string Name { get; set; }
                    public string AddressCity { get; set; }
                }

                [Mapper]
                public partial class UserMapper
                {
                    [FlattenProperty("Address.City", nameof(UserDto.AddressCity))]
                    public partial UserDto Map(UserEntity entity);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        // Should have regular mappings
        result.GeneratedSources[0].Should().Contain("Id = entity.Id");
        result.GeneratedSources[0].Should().Contain("Name = entity.Name");
        // And flatten mapping
        result.GeneratedSources[0].Should().Contain("AddressCity = entity.Address?.City ?? null!");
    }

    [Fact]
    public void FlattenProperty_CombinedWithMapProperty_Works()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Address
                {
                    public string City { get; set; }
                }

                public class UserEntity
                {
                    public int Id { get; set; }
                    public string FirstName { get; set; }
                    public Address Address { get; set; }
                }

                public class UserDto
                {
                    public int Id { get; set; }
                    public string FullName { get; set; }
                    public string AddressCity { get; set; }
                }

                [Mapper]
                public partial class UserMapper
                {
                    [MapProperty(nameof(UserEntity.FirstName), nameof(UserDto.FullName))]
                    [FlattenProperty("Address.City", nameof(UserDto.AddressCity))]
                    public partial UserDto Map(UserEntity entity);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        // Should have renamed mapping
        result.GeneratedSources[0].Should().Contain("FullName = entity.FirstName");
        // And flatten mapping
        result.GeneratedSources[0].Should().Contain("AddressCity = entity.Address?.City ?? null!");
    }

    [Fact]
    public void FlattenProperty_WithReadOnlyProperty_SkipsMapping()
    {
        var source = """
            using LoMapper;

            namespace TestNamespace
            {
                public class Address
                {
                    public string City { get; set; }
                }

                public class UserEntity
                {
                    public Address Address { get; set; }
                }

                public class UserDto
                {
                    public string AddressCity { get; } // read-only
                }

                [Mapper]
                public partial class UserMapper
                {
                    [FlattenProperty("Address.City", nameof(UserDto.AddressCity))]
                    public partial UserDto Map(UserEntity entity);
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        // Should not include the read-only property
        result.GeneratedSources[0].Should().NotContain("AddressCity = ");
    }
}
