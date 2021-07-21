using System.Collections.Generic;
using HoneydewCore.Models;
using HoneydewModels;
using Xunit;

namespace HoneydewCoreTest.Models
{
    public class ProjectModelTests
    {
        private readonly ProjectModel _sut;

        public ProjectModelTests()
        {
            _sut = new ProjectModel();
        }

        [Fact]
        public void Add_ShouldAddNamespace_WhenANewClassModelWithNamespaceIsAdded()
        {
            _sut.Add(new ClassModel {FullName = "Models.Class"});

            Assert.Equal(1, _sut.Namespaces.Count);
            Assert.Equal("Models", _sut.Namespaces["Models"].Name);
        }
        
        [Fact]
        public void Add_ShouldAddNewClass_WhenClassIsFromDefaultNamespace()
        {
            _sut.Add(new ClassModel {FullName = "GlobalClass"});

            Assert.Equal(1, _sut.Namespaces.Count);
            Assert.Equal("", _sut.Namespaces[""].Name);
        }

        [Fact]
        public void Add_ShouldAddNamespace_WhenMultipleClassModelsWithNamespaceIsAdded()
        {
            _sut.Add(new ClassModel {FullName = "Models.M1"});
            _sut.Add(new ClassModel {FullName = "Models.Domain.M2"});
            _sut.Add(new ClassModel {FullName = "Items.I1"});
            _sut.Add(new ClassModel {FullName = "Services.S1"});

            Assert.Equal(4, _sut.Namespaces.Count);
            Assert.Equal("Models", _sut.Namespaces["Models"].Name);
            Assert.Equal("Models.Domain", _sut.Namespaces["Models.Domain"].Name);
            Assert.Equal("Items", _sut.Namespaces["Items"].Name);
            Assert.Equal("Services", _sut.Namespaces["Services"].Name);
        }

        [Fact]
        public void Add_ShouldAddNamespaceOnce_WhenDifferentClassesAreAddedFromTheSameNamespace()
        {
            _sut.Add(new ClassModel {FullName = "Models.M1"});
            _sut.Add(new ClassModel {FullName = "Models.M2"});

            Assert.Equal(1, _sut.Namespaces.Count);
            Assert.Equal("Models", _sut.Namespaces["Models"].Name);
        }
    }
}
