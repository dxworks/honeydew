using System.Collections.Generic;
using Honeydew.Processors;
using DxWorks.ScriptBee.Plugins.Honeydew.Models;
using Xunit;

namespace Honeydew.Tests.Processors;

public class RepositoryModelToCyclomaticComplexityPerFileProcessorTests
{
    private readonly RepositoryModelToCyclomaticComplexityPerFileProcessor _sut;

    public RepositoryModelToCyclomaticComplexityPerFileProcessorTests()
    {
        _sut = new RepositoryModelToCyclomaticComplexityPerFileProcessor();
    }

    [Fact]
    public void Process_ShouldReturnEmptyRepresentation_WhenRepositoryModelIsNull()
    {
        var classRelationsRepresentation = _sut.Process(null);
        Assert.Empty(classRelationsRepresentation.File.Concerns);
    }

    [Fact]
    public void Process_ShouldReturnEmptyRepresentation_WhenRepositoryModelIsEmpty()
    {
        var classRelationsRepresentation = _sut.Process(new RepositoryModel());
        Assert.Empty(classRelationsRepresentation.File.Concerns);
    }

    [Fact]
    public void Process_ShouldReturn4ConcernsWith0Strength_WhenRepositoryModelHasOneClassWithNoMethodsOrProperties()
    {
        var repositoryModel = new RepositoryModel();
        var solutionModel = new SolutionModel();
        var projectModel = new ProjectModel();
        var compilationUnitModel = new FileModel();
        var classModel = new ClassModel
        {
            FilePath = "path",
            Fields = new List<FieldModel>
            {
                new()
                {
                    Name = "MyField",
                    Modifier = "static",
                    Type = new EntityType
                    {
                        Name = "System.Int32"
                    },
                    AccessModifier = AccessModifier.Public,
                }
            }
        };
        compilationUnitModel.Entities.Add(classModel);
        projectModel.Files.Add(compilationUnitModel);
        repositoryModel.Solutions.Add(solutionModel);
        repositoryModel.Projects.Add(projectModel);

        var representation = _sut.Process(repositoryModel);
        Assert.Equal(5, representation.File.Concerns.Count);

        foreach (var concern in representation.File.Concerns)
        {
            Assert.Equal("path", concern.Entity);
            Assert.Equal("0", concern.Strength);
        }

        Assert.Equal("metric.maxCyclo", representation.File.Concerns[0].Tag);
        Assert.Equal("metric.minCyclo", representation.File.Concerns[1].Tag);
        Assert.Equal("metric.avgCyclo", representation.File.Concerns[2].Tag);
        Assert.Equal("metric.sumCyclo", representation.File.Concerns[3].Tag);
        Assert.Equal("metric.atc", representation.File.Concerns[4].Tag);
    }

    [Fact]
    public void Process_ShouldReturn4Concerns_WhenRepositoryModelHasOneClassWithOneMethodInOneFile()
    {
        var repositoryModel = new RepositoryModel();
        var solutionModel = new SolutionModel();
        var projectModel = new ProjectModel();
        var compilationUnitModel = new FileModel();
        var classModel = new ClassModel
        {
            FilePath = "path",
            Methods = new List<MethodModel>
            {
                new()
                {
                    CyclomaticComplexity = 12
                }
            }
        };
        compilationUnitModel.Entities.Add(classModel);
        projectModel.Files.Add(compilationUnitModel);
        repositoryModel.Projects.Add(projectModel);
        repositoryModel.Solutions.Add(solutionModel);

        var representation = _sut.Process(repositoryModel);
        Assert.Equal(5, representation.File.Concerns.Count);

        for (var i = 0; i < representation.File.Concerns.Count - 1; i++)
        {
            var concern = representation.File.Concerns[i];
            Assert.Equal("path", concern.Entity);
            Assert.Equal("12", concern.Strength);
        }

        Assert.Equal("path", representation.File.Concerns[4].Entity);
        Assert.Equal("1", representation.File.Concerns[4].Strength);

        Assert.Equal("metric.maxCyclo", representation.File.Concerns[0].Tag);
        Assert.Equal("metric.minCyclo", representation.File.Concerns[1].Tag);
        Assert.Equal("metric.avgCyclo", representation.File.Concerns[2].Tag);
        Assert.Equal("metric.sumCyclo", representation.File.Concerns[3].Tag);
        Assert.Equal("metric.atc", representation.File.Concerns[4].Tag);
    }

    [Fact]
    public void Process_ShouldReturn4Concerns_WhenRepositoryModelHasMultipleClassesWithOneMethodInOneFile()
    {
        var repositoryModel = new RepositoryModel();
        var solutionModel = new SolutionModel();
        var projectModel = new ProjectModel();
        var compilationUnitModel = new FileModel();
        var classModel1 = new ClassModel
        {
            FilePath = "path",
            Methods = new List<MethodModel>
            {
                new()
                {
                    CyclomaticComplexity = 12
                }
            }
        };
        var classModel2 = new ClassModel
        {
            FilePath = "path",
            Methods = new List<MethodModel>
            {
                new()
                {
                    CyclomaticComplexity = 6
                }
            }
        };

        compilationUnitModel.Entities.Add(classModel1);
        compilationUnitModel.Entities.Add(classModel2);
        projectModel.Files.Add(compilationUnitModel);
        repositoryModel.Projects.Add(projectModel);
        repositoryModel.Solutions.Add(solutionModel);

        var representation = _sut.Process(repositoryModel);
        Assert.Equal(5, representation.File.Concerns.Count);

        foreach (var concern in representation.File.Concerns)
        {
            Assert.Equal("path", concern.Entity);
        }

        Assert.Equal("metric.maxCyclo", representation.File.Concerns[0].Tag);
        Assert.Equal("12", representation.File.Concerns[0].Strength);

        Assert.Equal("metric.minCyclo", representation.File.Concerns[1].Tag);
        Assert.Equal("6", representation.File.Concerns[1].Strength);

        Assert.Equal("metric.avgCyclo", representation.File.Concerns[2].Tag);
        Assert.Equal("9", representation.File.Concerns[2].Strength);

        Assert.Equal("metric.sumCyclo", representation.File.Concerns[3].Tag);
        Assert.Equal("18", representation.File.Concerns[3].Strength);

        Assert.Equal("metric.atc", representation.File.Concerns[4].Tag);
        Assert.Equal("1", representation.File.Concerns[4].Strength);
    }

    [Fact]
    public void Process_ShouldReturn4Concerns_WhenRepositoryModelHasOneClassWithMultipleMethodInOneFile()
    {
        var repositoryModel = new RepositoryModel();
        var solutionModel = new SolutionModel();
        var projectModel = new ProjectModel();
        var compilationUnitModel = new FileModel();
        var classModel1 = new ClassModel
        {
            FilePath = "path",
            Methods = new List<MethodModel>
            {
                new()
                {
                    CyclomaticComplexity = 12
                },
                new()
                {
                    CyclomaticComplexity = 7
                },
                new()
                {
                    CyclomaticComplexity = 1
                }
            }
        };

        compilationUnitModel.Entities.Add(classModel1);
        projectModel.Files.Add(compilationUnitModel);
        repositoryModel.Projects.Add(projectModel);
        repositoryModel.Solutions.Add(solutionModel);

        var representation = _sut.Process(repositoryModel);
        Assert.Equal(5, representation.File.Concerns.Count);

        foreach (var concern in representation.File.Concerns)
        {
            Assert.Equal("path", concern.Entity);
        }

        Assert.Equal("metric.maxCyclo", representation.File.Concerns[0].Tag);
        Assert.Equal("12", representation.File.Concerns[0].Strength);

        Assert.Equal("metric.minCyclo", representation.File.Concerns[1].Tag);
        Assert.Equal("1", representation.File.Concerns[1].Strength);

        Assert.Equal("metric.avgCyclo", representation.File.Concerns[2].Tag);
        Assert.Equal("6", representation.File.Concerns[2].Strength);

        Assert.Equal("metric.sumCyclo", representation.File.Concerns[3].Tag);
        Assert.Equal("20", representation.File.Concerns[3].Strength);

        Assert.Equal("metric.atc", representation.File.Concerns[4].Tag);
        Assert.Equal("1", representation.File.Concerns[4].Strength);
    }

    [Fact]
    public void
        Process_ShouldReturn4Concerns_WhenRepositoryModelHasMultipleClassesWithInMultipleFiles()
    {
        var repositoryModel = new RepositoryModel();
        var solutionModel = new SolutionModel();
        var projectModel = new ProjectModel();
        var compilationUnitModel = new FileModel();
        var classModel1 = new InterfaceModel
        {
            FilePath = "path1",
            Methods = new List<MethodModel>
            {
                new()
                {
                    CyclomaticComplexity = 12
                },
                new()
                {
                    CyclomaticComplexity = 8
                }
            }
        };
        var classModel2 = new ClassModel
        {
            FilePath = "path2",
            Properties = new List<PropertyModel>
            {
                new()
                {
                    CyclomaticComplexity = 1
                }
            },
            Methods = new List<MethodModel>
            {
                new()
                {
                    CyclomaticComplexity = 6
                },
            },
            Constructors = new List<MethodModel>
            {
                new()
                {
                    CyclomaticComplexity = 1,
                    ReturnValue = null,
                    Type = MethodType.Constructor,
                }
            } 
        };

        compilationUnitModel.Entities.Add(classModel1);
        compilationUnitModel.Entities.Add(classModel2);
        projectModel.Files.Add(compilationUnitModel);
        repositoryModel.Projects.Add(projectModel);
        repositoryModel.Solutions.Add(solutionModel);

        var representation = _sut.Process(repositoryModel);
        Assert.Equal(10, representation.File.Concerns.Count);


        Assert.Equal("path1", representation.File.Concerns[0].Entity);
        Assert.Equal("metric.maxCyclo", representation.File.Concerns[0].Tag);
        Assert.Equal("12", representation.File.Concerns[0].Strength);

        Assert.Equal("path1", representation.File.Concerns[1].Entity);
        Assert.Equal("metric.minCyclo", representation.File.Concerns[1].Tag);
        Assert.Equal("8", representation.File.Concerns[1].Strength);

        Assert.Equal("path1", representation.File.Concerns[2].Entity);
        Assert.Equal("metric.avgCyclo", representation.File.Concerns[2].Tag);
        Assert.Equal("10", representation.File.Concerns[2].Strength);

        Assert.Equal("path1", representation.File.Concerns[3].Entity);
        Assert.Equal("metric.sumCyclo", representation.File.Concerns[3].Tag);
        Assert.Equal("20", representation.File.Concerns[3].Strength);

        Assert.Equal("path1", representation.File.Concerns[4].Entity);
        Assert.Equal("metric.atc", representation.File.Concerns[4].Tag);
        Assert.Equal("1", representation.File.Concerns[4].Strength);

        
        Assert.Equal("path2", representation.File.Concerns[5].Entity);
        Assert.Equal("metric.maxCyclo", representation.File.Concerns[5].Tag);
        Assert.Equal("6", representation.File.Concerns[5].Strength);

        Assert.Equal("path2", representation.File.Concerns[6].Entity);
        Assert.Equal("metric.minCyclo", representation.File.Concerns[6].Tag);
        Assert.Equal("1", representation.File.Concerns[6].Strength);

        Assert.Equal("path2", representation.File.Concerns[7].Entity);
        Assert.Equal("metric.avgCyclo", representation.File.Concerns[7].Tag);
        Assert.Equal("2", representation.File.Concerns[7].Strength);

        Assert.Equal("path2", representation.File.Concerns[8].Entity);
        Assert.Equal("metric.sumCyclo", representation.File.Concerns[8].Tag);
        Assert.Equal("8", representation.File.Concerns[8].Strength);

        Assert.Equal("path2", representation.File.Concerns[9].Entity);
        Assert.Equal("metric.atc", representation.File.Concerns[9].Tag);
        Assert.Equal("0", representation.File.Concerns[9].Strength);
    }
}
