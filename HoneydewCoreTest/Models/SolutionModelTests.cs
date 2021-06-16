using System.Collections.Generic;
using HoneydewCore.Models;
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
        public void Add_ShouldAddNamespace_WhenANewClassModelWithNamespaceIsAdded()
        {
            _sut.Add(new ProjectClassModel {FullName = "Models.Class"});

            Assert.Equal(1, _sut.Namespaces.Count);
            Assert.Equal("Models", _sut.Namespaces["Models"].Name);
        }

        [Fact]
        public void Add_ShouldAddNamespace_WhenMultipleClassModelsWithNamespaceIsAdded()
        {
            _sut.Add(new ProjectClassModel {FullName = "Models.M1"});
            _sut.Add(new ProjectClassModel {FullName = "Models.Domain.M2"});
            _sut.Add(new ProjectClassModel {FullName = "Items.I1"});
            _sut.Add(new ProjectClassModel {FullName = "Services.S1"});

            Assert.Equal(4, _sut.Namespaces.Count);
            Assert.Equal("Models", _sut.Namespaces["Models"].Name);
            Assert.Equal("Models.Domain", _sut.Namespaces["Models.Domain"].Name);
            Assert.Equal("Items", _sut.Namespaces["Items"].Name);
            Assert.Equal("Services", _sut.Namespaces["Services"].Name);
        }

        [Fact]
        public void Add_ShouldAddNamespaceOnce_WhenDifferentClassesAreAddedFromTheSameNamespace()
        {
            _sut.Add(new ProjectClassModel {FullName = "Models.M1"});
            _sut.Add(new ProjectClassModel {FullName = "Models.M2"});

            Assert.Equal(1, _sut.Namespaces.Count);
            Assert.Equal("Models", _sut.Namespaces["Models"].Name);
        }

        [Fact]
        public void FindClassFullNameInUsings_ShouldReturnTheSameName_WhenGivenAClassNameThatCouldNotBeFoundInUsings()
        {
            var usingsList = new List<string>
            {
                "System",
                "Models"
            };

            _sut.Add(new ProjectClassModel {FullName = "Models.Model1"});
            _sut.Add(new ProjectClassModel {FullName = "Models.Model2"});

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

            _sut.Add(new ProjectClassModel {FullName = "Models.Model1"});
            _sut.Add(new ProjectClassModel {FullName = "Services.IService"});
            _sut.Add(new ProjectClassModel {FullName = "Models.Model2"});

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

            _sut.Add(new ProjectClassModel {FullName = "Models.Model1"});
            _sut.Add(new ProjectClassModel {FullName = "Services.IService"});
            _sut.Add(new ProjectClassModel {FullName = "Services.Impl.Service"});
            _sut.Add(new ProjectClassModel {FullName = "Models.Model2"});
            _sut.Add(new ProjectClassModel {FullName = "Domain.Model.DTO.ModelDTO"});

            var fullName = _sut.FindClassFullNameInUsings(usingsList, "ModelDTO");

            Assert.Equal("Domain.Model.DTO.ModelDTO", fullName);
        }
    }
}