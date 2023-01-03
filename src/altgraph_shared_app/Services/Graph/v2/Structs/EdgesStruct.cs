namespace altgraph_shared_app.Services.Graph.v2.Structs
{
  public class EdgesStruct
  {
    public DateTime Date { get; set; } = DateTime.Now;
    public long ElapsedMs { get; set; } = -1;
    public string Doctype { get; private set; } = "EdgesStruct";
    public string Vertex1 { get; set; } = string.Empty;
    public string Vertex2 { get; set; } = string.Empty;
    public List<EdgeStruct> Edges { get; private set; } = new List<EdgeStruct>();

    public void AddEdge(EdgeStruct edge)
    {
      if (edge != null && edge.IsValid())
      {
        edge.Seq = Edges.Count + 1;
        Edges.Add(edge);
      }
    }
  }
}