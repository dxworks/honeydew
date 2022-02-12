using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using DotNetGraph.Edge;
using DotNetGraph.SubGraph;
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

        var relationsSummary = Path.Combine(outputPath, relationsFolder, $"{projectName}-summary.txt");
        _textFileExporter.Export(relationsSummary, GetRelationsSummary(repositoryModel));

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

    private static string GetRelationsSummary(RepositoryModel repositoryModel)
    {
        var stringBuilder = new StringBuilder();

        var solutionsSet = new HashSet<string>();
        var projectsSet = new HashSet<string>();

        stringBuilder.AppendLine("Solution Name Conflicts");

        foreach (var solution in repositoryModel.Solutions)
        {
            var solutionName = Path.GetFileName(solution.FilePath);
            if (solutionsSet.Contains(solutionName))
            {
                stringBuilder.AppendLine($"{solutionName} | {solution.FilePath}");
            }
            else
            {
                solutionsSet.Add(solutionName);
            }
        }

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("Project Name Conflicts");

        foreach (var project in repositoryModel.Projects)
        {
            if (projectsSet.Contains(project.Name))
            {
                stringBuilder.AppendLine($"{project} | {project.FilePath}");
            }
            else
            {
                projectsSet.Add(project.Name);
            }
        }

        return stringBuilder.ToString();
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

        return graph.GenerateDotFileContent(true);
    }

    private static string GetSolutionProjectRelationsDotFile(RepositoryModel repositoryModel)
    {
        var graph = new Graph.Graph("Solution Project Relations", true)
        {
            DefaultEdgeProperties =
            {
                ArrowHead = DotEdgeArrowType.Vee
            }
        };

        foreach (var solution in repositoryModel.Solutions)
        {
            foreach (var project in solution.Projects)
            {
                graph.AddEdge(Path.GetFileName(solution.FilePath), project.Name, "");
            }
        }

        // foreach (var dotNode in graph.Nodes.Where(node => node.Identifier.EndsWith(".sln")))
        // {
        //     dotNode.FillColor = Color.Wheat;
        //     dotNode.Style = DotNodeStyle.Dashed;
        // }

        return graph.GenerateDotFileContent(true);
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

        return graph.GenerateDotFileContent(true);
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

        return graph.GenerateDotFileContent(true);
    }

    private static string GetProjectRelationsClusteredBySolutionsDotFile(RepositoryModel repositoryModel)
    {
        var graph = new Graph.Graph("Project Relations Clustered", true)
        {
            DefaultSubGraphProperties =
            {
                Color = Color.Coral,
                Style = DotSubGraphStyle.Solid
            }
        };

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

        return graph.GenerateDotFileContent(true);
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
