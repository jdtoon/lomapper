using LoMapper.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace LoMapper.Tests;

/// <summary>
/// Helper class for running the LoMapper source generator in tests.
/// </summary>
public static class GeneratorTestHelper
{
    /// <summary>
    /// Runs the LoMapper generator on the provided source code.
    /// </summary>
    public static GeneratorRunResult RunGenerator(string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        // Get all required references including netstandard
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Concat(new[]
            {
                MetadataReference.CreateFromFile(typeof(MapperAttribute).Assembly.Location),
            })
            .ToArray();

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new LoMapperGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        var runResult = driver.GetRunResult();

        return new GeneratorRunResult(
            runResult.GeneratedTrees.Select(t => t.ToString()).ToArray(),
            outputCompilation.GetDiagnostics().ToArray(),
            diagnostics.ToArray());
    }
}

public record GeneratorRunResult(
    string[] GeneratedSources,
    Diagnostic[] CompilationDiagnostics,
    Diagnostic[] GeneratorDiagnostics);
