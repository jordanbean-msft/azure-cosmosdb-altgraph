using System.Diagnostics.CodeAnalysis;
using altgraph_shared_app.Services.Graph.v2.Structs;
using QuikGraph;

namespace altgraph_shared_app.Services.Graph.v2
{
  public class JEdge : Edge<VertexValueStruct>
  {
    public JEdge([NotNullAttribute] VertexValueStruct source, [NotNullAttribute] VertexValueStruct target) : base(source, target)
    {
    }

    public string? S()
    {
      return Source.ToString();
    }

    public String? T()
    {
      return Target.ToString();
    }
  }
}