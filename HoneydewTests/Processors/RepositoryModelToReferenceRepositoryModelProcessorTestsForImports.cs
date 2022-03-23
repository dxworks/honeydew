using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewModels.CSharp;
using HoneydewScriptBeePlugin.Loaders;
using Moq;
using Xunit;

namespace HoneydewTests.Processors;

public class RepositoryModelToReferenceRepositoryModelProcessorTestsForImports
{
    private readonly RepositoryModelToReferenceRepositoryModelProcessor _sut;

    private readonly CSharpFactExtractor _extractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public RepositoryModelToReferenceRepositoryModelProcessorTestsForImports()
    {
        _sut = new RepositoryModelToReferenceRepositoryModelProcessor();

        var compositeVisitor = new CompositeVisitor();


        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
        {
            new BaseInfoClassVisitor(),
            new BaseTypesClassVisitor(),
            new ImportsVisitor(),
        }));

        compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

        _extractor = new CSharpFactExtractor(compositeVisitor);
    }


    [Theory]
    [FileData("TestData/Processors/ReferenceOfNamespaces.txt")]
    public void GetFunction_ShouldReturnImportNamespaceReference_WhenGivenClassWithImports(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _extractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var repositoryModel = new RepositoryModel
        {
            Projects =
            {
                new ProjectModel
                {
                    Name = "Project1",
                    CompilationUnits =
                    {
                        new CompilationUnitModel
                        {
                            FilePath = "Project1.Services",
                            ClassTypes = classTypes
                        }
                    }
                }
            }
        };


        var referenceSolutionModel = _sut.Process(repositoryModel);

        var classModel = referenceSolutionModel.Projects[0].Files[0].Entities[0];

        Assert.Equal("Namespace1", classModel.Namespace.Name);
        Assert.Equal(3, classModel.Imports.Count);

        Assert.Equal("Namespace2.SubNamespace", classModel.Imports[0].Namespace!.FullName);
        Assert.Equal("Namespace2", classModel.Imports[0].Namespace!.Parent!.FullName);
        Assert.Equal(referenceSolutionModel.Namespaces[1], classModel.Imports[0].Namespace!.Parent);

        Assert.Equal("Namespace3.N1.N2.MyClass3", classModel.Imports[1].Entity!.Name);
        Assert.Equal(referenceSolutionModel.Namespaces[2], classModel.Imports[1].Entity!.Namespace.Parent!.Parent);

        Assert.Equal("Namespace2.SubNamespace", classModel.Imports[2].Entity!.Namespace.FullName);
        Assert.Equal("Namespace2.SubNamespace.MyClass2", classModel.Imports[2].Entity!.Name);
        Assert.Equal(referenceSolutionModel.Namespaces[1], classModel.Imports[2].Entity!.Namespace.Parent);
    }
}
