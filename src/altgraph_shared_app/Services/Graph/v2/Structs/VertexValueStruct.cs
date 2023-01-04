namespace altgraph_shared_app.Services.Graph.v2.Structs
{
  public class VertexValueStruct
  {
    public long ElapsedMs = -1;
    public string Doctype { get; private set; } = "VertexValueStruct";
    public string Function { get; set; } = string.Empty;

    //public HashMap<string, Double> ranks;
    public List<JRank> Ranks { get; private set; } = new List<JRank>();

    public VertexValueStruct()
    {
    }
    public VertexValueStruct(string function)
    {
      Function = function;
    }

    public void AddRank(string vertex, double value)
    {
      if (vertex != null)
      {
        Ranks.Add(new JRank(vertex, value));
      }
    }

    public JRank GetRank(int idx)
    {
      return Ranks[idx];
    }

    public void Sort()
    {
      Ranks.Sort(new JRankComparator());
    }
  }
}