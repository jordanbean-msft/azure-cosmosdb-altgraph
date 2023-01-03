using altgraph_shared_app.Services.Graph.v2.Structs;
using Microsoft.Extensions.Logging;
using QuikGraph;

namespace altgraph_shared_app.Services.Graph.v2
{
  public class JGraph
  {
    public string Domain { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;

    public DateTime RefreshDate { get; set; }
    public long RefreshMs { get; set; }
    private readonly ILogger<JGraph> _logger;

    public BidirectionalGraph<string, Edge<VertexValueStruct>> Graph { get; private set; } = new BidirectionalGraph<string, Edge<VertexValueStruct>>();

    public JGraph(string domain, string source, ILogger<JGraph> logger)
    {
      Domain = domain;
      Source = source;
      _logger = logger;
      Refresh();
    }

    public int[] GetVertexAndEdgeCounts()
    {
      int[] counts = new int[2];
      counts[0] = Graph.Vertices.Count();
      counts[1] = Graph.Edges.Count();
      return counts;
    }

    /**
     * Find the shortest path with the DijkstraShortestPath class in JGraphT.
     */
    public GraphPath<string, DefaultEdge> GetShortestPath(string v1, string v2)
    {
      _logger.LogWarning($"getShortestPath, v1: {v1} to v2: {v2}");
      //long start = System.currentTimeMillis();
      if (!IsVertexPresent(v1))
      {
        return null;
      }
      if (!IsVertexPresent(v2))
      {
        return null;
      }
      GraphPath<string, DefaultEdge> path =
              DijkstraShortestPath.findPathBetween(Graph, v1, v2);
      //long elapsed = System.currentTimeMillis() - start;

      if (path == null)
      {
        _logger.LogWarning("path is null");
      }
      else
      {
        //_logger.LogWarning("elapsed milliseconds: " + elapsed);
        _logger.LogWarning("path getLength:       " + path.getLength());
        _logger.LogWarning("path getStartVertex:  " + path.getStartVertex());
        _logger.LogWarning("path getEndVertex:    " + path.getEndVertex());
      }
      return path;
    }


    public EdgesStruct GetShortestPathAsEdgesStruct(string v1, string v2)
    {
      //long startMs = System.currentTimeMillis();
      GraphPath<string, DefaultEdge> path = GetShortestPath(v1, v2);

      if (path != null)
      {
        EdgesStruct edgesStruct = new EdgesStruct();
        //edgesStruct.ElapsedMs = (System.currentTimeMillis() - startMs);
        edgesStruct.Vertex1 = v1;
        edgesStruct.Vertex2 = v2;
        foreach (DefaultEdge e : path.GetEdgeList())
        {
          EdgeStruct edgeStruct = ParseDefaultEdge(e);
          edgesStruct.AddEdge(edgeStruct);
        }
        return edgesStruct;
      }
      else
      {
        return null;
      }
    }

    public Set<DefaultEdge> EdgesOf(string v)
    {
      if (IsVertexPresent(v))
      {
        return graph.edgesOf(v);
      }
      return null;
    }

    public JStarNetwork StarNetworkFor(string rootVertex, int degrees)
    {
      JStarNetwork? star = null;
      if (IsVertexPresent(rootVertex))
      {
        star = new JStarNetwork(rootVertex, degrees);

        for (int d = 1; d <= degrees; d++)
        {
          List<string> unvisitedList = star.GetUnvisitedList();
          star.ResetUnvisitedSet();

          _logger.LogWarning($"networkFor, unvisitedList size: {unvisitedList.Count} degree: {d}");
          for (int i = 0; i < unvisitedList.Count; i++)
          {
            string v = unvisitedList[i];
            Set<DefaultEdge> edges = graph.edgesOf(v);
            star.AddOutEdgesFor(v, edges, d);
          }
        }
      }
      star.Finish();
      return star;
    }

    public Set<DefaultEdge> IncomingEdgesOf(string v)
    {
      if (IsVertexPresent(v))
      {
        return graph.incomingEdgesOf(v);
      }
      return null;
    }

    public int DegreeOf(string v)
    {
      if (IsVertexPresent(v))
      {
        return graph.degreeOf(v);
      }
      return -1;
    }

    public int InDegreeOf(string v)
    {
      if (IsVertexPresent(v))
      {
        return graph.inDegreeOf(v);
      }
      return -1;
    }

    public Double PageRankForVertex(string v)
    {
      if (IsVertexPresent(v))
      {
        PageRank pr = new PageRank(graph);
        return pr.GetVertexScore(v);
      }
      return -1.0;
    }

    public Map PageRankForAll()
    {
      PageRank pr = new PageRank(graph);
      return pr.GetScores();
    }

    public List<JRank> SortedPageRanks(int maxCount)
    {
      List<JRank> ranks = new List<JRank>();
      VertexValueStruct vvStruct = new VertexValueStruct();
      Dictionary<string, double> scores = PageRankForAll();
      Iterator<string> prAllIt = scores.keySet().iterator();
      while (prAllIt.hasNext())
      {
        string vertex = prAllIt.next();
        double value = scores[vertex];
        vvStruct.AddRank(vertex, value);
      }
      vvStruct.Sort();
      for (int i = 0; i < maxCount; i++)
      {
        ranks.Add(vvStruct.GetRank(i));
      }
      return ranks;
    }

    public Double CentralityOfVertex(string v)
    {
      if (IsVertexPresent(v))
      {
        KatzCentrality kc = new KatzCentrality(graph);
        return kc.getVertexScore(v);
      }
      return -1.0;
    }

    public Map CentralityRankAll()
    {
      KatzCentrality kc = new KatzCentrality(graph);
      return kc.getScores();
    }

    public void Refresh()
    {
      //long t1 = System.currentTimeMillis();
      org.jgrapht.Graph<string, DefaultEdge> newGraph = null;
      JGraphBuilder builder = new JGraphBuilder(source);
      _logger.LogWarning($"JGraph refresh(), domain: {Domain}");

      try
      {
        if (Domain.Equals(Constants.GRAPH_DOMAIN_IMDB, StringComparison.OrdinalIgnoreCase))
        {
          newGraph = builder.BuildImdbGraph();
          if (newGraph != null)
          {
            //refreshMs = System.currentTimeMillis() - t1;
            //refreshDate = new Date();
            _logger.LogWarning($"JGraph refresh() - replacing graph with newGraph, elapsed ms: {RefreshMs}");
            graph = newGraph;
          }
        }
      }
      catch (Exception ex)
      {
        //ex.printStackTrace();
      }
    }

    public bool IsVertexPresent(string v)
    {
      if (v != null)
      {
        return graph.containsVertex(v);
      }
      return false;
    }

    private EdgeStruct? ParseDefaultEdge(DefaultEdge e)
    {

      if (e != null)
      {
        string[] tokens = e.ToString().split(":");
        if (tokens.Length == 2)
        {
          EdgeStruct edgeStruct = new EdgeStruct();
          edgeStruct.V1Value = tokens[0].Replace('(', ' ').Trim();
          edgeStruct.V2Value = tokens[1].Replace(')', ' ').Trim();
          return edgeStruct;
        }
      }
      return null;
    }
  }
}