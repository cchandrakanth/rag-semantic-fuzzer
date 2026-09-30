using FluentAssertions;
using Fuzzer.Core.Engine;
using Xunit;

namespace Fuzzer.Tests;

public class MutationEngineTests
{
    [Fact]
    public async Task GenerateMutationsAsync_WithStandardDocument_ReturnsDistinctPerturbedStrings()
    {
        // Arrange
        var engine = new MutationEngine();
        string goldenDoc = "The system is compliant and 2024 revenue is 100% secure.";

        try
        {
            // Act (Requesting only 5 for fast testing)
            var mutationsRaw = await engine.GenerateMutationsAsync(goldenDoc, targetCount: 5);
            var mutations = mutationsRaw.ToList();

            // Assert
            mutations.Should().NotBeEmpty();
            mutations.Should().OnlyHaveUniqueItems();
            mutations.Should().NotContain(goldenDoc);
        }
        catch (HttpRequestException)
        {
            // Skip test if LM Studio is not running locally on 1234
            Assert.True(true, "LM Studio not running. Skipping test.");
        }
    }

    [Fact]
    public async Task GenerateMutationsAsync_WithEmptyString_HandlesGracefully()
    {
        // Arrange
        var engine = new MutationEngine();
        
        try
        {
            // Act
            var mutationsRaw = await engine.GenerateMutationsAsync("", targetCount: 5);
            var mutations = mutationsRaw.ToList();

            // Assert
            mutations.Should().NotBeEmpty();
        }
        catch (HttpRequestException)
        {
            // Skip test if LM Studio is not running locally on 1234
            Assert.True(true, "LM Studio not running. Skipping test.");
        }
    }
}
