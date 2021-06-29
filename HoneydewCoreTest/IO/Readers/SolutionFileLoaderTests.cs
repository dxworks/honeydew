using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.IO.Readers;
using HoneydewCore.IO.Readers.Strategies;
using HoneydewCore.Models;
using Microsoft.CodeAnalysis;
using Moq;
using Xunit;

namespace HoneydewCoreTest.IO.Readers
{
    public class SolutionFileLoaderTests
    {
        private readonly ISolutionLoader _sut;

        private readonly Mock<IFileReader> _fileReaderMock = new();
        private readonly Mock<ISolutionProvider> _solutionProviderMock = new();
        private readonly Mock<ISolutionLoadingStrategy> _solutionLoadingStrategyMock = new();

        public SolutionFileLoaderTests()
        {
            _sut = new SolutionFileLoader(new List<IFactExtractor>(), _solutionProviderMock.Object,
                _solutionLoadingStrategyMock.Object);
        }

        [Fact]
        public void LoadSolution_ShouldThrowProjectNotFoundException_WhenGivenAnInvalidPath()
        {
            const string pathToSolution = "invalidPathToProject";

            _solutionProviderMock.Setup(provider => provider.GetSolution(pathToSolution))
                .Throws<ProjectNotFoundException>();

            Assert.Throws<ProjectNotFoundException>(() => _sut.LoadSolution(pathToSolution));
        }

        [Fact]
        public void LoadSolution_ShouldThrowProjectWithErrors_WhenGivenAProjectWithErrors()
        {
            const string pathToSolution = "validPathToProject";

            _solutionProviderMock.Setup(provider => provider.GetSolution(pathToSolution))
                .Throws<ProjectWithErrorsException>();

            Assert.Throws<ProjectWithErrorsException>(() => _sut.LoadSolution(pathToSolution));
        }

        [Fact]
        public void LoadSolution_ShouldReturnCorrectSolutionModel_WhenGivenAPathToSolution()
        {
            const string pathToSolution = "validPathToProject";
            var solutionModelMock = new Mock<SolutionModel>();

            _solutionProviderMock.Setup(provider => provider.GetSolution(pathToSolution)).Returns(It.IsAny<Solution>());
            _solutionLoadingStrategyMock.Setup(strategy => strategy.Load(It.IsAny<Solution>(), new List<IFactExtractor>()))
                .Returns(solutionModelMock.Object);

            var loadSolution = _sut.LoadSolution(pathToSolution);
            
            Assert.Equal(solutionModelMock.Object, loadSolution);
        }

    }
}