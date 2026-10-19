using DotNetGraph.Core;
using DotNetGraph.Extensions;
using DotNetGraph.Compilation;
using DotNetGraph.Exceptions;

namespace Honeydew.Scripts.Graph;

public class Graph
{
    public DotEdge DefaultEdgeProperties { get; } = new();
    public DotNode DefaultNodeProperties { get; } = new();
    public DotSubgraph DefaultSubGraphProperties { get; } = new();

    private readonly IDictionary<string, DotNode> _nodes = new Dictionary<string, DotNode>();
    private readonly IDictionary<(string, string), DotEdge> _edges = new Dictionary<(string, string), DotEdge>();
    private readonly IDictionary<string, DotSubgraph> _subGraphs = new Dictionary<string, DotSubgraph>();
    private readonly DotGraph _graph;

    public IEnumerable<DotNode> Nodes => _nodes.Values;
    public IEnumerable<DotEdge> Edges => _edges.Values;
    public IEnumerable<DotSubgraph> SubGraphs => _subGraphs.Values;

    public Graph(string name, bool directed = false)
    {
        _graph = new DotGraph
        {
            Identifier = new DotIdentifier(name, false, true),
            Directed = directed
        };
    }

    public string GenerateDotFileContent(bool indented = false)
    {
        using var stringWriter = new StringWriter();
        var context = new CompilationContext(stringWriter, new CompilationOptions
        {
            Indented = indented
        });

        _graph.CompileAsync(context).GetAwaiter().GetResult();
        return stringWriter.ToString();
    }

    public DotNode AddNode(string nodeId, string? label = null)
    {
        if (_nodes.TryGetValue(nodeId, out var node))
        {
            return node;
        }

        node = CreateNode(nodeId, label);

        _nodes.Add(nodeId, node);
        _graph.Elements.Add(node);

        return node;
    }

    public DotEdge AddEdge(string leftNodeId, string rightNodeId, string? label = null)
    {
        if (_edges.TryGetValue((leftNodeId, rightNodeId), out var edge))
        {
            return edge;
        }

        edge = CreateEdge(leftNodeId, rightNodeId, label);

        _edges.Add((leftNodeId, rightNodeId), edge);
        _graph.Elements.Add(edge);

        return edge;
    }

    public DotSubgraph AddSubGraph(string subGraphName, string? label = null)
    {
        if (_subGraphs.TryGetValue(subGraphName, out var subGraph))
        {
            return subGraph;
        }

        subGraph = new DotSubgraph
        {
            Identifier = new DotIdentifier(subGraphName, false, true),
            Label = label ?? subGraphName,
        };

        TryApply(() => subGraph.Color = DefaultSubGraphProperties.Color);
        TryApply(() => subGraph.Style = DefaultSubGraphProperties.Style);

        _subGraphs.Add(subGraphName, subGraph);
        _graph.Elements.Add(subGraph);

        return subGraph;
    }

    public DotNode GetNode(string nodeId)
    {
        return _nodes[nodeId];
    }

    public DotEdge GetEdge(string leftNodeId, string rightNodeId)
    {
        return _edges[(leftNodeId, rightNodeId)];
    }

    public DotSubgraph GetSubGraph(string subGraphId)
    {
        return _subGraphs[subGraphId];
    }

    public void AddNodeToSubGraph(DotSubgraph subGraph, string nodeId, string? nodeLabel = null)
    {
        var node = CreateNode(nodeId, nodeLabel);
        subGraph.Elements.Add(node);

        _nodes.TryAdd(nodeId, node);
    }

    public void AddEdgeToSubGraph(DotSubgraph subGraph, string leftNodeId, string rightNodeId, string? label = null)
    {
        var edge = CreateEdge(leftNodeId, rightNodeId, label);
        subGraph.Elements.Add(edge);

        _edges.TryAdd((leftNodeId, rightNodeId), edge);
    }

    private DotNode CreateNode(string nodeId, string? label = null)
    {
        var node = new DotNode
        {
            Identifier = new DotIdentifier(nodeId, false, true),
            Label = label ?? nodeId,
        };

        TryApply(() => node.Height = DefaultNodeProperties.Height);
        TryApply(() => node.Width = DefaultNodeProperties.Width);
        TryApply(() => node.Shape = DefaultNodeProperties.Shape);
        TryApply(() => node.Color = DefaultNodeProperties.Color);
        TryApply(() => node.FillColor = DefaultNodeProperties.FillColor);
        TryApply(() => node.FontColor = DefaultNodeProperties.FontColor);
        TryApply(() => node.Style = DefaultNodeProperties.Style);
        TryApply(() => node.PenWidth = DefaultNodeProperties.PenWidth);

        return node;
    }

    private DotEdge CreateEdge(string leftNodeId, string rightNodeId, string? label = null)
    {
        var leftNode = AddNode(leftNodeId);
        var rightNode = AddNode(rightNodeId);

        var edge = new DotEdge
        {
            From = leftNode.Identifier,
            To = rightNode.Identifier,
            Label = label ?? $"{leftNodeId}-{rightNodeId}",
        };

        TryApply(() => edge.ArrowHead = DefaultEdgeProperties.ArrowHead);
        TryApply(() => edge.ArrowTail = DefaultEdgeProperties.ArrowTail);
        TryApply(() => edge.Color = DefaultEdgeProperties.Color);
        TryApply(() => edge.FontColor = DefaultEdgeProperties.FontColor);
        TryApply(() => edge.Style = DefaultEdgeProperties.Style);
        TryApply(() => edge.PenWidth = DefaultEdgeProperties.PenWidth);

        return edge;
    }

    private static void TryApply(Action action)
    {
        try
        {
            action();
        }
        catch (AttributeNotFoundException)
        {
        }
    }
}
