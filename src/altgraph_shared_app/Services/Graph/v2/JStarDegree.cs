namespace altgraph_shared_app.Services.Graph.v2
{
  public class JStarDegree
  {
    public int Degree { get; set; }
    public Dictionary<string, Set<DefaultEdge>> OutgoingEdgesMap { get; set; };

    public JStarDegree(int degree)
    {
      Degree = degree;
      OutgoingEdgesMap = new Dictionary<string, Set<DefaultEdge>>();
    }

    public void Add(string vertex, Set<DefaultEdge> outEdges)
    {
      OutgoingEdgesMap.Add(vertex, outEdges);
    }
  }
}