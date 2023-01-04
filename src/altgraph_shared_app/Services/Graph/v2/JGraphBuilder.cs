using altgraph_shared_app.Models.Imdb;
using altgraph_shared_app.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuikGraph;

namespace altgraph_shared_app.Services.Graph.v2
{
  public class JGraphBuilder : IJGraphBuilder
  {
    public string Uri { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string DbName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public bool Directed { get; set; } = false;
    private ILogger _logger;
    private CosmosOptions _cosmosOptions;
    private ImdbOptions _imdbOptions;

    public JGraphBuilder(string source, ILogger logger, IOptions<CosmosOptions> cosmosOptions, IOptions<ImdbOptions> imdbOptions)
    {
      Source = source;
      _logger = logger;
      _cosmosOptions = cosmosOptions.Value;
      _imdbOptions = imdbOptions.Value;
    }

    public IMutableGraph<string, Edge<string>>? BuildImdbGraph()
    {
      _logger.LogWarning($"buildImdbGraph, source:   {Source}");
      _logger.LogWarning($"buildImdbGraph, directed: {Directed}");

      try
      {
        switch (Source)
        {
          case Constants.IMDB_GRAPH_SOURCE_DISK:
            return LoadImdbGraphFromDisk(Directed);
          case Constants.IMDB_GRAPH_SOURCE_COSMOS:
            return LoadImdbGraphFromCosmos(Directed);
          default:
            _logger.LogWarning($"undefined graph source: {Source}");
            return null;
        }
      }
      catch (Exception ex)
      {
        //ex.printStackTrace();
        throw ex;
      }
      //return null;
    }

    private IMutableGraph<string, Edge<string>> CreateGraphObject(bool directed)
    {
      IMutableGraph<string, Edge<string>> graph;

      if (directed)
      {
        //graph = new DirectedMultigraph(DefaultEdge.class);
        graph = new BidirectionalGraph<string, Edge<string>>();
      }
      else
      {
        //graph = new Multigraph(DefaultEdge.class);
        graph = new UndirectedGraph<string, Edge<string>>();
      }
      return graph;
    }

    private IMutableGraph<string, Edge<string>> LoadImdbGraphFromDisk(bool directed)
    {
      throw new NotImplementedException();
      // IMutableGraph<string, Edge<string>> graph = CreateGraphObject(directed);

      // JsonLoader jsonLoader = new JsonLoader();
      // Dictionary<string, Movie> moviesHash = new Dictionary<string, Movie>();
      // jsonLoader.readMovieDocuments(moviesHash, true);
      // CheckMemory(true, true, "loadImdbGraphFromDisk - after reading movies from file");

      // Iterator<string> moviesIt = moviesHash.keySet().iterator();
      // long movieNodesCreated = 0;
      // long personNodesCreated = 0;
      // long edgesCreated = 0;

      // while (moviesIt.hasNext())
      // {
      //   string tconst = moviesIt.next();
      //   Movie movie = moviesHash.get(tconst);
      //   if (!graph.ContainsVertex(tconst))
      //   {
      //     graph.AddVertex(tconst);
      //     movieNodesCreated++;
      //   }

      //   Iterator<string> peopleIt = movie.getPeople().iterator();
      //   while (peopleIt.hasNext())
      //   {
      //     string nconst = peopleIt.next();
      //     if (!graph.ContainsVertex(nconst))
      //     {
      //       graph.AddVertex(nconst);
      //       personNodesCreated++;
      //     }
      //     graph.AddEdge(nconst, tconst);  // person-to-movie
      //     edgesCreated++;
      //     if (directed)
      //     {
      //       // just a single edge between vertices
      //     }
      //     else
      //     {
      //       graph.AddEdge(tconst, nconst);  // movie-to-person
      //       edgesCreated++;
      //     }
      //   }
      // }

      // CheckMemory(true, true, "loadImdbGraphFromDisk - after building graph");
      // _logger.LogWarning($"loadImdbGraphFromDisk - movieNodesCreated:  {movieNodesCreated}");
      // _logger.LogWarning($"loadImdbGraphFromDisk - personNodesCreated: {personNodesCreated}");
      // _logger.LogWarning($"loadImdbGraphFromDisk - edgesCreated:       {edgesCreated}");
      // return graph;
    }

    private IMutableGraph<string, Edge<string>> LoadImdbGraphFromCosmos(bool directed)
    {
      Uri = _cosmosOptions.ConnectionString;
      //Key = _cosmosOptions.Key;
      DbName = _cosmosOptions.DatabaseId;
      Source = _imdbOptions.GraphSource;
      Directed = _imdbOptions.GraphDirected;

      IMutableGraph<string, Edge<string>> graph = CreateGraphObject(directed);

      CosmosClient client;
      Database database;
      Container container;
      double requestCharge = 0;
      long documentsRead = 0;
      long movieNodesCreated = 0;
      long personNodesCreated = 0;
      long edgesCreated = 0;
      string sql = "select * from c where c.pk = '" + Constants.DOCTYPE_MOVIE_SEED + "'";
      int pageSize = 1000;
      string continuationToken = null;
      //CosmosQueryRequestOptions queryOptions = new CosmosQueryRequestOptions();

      _logger.LogWarning("uri:    " + Uri);
      _logger.LogWarning("key:    " + Key);
      _logger.LogWarning("dbName: " + DbName);
      _logger.LogWarning("sql:    " + sql);

      //CheckMemory(true, true, "loadImdbGraphFromCosmos - start");
      //long startMs = System.currentTimeMillis();

      client = new CosmosClientBuilder(
              connectionString: Uri
      )
              .WithApplicationPreferredRegions(_cosmosOptions.PreferredLocations)
              .WithConsistencyLevel(ConsistencyLevel.Session)
              .WithContentResponseOnWrite(true)
              .Build();

      database = client.GetDatabase(DbName);
      _logger.LogWarning("client connected to database Id: " + database.getId());

      container = database.GetContainer(Constants.IMDB_SEED_CONTAINER_NAME);
      _logger.LogWarning("container: " + container.Id);

      //long dbConnectMs = System.currentTimeMillis();

      try
      {
        do
        {
          Iterable<FeedResponse<SeedDocument>> feedResponseIterator =
                  container.queryItems(sql, queryOptions, SeedDocument.class).byPage(
                          continuationToken, pageSize).toIterable();  // Convert Asynch Flux to Iterable

foreach (FeedResponse<SeedDocument> page : feedResponseIterator)
{
  foreach (SeedDocument doc : page.GetResults())
  {
    documentsRead++;
    if ((documentsRead % 10000) == 0)
    {
      _logger.LogWarning("" + documentsRead + " -> " + doc.asJson(false));
    }
  string tconst = doc.GetTargetId();
  graph.AddVertex(tconst);
movieNodesCreated++;

for (int i = 0; i<doc.GetAdjacentVertices().Count(); i++)
{
  string nconst = doc.GetAdjacentVertices().get(i);
  if (!graph.ContainsVertex(nconst))
  {
    graph.AddVertex(nconst);
    personNodesCreated++;
  }
graph.AddEdge(nconst, tconst);  // person-to-movie
edgesCreated++;

if (directed)
{
  // just a single edge between vertices
}
else
{
  graph.AddEdge(tconst, nconst);  // movie-to-person
  edgesCreated++;
}
}

  }
  requestCharge = requestCharge + page.getRequestCharge();
continuationToken = page.getContinuationToken();
}
            }
            while (continuationToken != null) ;
        } catch (Exception t)
{
  //t.printStackTrace();
}
// long finishMs = System.currentTimeMillis();
// long dbConnectElapsed = dbConnectMs - startMs;
// long dbReadingElapsed = finishMs - dbConnectMs;
// double dbReadingSeconds = (double)dbReadingElapsed / 1000.0;
// long totalElapsed = finishMs - startMs;

// double ruPerSec = (double)requestCharge / dbReadingSeconds;

CheckMemory(true, true, "loadImdbGraphFromCosmos - after building graph");
_logger.LogWarning("loadImdbGraphFromCosmos - documentsRead:      " + documentsRead);
_logger.LogWarning("loadImdbGraphFromCosmos - movieNodesCreated:  " + movieNodesCreated);
_logger.LogWarning("loadImdbGraphFromCosmos - personNodesCreated: " + personNodesCreated);
_logger.LogWarning("loadImdbGraphFromCosmos - edgesCreated:       " + edgesCreated);
_logger.LogWarning("loadImdbGraphFromCosmos - requestCharge:      " + requestCharge);
_logger.LogWarning("loadImdbGraphFromCosmos - ru per second:      " + ruPerSec);
_logger.LogWarning("loadImdbGraphFromCosmos - db connect ms:      " + dbConnectElapsed);
_logger.LogWarning("loadImdbGraphFromCosmos - db read ms:         " + dbReadingElapsed);
_logger.LogWarning("loadImdbGraphFromCosmos - total elapsed ms:   " + totalElapsed);
return graph;
    }

    protected MemoryStats CheckMemory(bool doGc, bool display, string note)
{
  // if (doGc)
  // {
  //   System.gc();
  // }
  // MemoryStats ms = new MemoryStats(note);
  // if (display)
  // {
  //   try
  //   {
  //     sysout(ms.asDelimitedHeaderLine("|"));
  //     sysout(ms.asDelimitedDataLine("|"));
  //   }
  //   catch (Exception e)
  //   {
  //     sysout("error serializing MemoryStats to JSON");
  //   }
  // }
  // return ms;
}

// protected void sysout(string s)
// {
//   System.out.println(s);
// }

}
}