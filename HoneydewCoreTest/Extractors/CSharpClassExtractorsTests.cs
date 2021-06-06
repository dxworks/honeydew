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
            var entities = _sut.Extract(fileContent);

            Assert.Equal(1, entities.Count);
            
            foreach (var entity in entities)
            {
                Assert.Equal(typeof(ClassModel), entity.GetType());

                var projectClass = (ClassModel) entity;

                Assert.Equal("Models.Main.Items", projectClass.Namespace);
                Assert.Equal("MainItem", projectClass.Name);   
            }
        }

        [Fact]
        public void Extract_ShouldSetClassNameAndNamespace_WhenParsingTextWithMultipleClasses()
        {
            
            const string fileContent = @"using System;                                
                                    using Microsoft.CodeAnalysis;
                                    using Microsoft.CodeAnalysis.CSharp;

                                    namespace TopLevel
                                    {
                                        using Microsoft;
                                        using System.ComponentModel;

                                        namespace Child1
                                        {
                                            using Microsoft.Win32;

                                            class Foo { }
                                        }

                                        namespace Child2
                                        {
                                            using System.CodeDom;
                                            using Microsoft.CSharp;

                                            class Bar { }
                                        }
                                    }";

            var classNames = new string[2];
            classNames[0] = "Foo";
            classNames[1] = "Barr";
            
            var classNamespaces = new string[2];
            classNamespaces[0] = "TopLevel.Child1";
            classNamespaces[1] = "TopLevel.Child2";
            
            var entities = _sut.Extract(fileContent);

            Assert.Equal(2, entities.Count);
            

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                Assert.Equal(typeof(ClassModel), entity.GetType());

                var projectClass = (ClassModel) entity;

                Assert.Equal(classNamespaces[i], projectClass.Namespace);
                Assert.Equal(classNames[i], projectClass.Name);
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
            var entities = _sut.Extract(fileContent);

            foreach (var entity in entities)
            {
                Assert.Equal(typeof(ClassModel), entity.GetType());

                var projectClass = (ClassModel) entity;

                Assert.Empty(projectClass.Metrics);   
            }
        }
    }
}