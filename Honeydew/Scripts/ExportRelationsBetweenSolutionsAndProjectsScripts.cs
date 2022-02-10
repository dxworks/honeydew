using System.Collections.Generic;
using System.IO;
using HoneydewCore.IO.Writers.Exporters;
using HoneydewCore.ModelRepresentations;
using HoneydewModels.Reference;

namespace Honeydew.Scripts;

/// <summary>
/// Requires the following arguments:
/// <list type="bullet">
///     <item>
///         <description>outputPath</description>
///     </item>
///     <item>
///         <description>projectName</description>
///     </item>
///     <item>
///         <description>referenceRepositoryModel</description>
///     </item>
/// </list>
/// </summary>
public class ExportRelationsBetweenSolutionsAndProjectsScripts : Script
{
    private readonly CsvRelationsRepresentationExporter _representationExporter;
    private readonly TextFileExporter _textFileExporter;

    public ExportRelationsBetweenSolutionsAndProjectsScripts(CsvRelationsRepresentationExporter representationExporter,
        TextFileExporter textFileExporter)
    {
        _representationExporter = representationExporter;
        _textFileExporter = textFileExporter;
    }

    public override void Run(Dictionary<string, object> arguments)
    {
        var outputPath = VerifyArgument<string>(arguments, "outputPath");
        var projectName = VerifyArgument<string>(arguments, "projectName");
        var repositoryModel = VerifyArgument<RepositoryModel>(arguments, "referenceRepositoryModel");

        const string relationsFolder = "relations";

        var projectRelationsCsvPath = Path.Combine(outputPath, relationsFolder, $"{projectName}-project_relations.csv");
        _representationExporter.Export(projectRelationsCsvPath, GetProjectRelations(repositoryModel));

        var solutionProjectsRelationsCsvPath =
            Path.Combine(outputPath, relationsFolder, $"{projectName}-solution_project_relations.csv");
        _representationExporter.Export(solutionProjectsRelationsCsvPath, GetSolutionProjectRelations(repositoryModel));


        var projectRelationsDotFilePath =
            Path.Combine(outputPath, relationsFolder, $"{projectName}-project_relations.dot");
        _textFileExporter.Export(projectRelationsDotFilePath, GetProjectRelationsDotFile(repositoryModel));

        var solutionProjectsRelationsDotFilePath = Path.Combine(outputPath, relationsFolder,
            $"{projectName}-solution_project_relations.dot");
        _textFileExporter.Export(solutionProjectsRelationsDotFilePath,
            GetSolutionProjectRelationsDotFile(repositoryModel));

        var solutionProjectsClustersDotFilePath =
            Path.Combine(outputPath, relationsFolder, $"{projectName}-solution_project_clusters.dot");
        _textFileExporter.Export(solutionProjectsClustersDotFilePath,
            GetSolutionProjectClustersDotFile(repositoryModel));

        var solutionProjectAllRelationsDotFilePath = Path.Combine(outputPath, relationsFolder,
            $"{projectName}-solution_project_all_relations.dot");
        _textFileExporter.Export(solutionProjectAllRelationsDotFilePath,
            GetSolutionProjectAllRelationsDotFile(repositoryModel));

        var projectRelationsClusteredBySolutionDotFilePath = Path.Combine(outputPath, relationsFolder,
            $"{projectName}-project_relations_clustered_relations.dot");
        _textFileExporter.Export(projectRelationsClusteredBySolutionDotFilePath,
            GetProjectRelationsClusteredBySolutionsDotFile(repositoryModel));
    }

    private static string GetProjectRelationsDotFile(RepositoryModel repositoryModel)
    {
        var graph = new Graph.Graph("Project Relations", true);

        foreach (var project in repositoryModel.Projects)
        {
            foreach (var reference in project.ProjectReferences)
            {
                graph.AddEdge(project.Name, reference.Name, "");
            }
        }

        return graph.GenerateDotFileContent();
    }

    private static string GetSolutionProjectRelationsDotFile(RepositoryModel repositoryModel)
    {
        var graph = new Graph.Graph("Solution Project Relations", true);

        foreach (var solution in repositoryModel.Solutions)
        {
            foreach (var project in solution.Projects)
            {
                graph.AddEdge(Path.GetFileName(solution.FilePath), project.Name, "");
            }
        }

        return graph.GenerateDotFileContent();
    }

    private static string GetSolutionProjectClustersDotFile(RepositoryModel repositoryModel)
    {
        var graph = new Graph.Graph("Solution Project Relations", true);

        foreach (var solution in repositoryModel.Solutions)
        {
            var subGraph = graph.AddSubGraph(Path.GetFileNameWithoutExtension(solution.FilePath));
            foreach (var project in solution.Projects)
            {
                graph.AddNodeToSubGraph(subGraph, project.Name);
            }
        }

        return graph.GenerateDotFileContent();
    }

    private static string GetSolutionProjectAllRelationsDotFile(RepositoryModel repositoryModel)
    {
        var graph = new Graph.Graph("Solution Project All Relations", true);

        foreach (var solution in repositoryModel.Solutions)
        {
            foreach (var project in solution.Projects)
            {
                graph.AddEdge(Path.GetFileName(solution.FilePath), project.Name, "");
            }
        }

        foreach (var project in repositoryModel.Projects)
        {
            foreach (var reference in project.ProjectReferences)
            {
                graph.AddEdge(project.Name, reference.Name, "");
            }
        }

        return graph.GenerateDotFileContent();
    }

    private static string GetProjectRelationsClusteredBySolutionsDotFile(RepositoryModel repositoryModel)
    {
        var graph = new Graph.Graph("Project Relations Clustered", true);

        foreach (var solution in repositoryModel.Solutions)
        {
            var subGraph = graph.AddSubGraph(Path.GetFileNameWithoutExtension(solution.FilePath));
            foreach (var project in solution.Projects)
            {
                foreach (var reference in project.ProjectReferences)
                {
                    graph.AddEdgeToSubGraph(subGraph, project.Name, reference.Name, "");
                }
            }
        }

        return graph.GenerateDotFileContent();
    }

    private static RelationsRepresentation GetProjectRelations(RepositoryModel repositoryModel)
    {
        var relationsRepresentation = new RelationsRepresentation();

        foreach (var project in repositoryModel.Projects)
        {
            foreach (var reference in project.ProjectReferences)
            {
                relationsRepresentation.Add(project.Name, reference.Name, "Value", 1);
            }
        }

        return relationsRepresentation;
    }

    private static RelationsRepresentation GetSolutionProjectRelations(RepositoryModel repositoryModel)
    {
        var relationsRepresentation = new RelationsRepresentation();

        foreach (var solution in repositoryModel.Solutions)
        {
            foreach (var project in solution.Projects)
            {
                relationsRepresentation.Add(Path.GetFileNameWithoutExtension(solution.FilePath), project.Name, "Value",
                    1);
            }
        }

        return relationsRepresentation;
    }
}
