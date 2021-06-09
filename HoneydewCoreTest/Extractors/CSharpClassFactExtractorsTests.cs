using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Extractors.Models;
using HoneydewCore.Models;
using Xunit;

namespace HoneydewCoreTest.Extractors
{
    public class CSharpClassFactExtractorsTests
    {
        private readonly IFactExtractor _sut;

        public CSharpClassFactExtractorsTests()
        {
            _sut = new CSharpClassFactExtractor(new List<CSharpMetricExtractor>());
        }

        [Fact]
        public void FileType_ShouldReturnCS()
        {
            Assert.Equal(".cs", _sut.FileType());
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
        public void Extract_ShouldSetClassNameAndNamespace_WhenParsingTextWithOneClass()
        {
            const string fileContent = @"        
                                    namespace Models.Main.Items
                                    {
                                      public class MainItem
                                      {
                                      }
                                    }
                                    ";
            var classModels = _sut.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            foreach (var classModel in classModels)
            {
                Assert.Equal(typeof(ClassModel), classModel.GetType());

                Assert.Equal("Models.Main.Items", classModel.Namespace);
                Assert.Equal("MainItem", classModel.Name);
            }
        }

        [Fact]
        public void Extract_ShouldNotHaveMetrics_WhenGivenAnEmptyListOfMetrics_ForOneClass()
        {
            const string fileContent = @"    

                                    using System;
                                    using System.Collections.Generic;    
                                    namespace Models
                                    {
                                      public class Item
                                      {
                                      }
                                    }
                                    ";
            var classModels = _sut.Extract(fileContent);

            foreach (var classModel in classModels)
            {
                Assert.Equal(typeof(ClassModel), classModel.GetType());

                Assert.False(classModel.Metrics.HasMetrics());
            }
        }
    }
}