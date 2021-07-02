using HoneydewCore.Models.Representations.ReferenceModel;
using Xunit;

namespace HoneydewCoreTest.Models.Representations.ReferenceModel
{
    public class MissingClassModelsHandlerTests
    {
        private readonly IMissingClassModelsHandler _sut;

        public MissingClassModelsHandlerTests()
        {
            _sut = new MissingClassModelsHandler();
        }

        [Theory]
        [InlineData("int")]
        [InlineData("Project0.Namespace.Class")]
        [InlineData("string")]
        [InlineData("void")]
        public void GetAndAddReference_ShouldReturnTheSameReference_WhenCalledMultipleTimesWithTheSameTypeName(
            string typeName)
        {
            var referenceClassModel = _sut.GetAndAddReference(typeName);
            Assert.NotNull(referenceClassModel);
            Assert.Equal(referenceClassModel, _sut.GetAndAddReference(typeName));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void GetAndAddReference_ShouldReturnNull_WhenCalledMultipleTimesWithEmptyOrWhitespaceName(
            string typeName)
        {
            Assert.Null(_sut.GetAndAddReference(typeName));
        }

        [Fact]
        public void GetAllReferences_ShouldReturnAllReferences_WhenAddedMultipleTypeNames()
        {
            _sut.GetAndAddReference("int");
            _sut.GetAndAddReference(null);
            _sut.GetAndAddReference("");
            _sut.GetAndAddReference("int");
            _sut.GetAndAddReference("float");
            _sut.GetAndAddReference("void");
            _sut.GetAndAddReference("Project.Namespace.Class1");
            _sut.GetAndAddReference("Project.Namespace.Class2");
            _sut.GetAndAddReference("Project.Namespace2.Class1");
            _sut.GetAndAddReference("string");
            _sut.GetAndAddReference("SomeClass");
            _sut.GetAndAddReference(" ");


            var referenceClassModels = _sut.GetAllReferences();
            
            Assert.Equal(8, referenceClassModels.Count);
            
            Assert.Equal("int", referenceClassModels[0].Name);
            Assert.Equal("float", referenceClassModels[1].Name);
            Assert.Equal("void", referenceClassModels[2].Name);
            Assert.Equal("Project.Namespace.Class1", referenceClassModels[3].Name);
            Assert.Equal("Project.Namespace.Class2", referenceClassModels[4].Name);
            Assert.Equal("Project.Namespace2.Class1", referenceClassModels[5].Name);
            Assert.Equal("string", referenceClassModels[6].Name);
            Assert.Equal("SomeClass", referenceClassModels[7].Name);
        }
    }
}