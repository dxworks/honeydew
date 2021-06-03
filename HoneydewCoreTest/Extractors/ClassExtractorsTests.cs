using HoneydewCore.Extractors;
using HoneydewCore.Models;
using Xunit;

namespace HoneydewCoreTest.Extractors
{
    public class ClassExtractorsTests
    {
        private readonly IExtractor _sut;

        public ClassExtractorsTests()
        {
            _sut = new ClassExtractor();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("  ")]
        [InlineData("\t")]
        [InlineData(null)]
        public void Extract_ShouldThrowEmptyContentException_WhenTryingToExtractFromEmptyString(string emptyContent)
        {
            Assert.Throws<EmptyContentException>(() => _sut.Extract(emptyContent));
        }

        [Theory]
        [InlineData(@"namespace Models
                                    {
                                      public class Item
                                      
                                      }
                                    }
                                    ")]
        [InlineData(@"namespace Models
                                    {
                                      publizzc class Item
                                      {
                                            void a(){ }
                                      }
                                    }
                                    ")]
        [InlineData(@"namespace Models
                                    {
                                      public class Item
                                      {
                                            void a(){ int c}
                                      }
                                    }
                                    ")]
        public void Extract_ShouldThrowExtractionException_WhenParsingTextWithCompilationErrors(string fileContent)
        {
            Assert.Throws<ExtractionException>(() => _sut.Extract(fileContent));
        }
        
        [Fact]
        public void Extract_ShouldSetClassNameAndNamespace_WhenParsingText()
        {
            const string fileContent = @"namespace Models
                                    {
                                      public class Item
                                      {
                                      }
                                    }
                                    ";
            var entity = _sut.Extract(fileContent);
            Assert.Equal(typeof(ProjectClass), entity.GetType());

            var projectClass = (ProjectClass) entity;

            Assert.Equal("Models", projectClass.Namespace);
            Assert.Equal("Item", projectClass.Name);
        }
    }
}