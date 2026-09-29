using System.Numerics.Tensors;
using FluentAssertions;
using Xunit;

namespace Fuzzer.Tests;

public class CosineSimilarityTests
{
    [Fact]
    public void CosineSimilarity_ExactCopies_ReturnsOne()
    {
        // Arrange
        float[] vectorA = { 1.0f, 0.5f, 0.2f };
        float[] vectorB = { 1.0f, 0.5f, 0.2f };

        // Act
        float similarity = TensorPrimitives.CosineSimilarity(vectorA, vectorB);

        // Assert
        similarity.Should().BeApproximately(1.0f, 0.001f);
    }

    [Fact]
    public void CosineSimilarity_OrthogonalVectors_ReturnsZero()
    {
        // Arrange
        float[] vectorA = { 1.0f, 0.0f };
        float[] vectorB = { 0.0f, 1.0f };

        // Act
        float similarity = TensorPrimitives.CosineSimilarity(vectorA, vectorB);

        // Assert
        similarity.Should().BeApproximately(0.0f, 0.001f);
    }

    [Fact]
    public void CosineSimilarity_OppositeVectors_ReturnsNegativeOne()
    {
        // Arrange
        float[] vectorA = { 1.0f, 1.0f };
        float[] vectorB = { -1.0f, -1.0f };

        // Act
        float similarity = TensorPrimitives.CosineSimilarity(vectorA, vectorB);

        // Assert
        similarity.Should().BeApproximately(-1.0f, 0.001f);
    }
}
