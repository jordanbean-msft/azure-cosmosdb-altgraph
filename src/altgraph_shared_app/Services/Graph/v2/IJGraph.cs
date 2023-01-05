using QuikGraph;

namespace altgraph_shared_app.Services.Graph.v2
{
  public interface IJGraph
  {
    string Domain { get; set; }
    string Source { get; set; }
    DateTime RefreshDate { get; set; }
    long RefreshMs { get; set; }
    IMutableGraph<string, Edge<string>>? Graph { get; set; }
    int[] GetVertexAndEdgeCounts();
    IEnumerable<Edge<string>>? GetShortestPath(string v1, string v2);
    bool IsVertexPresent(string v);
    void Refresh();
  }
}