using Xunit;

namespace LoMapper.Tests;

public class CircularReferenceTests
{
    [Fact]
    public void CircularReference_DirectCycle_ReportsDiagnostic()
    {
        var source = @"
using LoMapper;

namespace Test
{
    public class Parent
    {
        public int Id { get; set; }
        public Child? Child { get; set; }
    }

    public class Child
    {
        public int Id { get; set; }
        public Parent? Parent { get; set; }
    }

    public class ParentDto
    {
        public int Id { get; set; }
        public ChildDto? Child { get; set; }
    }

    public class ChildDto
    {
        public int Id { get; set; }
        public ParentDto? Parent { get; set; }
    }

    [Mapper]
    public partial class TestMapper
    {
        public partial ParentDto Map(Parent parent);
        public partial ChildDto Map(Child child);
    }
}";

        var diagnostics = GeneratorTestHelper.RunGenerator(source);

        // Should report LOM010 for circular reference
        var allDiagnostics = diagnostics.CompilationDiagnostics.Concat(diagnostics.GeneratorDiagnostics);
        Assert.Contains(allDiagnostics, d => d.Id == "LOM010" && d.GetMessage().Contains("Parent") && d.GetMessage().Contains("Child"));
    }

    [Fact]
    public void CircularReference_IndirectCycle_ReportsDiagnostic()
    {
        var source = @"
using LoMapper;

namespace Test
{
    public class NodeA
    {
        public int Id { get; set; }
        public NodeB? Next { get; set; }
    }

    public class NodeB
    {
        public int Id { get; set; }
        public NodeC? Next { get; set; }
    }

    public class NodeC
    {
        public int Id { get; set; }
        public NodeA? Next { get; set; }
    }

    public class NodeADto
    {
        public int Id { get; set; }
        public NodeBDto? Next { get; set; }
    }

    public class NodeBDto
    {
        public int Id { get; set; }
        public NodeCDto? Next { get; set; }
    }

    public class NodeCDto
    {
        public int Id { get; set; }
        public NodeADto? Next { get; set; }
    }

    [Mapper]
    public partial class TestMapper
    {
        public partial NodeADto Map(NodeA node);
        public partial NodeBDto Map(NodeB node);
        public partial NodeCDto Map(NodeC node);
    }
}";

        var diagnostics = GeneratorTestHelper.RunGenerator(source);

        // Should report LOM010 for circular reference chain
        var allDiagnostics = diagnostics.CompilationDiagnostics.Concat(diagnostics.GeneratorDiagnostics);
        Assert.Contains(allDiagnostics, d => d.Id == "LOM010");
    }

    [Fact]
    public void CircularReference_WithMapIgnore_NoDiagnostic()
    {
        var source = @"
using LoMapper;

namespace Test
{
    public class Parent
    {
        public int Id { get; set; }
        public Child? Child { get; set; }
    }

    public class Child
    {
        public int Id { get; set; }
        public Parent? Parent { get; set; }
    }

    public class ParentDto
    {
        public int Id { get; set; }
        public ChildDto? Child { get; set; }
    }

    public class ChildDto
    {
        public int Id { get; set; }
        public ParentDto? Parent { get; set; }
    }

    [Mapper]
    public partial class TestMapper
    {
        public partial ParentDto Map(Parent parent);

        [MapIgnore(nameof(ChildDto.Parent))]
        public partial ChildDto Map(Child child);
    }
}";

        var diagnostics = GeneratorTestHelper.RunGenerator(source);

        // Should NOT report LOM010 because the cycle is broken by MapIgnore
        // The ChildDto mapper ignores the Parent property, so no infinite recursion.
        var allDiagnostics = diagnostics.CompilationDiagnostics.Concat(diagnostics.GeneratorDiagnostics);
        Assert.DoesNotContain(allDiagnostics, d => d.Id == "LOM010");
    }

    [Fact]
    public void CircularReference_CollectionOfSameType_ReportsDiagnostic()
    {
        var source = @"
using LoMapper;
using System.Collections.Generic;

namespace Test
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = """";
        public List<Category> SubCategories { get; set; } = new();
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = """";
        public List<CategoryDto> SubCategories { get; set; } = new();
    }

    [Mapper]
    public partial class TestMapper
    {
        public partial CategoryDto Map(Category category);
    }
}";

        var diagnostics = GeneratorTestHelper.RunGenerator(source);

        // Should report LOM010 for self-referencing collection
        var allDiagnostics = diagnostics.CompilationDiagnostics.Concat(diagnostics.GeneratorDiagnostics);
        Assert.Contains(allDiagnostics, d => d.Id == "LOM010" && d.GetMessage().Contains("Category"));
    }

    [Fact]
    public void CircularReference_NoCycle_NoDiagnostic()
    {
        var source = @"
using LoMapper;

namespace Test
{
    public class Order
    {
        public int Id { get; set; }
        public Customer? Customer { get; set; }
        public Address? ShippingAddress { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = """";
    }

    public class Address
    {
        public string Street { get; set; } = """";
        public string City { get; set; } = """";
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public CustomerDto? Customer { get; set; }
        public AddressDto? ShippingAddress { get; set; }
    }

    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = """";
    }

    public class AddressDto
    {
        public string Street { get; set; } = """";
        public string City { get; set; } = """";
    }

    [Mapper]
    public partial class TestMapper
    {
        public partial OrderDto Map(Order order);
        public partial CustomerDto Map(Customer customer);
        public partial AddressDto Map(Address address);
    }
}";

        var diagnostics = GeneratorTestHelper.RunGenerator(source);

        // Should NOT report LOM010 when there's no cycle
        var allDiagnostics = diagnostics.CompilationDiagnostics.Concat(diagnostics.GeneratorDiagnostics);
        Assert.DoesNotContain(allDiagnostics, d => d.Id == "LOM010");
    }
}
