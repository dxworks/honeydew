using HoneydewCore.IO.Writers.Exporters;
using HoneydewCore.Models;
using Moq;
using Xunit;

namespace HoneydewCoreTest.Models
{
    public class SolutionModelTests
    {
        private readonly SolutionModel _sut;

        public SolutionModelTests()
        {
            _sut = new SolutionModel();
        }

        [Fact]
        public void Add_ShouldAddProject_WhenCallingAdd()
        {
            _sut.Projects.Add(new ProjectModel());

            Assert.Equal(1, _sut.Projects.Count);
        }

        [Fact]
        public void Add_ShouldAddProjects_WhenAddingProjectsWithName()
        {
            _sut.Projects.Add(new ProjectModel("Project1"));
            _sut.Projects.Add(new ProjectModel("Project2"));
            _sut.Projects.Add(new ProjectModel("Project3"));

            Assert.Equal(3, _sut.Projects.Count);
            Assert.Equal("Project1", _sut.Projects[0].Name);
            Assert.Equal("Project2", _sut.Projects[1].Name);
            Assert.Equal("Project3", _sut.Projects[2].Name);
        }

        [Fact]
        public void Export_ShouldReturnEmptyString_WhenExporterIsNotASolutionModelExporter()
        {
            var exporterMock = new Mock<IExporter>();

            Assert.Equal("", _sut.Export(exporterMock.Object));
        }
    }
}