using System.Linq;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.CompilationUnitLevel
{
    public class CSharpCompilationUnitImportsMetricTests
    {
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpCompilationUnitImportsMetricTests()
        {
            var compositeVisitor = new CompositeVisitor();
            
            compositeVisitor.Add(new ImportsVisitor());

            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), compositeVisitor);
        }

        [Theory]
        [InlineData("class")]
        [InlineData("record")]
        [InlineData("struct")]
        [InlineData("enum")]
        [InlineData("interface")]
        public void Extract_ShouldHaveUsings_WhenGivenOneClass(string classType)
        {
            var fileContent = $@"using System;
                                     using System.Collections.Generic;
                                     using System.Linq;
                                     using System.Text;
                                     using Microsoft.CodeAnalysis;
                                     using Microsoft.CodeAnalysis.CSharp;

                                     namespace TopLevel
                                     {{
                                         public {classType} Foo {{ }}                                        
                                     }}";

            var compilationUnit = _factExtractor.Extract(fileContent);

            Assert.Equal(6, compilationUnit.Imports.Count);

            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "System"));
            Assert.NotNull(
                compilationUnit.Imports.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "System.Linq"));
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "System.Text"));
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis"));
            Assert.NotNull(
                compilationUnit.Imports.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis.CSharp"));
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithOneDelegate.txt")]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithClassRecordStructInterfaceAndDelegate_DeclaredInside.txt")]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithClassRecordStructInterfaceAndDelegate.txt")]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithClassRecordStructInterfaceAndDelegate_SharedUsings.txt")]
        public void Extract_ShouldHaveUsings_WhenCompilationUnitText(string fileContent)
        {
            var compilationUnit = _factExtractor.Extract(fileContent);

            Assert.Equal(6, compilationUnit.Imports.Count);

            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "System"));
            Assert.NotNull(
                compilationUnit.Imports.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "System.Linq"));
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "System.Text"));
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis"));
            Assert.NotNull(
                compilationUnit.Imports.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis.CSharp"));
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithRecordStructInterfaceAndDelegate_ImbricatedNamespaces.txt")]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithClassRecordStructInterfaceAndDelegate_SharedUsings_MultipleLevels.txt")]
        public void Extract_ShouldHaveUsings_WhenGivenMultipleClassesAndDelegatesInImbricatedNamespaces(
            string fileContent)
        {
            var compilationUnit = _factExtractor.Extract(fileContent);

            Assert.Equal(8, compilationUnit.Imports.Count);

            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "System"));
            Assert.NotNull(
                compilationUnit.Imports.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "System.Linq"));
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "System.Text"));
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis"));
            Assert.NotNull(
                compilationUnit.Imports.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis.CSharp"));
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "MyLib"));
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "MyLib.Records"));
        }


        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithInnerClasses.txt")]
        public void Extract_ShouldHaveUsings_WhenGivenMultipleClassesWithInnerClasses(string fileContent)
        {
            var compilationUnit = _factExtractor.Extract(fileContent);

            Assert.Equal(6, compilationUnit.Imports.Count);


            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "System"));
            Assert.NotNull(
                compilationUnit.Imports.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "System.Linq"));
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "System.Text"));
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "MyLib"));
            Assert.NotNull(
                compilationUnit.Imports.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis.CSharp"));
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithInnerClasses_ButNoNamespace.txt")]
        public void Extract_ShouldHaveUsings_WhenGivenMultipleClassesWithInnerClasses_ButNoNamespace(string fileContent)
        {
            var compilationUnit = _factExtractor.Extract(fileContent);

            Assert.Equal(4, compilationUnit.Imports.Count);
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "System"));
            Assert.NotNull(
                compilationUnit.Imports.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "System.Linq"));
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "MyLib"));
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithClassRecordStructInterfaceAndDelegate_StaticUsings.txt")]
        public void Extract_ShouldHaveUsingsInClassModels_WhenGivenStaticUsings(string fileContent)
        {
            var compilationUnit = _factExtractor.Extract(fileContent);

            var usingModels = compilationUnit.Imports.Cast<UsingModel>().ToList();
            Assert.Equal(9, usingModels.Count);

            Assert.NotNull(usingModels.SingleOrDefault(model => model.Name == "System" && !model.IsStatic));
            Assert.NotNull(usingModels.SingleOrDefault(model =>
                model.Name == "System.Collections.Generic" && !model.IsStatic));
            Assert.NotNull(
                usingModels.SingleOrDefault(model => model.Name == "System.Linq" && !model.IsStatic));
            Assert.NotNull(
                usingModels.SingleOrDefault(model => model.Name == "System.Text" && !model.IsStatic));
            Assert.NotNull(usingModels.SingleOrDefault(model =>
                model.Name == "Microsoft.CodeAnalysis" && !model.IsStatic));
            Assert.NotNull(usingModels.SingleOrDefault(model => model.Name == "System.Math" && model.IsStatic));
            Assert.NotNull(usingModels.SingleOrDefault(model => model.Name == "MyLib" && model.IsStatic));
            Assert.NotNull(usingModels.SingleOrDefault(model => model.Name == "MyLib.Records" && model.IsStatic));
            Assert.NotNull(usingModels.SingleOrDefault(model =>
                model.Name == "Microsoft.CodeAnalysis.CSharp" && model.IsStatic));
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithAliasedNamespace.txt")]
        public void Extract_ShouldHaveUsingsInClassModels_WhenGivenAliasedNamespace(string fileContent)
        {
            var compilationUnit = _factExtractor.Extract(fileContent);

            Assert.Equal(3, compilationUnit.Imports.Count);

            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model =>
                model.Name == "System" && model.AliasType == nameof(EAliasType.None) && model.Alias == ""));
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model =>
                model.Name == "PC.MyCompany.Project" && model.AliasType == nameof(EAliasType.Namespace) &&
                model.Alias == "Project"));
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(
                model => model.Name == "MyCompany" && model.AliasType == nameof(EAliasType.Namespace) &&
                         model.Alias == "Company"));
        }


        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithAliasedClass.txt")]
        public void Extract_ShouldHaveUsingsInClassModels_WhenGivenAliasedClass(string fileContent)
        {
            var compilationUnit = _factExtractor.Extract(fileContent);

            Assert.Equal(3, compilationUnit.Imports.Count);

            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model =>
                model.Name == "System" && model.AliasType == nameof(EAliasType.None) && model.Alias == ""));
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model =>
                model.Name == "NameSpace1.MyClass" && model.AliasType == nameof(EAliasType.Class) &&
                model.Alias == "AliasToMyClass"));
            Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model =>
                model.Name == "NameSpace2.MyClass<int>" && model.AliasType == nameof(EAliasType.Class) &&
                model.Alias == "UsingAlias"));
        }
    }
}
