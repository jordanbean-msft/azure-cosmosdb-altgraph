using QuikGraph;

namespace altgraph_shared_app.Services.Graph.v2
{
  public interface IJGraphBuilder
  {
    public Task<IMutableGraph<string, Edge<string>>?> BuildImdbGraphAsync();
  }
}