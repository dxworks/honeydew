using System.Collections.Generic;
using HoneydewCore.IO.Writers;
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
        public void FindClassFullNameInUsings_ShouldReturnTheSameName_WhenGivenAClassNameThatCouldNotBeFoundInUsings()
        {
            var usingsList = new List<string>
            {
                "System",
                "Models"
            };

            var projectModel = new ProjectModel("Project");

            projectModel.Add(new ClassModel {FullName = "Models.Model1"});
            projectModel.Add(new ClassModel {FullName = "Models.Model2"});

            _sut.Projects.Add(projectModel);

            var fullName = _sut.FindClassFullNameInUsings(usingsList, "Service");

            Assert.Equal("Service", fullName);
        }

        [Fact]
        public void
            FindClassFullNameInUsings_ShouldReturnTheSameName_WhenGivenAClassNameThatCanBeFoundInUsings_NamespaceHasOneLevel()
        {
            var usingsList = new List<string>
            {
                "System",
                "Models",
                "Services"
            };

            var projectModel = new ProjectModel("Project");

            projectModel.Add(new ClassModel {FullName = "Models.Model1"});
            projectModel.Add(new ClassModel {FullName = "Services.IService"});
            projectModel.Add(new ClassModel {FullName = "Models.Model2"});

            _sut.Projects.Add(projectModel);

            var fullName = _sut.FindClassFullNameInUsings(usingsList, "IService");

            Assert.Equal("Services.IService", fullName);
        }

        [Fact]
        public void
            FindClassFullNameInUsings_ShouldReturnTheSameName_WhenGivenAClassNameThatCanBeFoundInUsings_NamespaceHasMultipleevels()
        {
            var usingsList = new List<string>
            {
                "System",
                "Models",
                "Services",
                "Services.Impl",
                "Domain.Model.DTO"
            };

            var projectModel = new ProjectModel("Project");

            projectModel.Add(new ClassModel {FullName = "Models.Model1"});
            projectModel.Add(new ClassModel {FullName = "Services.IService"});
            projectModel.Add(new ClassModel {FullName = "Services.Impl.Service"});
            projectModel.Add(new ClassModel {FullName = "Models.Model2"});
            projectModel.Add(new ClassModel {FullName = "Domain.Model.DTO.ModelDTO"});

            _sut.Projects.Add(projectModel);

            var fullName = _sut.FindClassFullNameInUsings(usingsList, "ModelDTO");

            Assert.Equal("Domain.Model.DTO.ModelDTO", fullName);
        }


        [Fact]
        public void Export_ShouldReturnEmptyString_WhenExporterIsNotASolutionModelExporter()
        {
            var exporterMock = new Mock<IExporter>();

            Assert.Equal("", _sut.Export(exporterMock.Object));
        }

        [Fact]
        public void GetClassModelByFullName_ShouldReturnClassModel_WhenGivenFullClassName()
        {
            var projectModel = new ProjectModel("Project");

            string[] array =
            {
                "Project.Models.Model1",
                "Project.Services.IService",
                "Project.Services.Impl.Service",
                "Project.Models.Model2",
                "Project.Domain.Model.DTO.ModelDTO"
            };

            foreach (var className in array)
            {
                projectModel.Add(new ClassModel {FullName = className});
            }

            _sut.Projects.Add(projectModel);


            Assert.Equal(_sut.Projects[0].Namespaces["Project.Models"].ClassModels[0],
                _sut.GetClassModelByFullName(array[0]));
            Assert.Equal(_sut.Projects[0].Namespaces["Project.Services"].ClassModels[0],
                _sut.GetClassModelByFullName(array[1]));
            Assert.Equal(_sut.Projects[0].Namespaces["Project.Services.Impl"].ClassModels[0],
                _sut.GetClassModelByFullName(array[2]));
            Assert.Equal(_sut.Projects[0].Namespaces["Project.Models"].ClassModels[1],
                _sut.GetClassModelByFullName(array[3]));
            Assert.Equal(_sut.Projects[0].Namespaces["Project.Domain.Model.DTO"].ClassModels[0],
                _sut.GetClassModelByFullName(array[4]));
        }
    }
}