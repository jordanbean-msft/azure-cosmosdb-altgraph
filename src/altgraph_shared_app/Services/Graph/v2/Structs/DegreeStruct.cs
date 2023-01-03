namespace altgraph_shared_app.Services.Graph.v2.Structs
{
  public class DegreeStruct
  {
    public long ElapsedMs { get; set; } = -1;
    public string Doctype { get; private set; } = "DegreeStruct";
    public string Vertex { get; set; } = string.Empty;
    public int Degree { get; set; }
    public int InDegree { get; set; }
  }
}