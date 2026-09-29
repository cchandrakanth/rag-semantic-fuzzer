using FluentAssertions;
using Fuzzer.Core.Abstractions;
using Fuzzer.Core.Engine;
using Fuzzer.Core.Models;
using Moq;
using Xunit;

namespace Fuzzer.Tests;

public class EmbeddingDriftFuzzerTests
{
    [Fact]
    public async Task RunSemanticFuzzAsync_CalculatesAccurateReports()
    {
        // Arrange
        var goldenDocument = "The system is compliant.";
        var mockEmbeddingService = new Mock<ITextEmbeddingService>();
        
        // Mock returning a deterministic embedding
        mockEmbeddingService
            .Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReadOnlyMemory<float>(new float[] { 1.0f, 0.0f }));

        var mockVectorStore = new Mock<IVectorStoreAdapter>();
        
        // Setup vector store to return one unpoisoned chunk for all searches
        var chunks = new List<VectorChunk>
        {
            new VectorChunk("1", goldenDocument, new float[] { 1.0f, 0.0f }, IsPoisoned: false)
        };
        
        mockVectorStore
            .Setup(v => v.SearchAsync(It.IsAny<ReadOnlyMemory<float>>(), It.IsAny<int>(), It.IsAny<float>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(chunks);

        var fuzzer = new EmbeddingDriftFuzzer(mockEmbeddingService.Object, mockVectorStore.Object);

        // Act
        var report = await fuzzer.RunSemanticFuzzAsync(goldenDocument, perturbationThreshold: 0.5f);

        // Assert
        report.Should().NotBeNull();
        report.GoldenDocument.Should().Be(goldenDocument);
        report.TotalMutantsGenerated.Should().BeGreaterThan(0);
        report.Results.Should().HaveCount(report.TotalMutantsGenerated);
        
        // All mutants were matched with an unpoisoned chunk
        report.SuccessfulRetrievals.Should().Be(report.TotalMutantsGenerated);
        report.RobustnessDelta.Should().Be(1.0f);
        report.HallucinationSusceptibilityScore.Should().Be(0.0f);
        
        // Ensure all cosine similarities are 1.0 since we mocked the embedding service to always return {1.0, 0.0}
        report.AverageCosineDrift.Should().BeApproximately(1.0f, 0.001f);
    }

    [Fact]
    public async Task RunSemanticFuzzAsync_IdentifiesPoisonedRetrievals()
    {
        // Arrange
        var goldenDocument = "The system is compliant.";
        var mockEmbeddingService = new Mock<ITextEmbeddingService>();
        
        mockEmbeddingService
            .Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReadOnlyMemory<float>(new float[] { 1.0f, 0.0f }));

        var mockVectorStore = new Mock<IVectorStoreAdapter>();
        
        // Setup vector store to return a POISONED chunk
        var chunks = new List<VectorChunk>
        {
            new VectorChunk("1", "The system is NON-COMPLIANT.", new float[] { 1.0f, 0.0f }, IsPoisoned: true)
        };
        
        mockVectorStore
            .Setup(v => v.SearchAsync(It.IsAny<ReadOnlyMemory<float>>(), It.IsAny<int>(), It.IsAny<float>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(chunks);

        var fuzzer = new EmbeddingDriftFuzzer(mockEmbeddingService.Object, mockVectorStore.Object);

        // Act
        var report = await fuzzer.RunSemanticFuzzAsync(goldenDocument);

        // Assert
        report.Should().NotBeNull();
        report.TotalMutantsGenerated.Should().BeGreaterThan(0);
        
        // Since all retrievals are poisoned, we should have 0 successful retrievals
        report.SuccessfulRetrievals.Should().Be(0);
        report.RobustnessDelta.Should().Be(0.0f);
        report.HallucinationSusceptibilityScore.Should().Be(1.0f);
        
        // Check individual results
        report.Results.Should().OnlyContain(r => r.RetrievedPoisonedData == true);
        report.Results.Should().OnlyContain(r => r.WasSuccessful == false);
    }
}
