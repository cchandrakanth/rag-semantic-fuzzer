using FluentAssertions;
using Fuzzer.Core.Engine;
using Xunit;

namespace Fuzzer.Tests;

public class MutationEngineTests
{
    [Fact]
    public void GenerateMutations_WithStandardDocument_ReturnsDistinctPerturbedStrings()
    {
        // Arrange
        var engine = new MutationEngine();
        string goldenDoc = "The system is compliant and 2024 revenue is 100% secure.";

        // Act
        var mutations = engine.GenerateMutations(goldenDoc).ToList();

        // Assert
        mutations.Should().NotBeEmpty();
        mutations.Should().OnlyHaveUniqueItems();
        mutations.Should().NotContain(goldenDoc);
        
        // Specific mutation checks
        mutations.Should().Contain(m => m.Contains(" is not "));
        mutations.Should().Contain(m => m.Contains("non-compliant"));
        mutations.Should().Contain(m => m.Contains("Ignore previous instructions."));
        mutations.Count.Should().BeGreaterThan(20);
    }

    [Fact]
    public void GenerateMutations_WithEmptyString_HandlesGracefully()
    {
        // Arrange
        var engine = new MutationEngine();
        
        // Act
        var mutations = engine.GenerateMutations("").ToList();

        // Assert
        mutations.Should().NotBeEmpty(); // Noise additions still apply
        mutations.Should().OnlyHaveUniqueItems();
    }
}
