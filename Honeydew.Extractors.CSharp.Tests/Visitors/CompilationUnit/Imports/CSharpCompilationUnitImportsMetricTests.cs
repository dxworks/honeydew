using System.Collections.Generic;
using System.Linq;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Models;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.CompilationUnit.Imports;

public class CSharpCompilationUnitImportsMetricTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpCompilationUnitImportsMetricTests()
    {
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new ImportsVisitor()
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
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

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var compilationUnit = _factExtractor.Extract(syntaxTree, semanticModel);

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
    [FileData("TestData/CompilationUnitWithOneDelegate.txt")]
    [FileData("TestData/CompilationUnitWithClassRecordStructInterfaceAndDelegate_DeclaredInside.txt")]
    [FileData("TestData/CompilationUnitWithClassRecordStructInterfaceAndDelegate.txt")]
    [FileData("TestData/CompilationUnitWithClassRecordStructInterfaceAndDelegate_SharedUsings.txt")]
    public void Extract_ShouldHaveUsings_WhenCompilationUnitText(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var compilationUnit = _factExtractor.Extract(syntaxTree, semanticModel);

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
    [FileData("TestData/CompilationUnitWithRecordStructInterfaceAndDelegate_ImbricatedNamespaces.txt")]
    [FileData("TestData/CompilationUnitWithClassRecordStructInterfaceAndDelegate_SharedUsings_MultipleLevels.txt")]
    public void Extract_ShouldHaveUsings_WhenGivenMultipleClassesAndDelegatesInImbricatedNamespaces(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var compilationUnit = _factExtractor.Extract(syntaxTree, semanticModel);

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
    [FileData("TestData/CompilationUnitWithInnerClasses.txt")]
    public void Extract_ShouldHaveUsings_WhenGivenMultipleClassesWithInnerClasses(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var compilationUnit = _factExtractor.Extract(syntaxTree, semanticModel);

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
    [FileData("TestData/CompilationUnitWithInnerClasses_ButNoNamespace.txt")]
    public void Extract_ShouldHaveUsings_WhenGivenMultipleClassesWithInnerClasses_ButNoNamespace(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var compilationUnit = _factExtractor.Extract(syntaxTree, semanticModel);

        Assert.Equal(4, compilationUnit.Imports.Count);
        Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "System"));
        Assert.NotNull(
            compilationUnit.Imports.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
        Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "System.Linq"));
        Assert.NotNull(compilationUnit.Imports.SingleOrDefault(model => model.Name == "MyLib"));
    }

    [Theory]
    [FileData("TestData/CompilationUnitWithClassRecordStructInterfaceAndDelegate_StaticUsings.txt")]
    public void Extract_ShouldHaveUsingsInClassModels_WhenGivenStaticUsings(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var compilationUnit = _factExtractor.Extract(syntaxTree, semanticModel);

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
    [FileData("TestData/CompilationUnitWithAliasedNamespace.txt")]
    public void Extract_ShouldHaveUsingsInClassModels_WhenGivenAliasedNamespace(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var compilationUnit = _factExtractor.Extract(syntaxTree, semanticModel);

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
    [FileData("TestData/CompilationUnitWithAliasedClass.txt")]
    public void Extract_ShouldHaveUsingsInClassModels_WhenGivenAliasedClass(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var compilationUnit = _factExtractor.Extract(syntaxTree, semanticModel);

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
