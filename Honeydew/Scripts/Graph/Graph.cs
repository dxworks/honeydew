using System.Collections.Generic;
using DotNetGraph;
using DotNetGraph.Edge;
using DotNetGraph.Extensions;
using DotNetGraph.Node;
using DotNetGraph.SubGraph;

namespace Honeydew.Scripts.Graph;

public class Graph
{
    public DotEdge DefaultEdgeProperties { get; } = new("_", "_");
    public DotNode DefaultNodeProperties { get; } = new("_");
    public DotSubGraph DefaultSubGraphProperties { get; } = new("_");

    private readonly IDictionary<string, DotNode> _nodes = new Dictionary<string, DotNode>();
    private readonly IDictionary<(string, string), DotEdge> _edges = new Dictionary<(string, string), DotEdge>();
    private readonly IDictionary<string, DotSubGraph> _subGraphs = new Dictionary<string, DotSubGraph>();
    private readonly DotGraph _graph;

    public IEnumerable<DotNode> Nodes => _nodes.Values;
    public IEnumerable<DotEdge> Edges => _edges.Values;
    public IEnumerable<DotSubGraph> SubGraphs => _subGraphs.Values;

    public Graph(string name, bool directed = false)
    {
        _graph = new DotGraph(name, directed);
    }

    public string GenerateDotFileContent(bool indented = false)
    {
        return _graph.Compile(indented);
    }

    public DotNode AddNode(string nodeId, string label = null)
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

    public DotEdge AddEdge(string leftNodeId, string rightNodeId, string label = null)
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

    public DotSubGraph AddSubGraph(string subGraphName, string label = null)
    {
        if (_subGraphs.TryGetValue(subGraphName, out var subGraph))
        {
            return subGraph;
        }

        subGraph = new DotSubGraph(subGraphName)
        {
            Color = DefaultSubGraphProperties.Color,
            Style = DefaultSubGraphProperties.Style,
            Label = label ?? subGraphName,
        };

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

    public DotSubGraph GetSubGraph(string subGraphId)
    {
        return _subGraphs[subGraphId];
    }

    public void AddNodeToSubGraph(DotSubGraph subGraph, string nodeId, string nodeLabel = null)
    {
        var node = CreateNode(nodeId, nodeLabel);
        subGraph.Elements.Add(node);

        _nodes.TryAdd(nodeId, node);
    }

    public void AddEdgeToSubGraph(DotSubGraph subGraph, string leftNodeId, string rightNodeId, string label = null)
    {
        var edge = CreateEdge(leftNodeId, rightNodeId, label);
        subGraph.Elements.Add(edge);

        _edges.TryAdd((leftNodeId, rightNodeId), edge);
    }

    private DotNode CreateNode(string nodeId, string label = null)
    {
        return new DotNode(nodeId)
        {
            Label = label ?? nodeId,
            Height = DefaultNodeProperties.Height,
            Width = DefaultNodeProperties.Width,
            Shape = DefaultNodeProperties.Shape,
            Color = DefaultNodeProperties.Color,
            FillColor = DefaultNodeProperties.FillColor,
            FontColor = DefaultNodeProperties.FontColor,
            Style = DefaultNodeProperties.Style,
            PenWidth = DefaultNodeProperties.PenWidth,
        };
    }

    private DotEdge CreateEdge(string leftNodeId, string rightNodeId, string label = null)
    {
        var leftNode = AddNode(leftNodeId);
        var rightNode = AddNode(rightNodeId);

        return new DotEdge(leftNode, rightNode)
        {
            ArrowHead = DefaultEdgeProperties.ArrowHead,
            ArrowTail = DefaultEdgeProperties.ArrowTail,
            Color = DefaultEdgeProperties.Color,
            FontColor = DefaultEdgeProperties.FontColor,
            Style = DefaultEdgeProperties.Style,
            PenWidth = DefaultEdgeProperties.PenWidth,
            Label = label ?? $"{leftNodeId}-{rightNodeId}",
        };
    }
}
