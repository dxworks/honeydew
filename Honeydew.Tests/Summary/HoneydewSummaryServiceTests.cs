using System;
using System.IO;
using System.Text.Json;
using Honeydew.Models;
using Honeydew.Models.CSharp;
using Honeydew.Models.VisualBasic;
using Honeydew.Summary;
using Xunit;

namespace Honeydew.Tests.Summary;

public class HoneydewSummaryServiceTests
{
    [Fact]
    public void BuildSummaryData_ShouldReturnBalancedCounters_WhenRepositoryContainsCSharpAndVisualBasic()
    {
        var repositoryModel = CreateRepositoryModel();

        var summaryData = HoneydewSummaryService.BuildSummaryData("demo", repositoryModel);

        Assert.Equal(HoneydewSummaryService.PartialStatus, summaryData.Status);
        Assert.Equal("demo", summaryData.ProjectName);
        Assert.Equal(1, summaryData.SolutionsCount);
        Assert.Equal(2, summaryData.ProjectsCount);
        Assert.Equal(1, summaryData.ProjectsCSharpCount);
        Assert.Equal(1, summaryData.ProjectsVisualBasicCount);
        Assert.Equal(2, summaryData.FilesCount);
        Assert.Equal(1, summaryData.FilesCSharpCount);
        Assert.Equal(1, summaryData.FilesVisualBasicCount);
        Assert.Equal(4, summaryData.TopLevelClassesCount);
        Assert.Equal(2, summaryData.InterfacesCount);
        Assert.Equal(2, summaryData.AbstractClassesCount);
        Assert.Equal(1, summaryData.UnprocessedProjectsCount);
        Assert.Equal(1, summaryData.UnprocessedSourceFilesCount);
        Assert.Equal(18, summaryData.SourceCodeLines);
    }

    [Fact]
    public void WriteSummaryData_ShouldPersistSummaryDataFile_WhenSummaryIsGenerated()
    {
        var repositoryModel = CreateRepositoryModel();
        var summaryData = HoneydewSummaryService.BuildSummaryData("demo", repositoryModel);
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        try
        {
            var summaryDataPath = HoneydewSummaryService.WriteSummaryData(temporaryDirectory, summaryData);
            Assert.True(File.Exists(summaryDataPath));

            var serialized = File.ReadAllText(summaryDataPath);
            var parsed = JsonSerializer.Deserialize<HoneydewSummaryData>(serialized);
            Assert.NotNull(parsed);
            Assert.Equal(summaryData.Status, parsed!.Status);
            Assert.Equal(summaryData.TopLevelClassesCount, parsed.TopLevelClassesCount);
            Assert.Equal(summaryData.FilesCount, parsed.FilesCount);
        }
        finally
        {
            Directory.Delete(temporaryDirectory, true);
        }
    }

    private static RepositoryModel CreateRepositoryModel()
    {
        var csharpCompilationUnit = new CSharpCompilationUnitModel
        {
            FilePath = "src/Alpha.cs",
            Loc = new LinesOfCode
            {
                SourceLines = 12,
            },
            ClassTypes =
            {
                new CSharpClassModel
                {
                    ClassType = "class",
                    Modifier = "abstract",
                },
                new CSharpClassModel
                {
                    ClassType = "interface",
                },
                new CSharpClassModel
                {
                    ClassType = "class",
                    ContainingClassName = "NestedInsideAlpha",
                },
            },
        };

        var visualBasicCompilationUnit = new VisualBasicCompilationUnitModel
        {
            FilePath = "src/Beta.vb",
            Loc = new LinesOfCode
            {
                SourceLines = 6,
            },
            ClassTypes =
            {
                new VisualBasicClassModel
                {
                    ClassType = "class",
                    Modifier = "MustInherit",
                },
                new VisualBasicClassModel
                {
                    ClassType = "interface",
                },
            },
        };

        var repositoryModel = new RepositoryModel();
        repositoryModel.Solutions.Add(new SolutionModel
        {
            FilePath = "demo.sln",
        });
        repositoryModel.Projects.Add(new ProjectModel
        {
            Name = "Alpha",
            Language = ProjectExtractorFactory.CSharp,
            CompilationUnits = { csharpCompilationUnit },
        });
        repositoryModel.Projects.Add(new ProjectModel
        {
            Name = "Beta",
            Language = ProjectExtractorFactory.VisualBasic,
            CompilationUnits = { visualBasicCompilationUnit },
        });
        repositoryModel.UnprocessedProjects.Add(new ProjectModel
        {
            Name = "UnprocessedProject",
        });
        repositoryModel.UnprocessedSourceFiles.Add(new ProjectModel
        {
            Name = "UnprocessedSourceFile",
        });

        return repositoryModel;
    }
}
