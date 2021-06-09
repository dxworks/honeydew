using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Models;
using Xunit;

namespace HoneydewCoreTest.Extractors
{
    public class CSharpClassExtractorsTests
    {
        private readonly Extractor<CSharpMetricExtractor> _sut;

        public CSharpClassExtractorsTests()
        {
            _sut = new CSharpClassExtractor(new List<CSharpMetricExtractor>());
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
            var compilationUnitModel = _sut.Extract(fileContent);

            Assert.Equal(1, compilationUnitModel.Entities.Count);

            foreach (var entity in compilationUnitModel.Entities)
            {
                Assert.Equal(typeof(ClassModel), entity.GetType());

                var projectClass = (ClassModel) entity;

                Assert.Equal("Models.Main.Items", projectClass.Namespace);
                Assert.Equal("MainItem", projectClass.Name);
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
            var compilationUnitModel = _sut.Extract(fileContent);

            foreach (var entity in compilationUnitModel.Entities)
            {
                Assert.Equal(typeof(ClassModel), entity.GetType());

                Assert.False(entity.Metrics.HasMetrics());
            }
        }
    }
}