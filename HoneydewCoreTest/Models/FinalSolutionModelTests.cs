using HoneydewCore.Models;
using Xunit;

namespace HoneydewCoreTest.Models
{
    public class FinalSolutionModelTests
    {
        private readonly FinalSolutionModel _sut;

        public FinalSolutionModelTests()
        {
            _sut = new FinalSolutionModel();
        }

        [Fact]
        public void Add_ShouldAddNamespace_WhenANewClassModelWithNamespaceIsAdded()
        {
            _sut.Add(new ClassModel {Namespace = "Models"});

            Assert.Equal(1, _sut.Namespaces.Count);
            Assert.Equal("Models", _sut.Namespaces[0].Name);
        }
        
        [Fact]
        public void Add_ShouldAddNamespace_WhenMultipleClassModelsWithNamespaceIsAdded()
        {
            _sut.Add(new ClassModel {Namespace = "Models"});
            _sut.Add(new ClassModel {Namespace = "Models.Domain"});
            _sut.Add(new ClassModel {Namespace = "Items"});
            _sut.Add(new ClassModel {Namespace = "Services"});

            Assert.Equal(4, _sut.Namespaces.Count);
            Assert.Equal("Models", _sut.Namespaces[0].Name);
            Assert.Equal("Models.Domain", _sut.Namespaces[1].Name);
            Assert.Equal("Items", _sut.Namespaces[2].Name);
            Assert.Equal("Services", _sut.Namespaces[3].Name);
        }
        
        [Fact]
        public void Add_ShouldAddNamespaceOnce_WhenDifferentClassesAreAddedFromTheSameNamespace()
        {
            _sut.Add(new ClassModel {Namespace = "Models"});
            _sut.Add(new ClassModel {Namespace = "Models"});

            Assert.Equal(1, _sut.Namespaces.Count);
            Assert.Equal("Models", _sut.Namespaces[0].Name);
        }
    }
}