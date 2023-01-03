using Microsoft.Extensions.Logging;

namespace altgraph_shared_app.Services.Graph.v2
{
  public class JGraphBuilder
  {
    public string Uri { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string DbName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public bool Directed { get; set; } = false;
    private ILogger<JGraphBuilder> _logger;

    public JGraphBuilder(string source, ILogger<JGraphBuilder> logger)
    {
      Source = source;
      _logger = logger;
    }

    public org.jgrapht.Graph<string, DefaultEdge> BuildImdbGraph()
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
        }
      }
      catch (Exception ex)
      {
        //ex.printStackTrace();
        throw ex;
      }
      return null;
    }

    private org.jgrapht.Graph<string, DefaultEdge> CreateGraphObject(bool directed)
    {
      org.jgrapht.Graph<string, DefaultEdge> graph = null;

      if (directed)
      {
        //graph = new DirectedMultigraph(DefaultEdge.class);
      }
      else
      {
        //graph = new Multigraph(DefaultEdge.class);
      }
      return graph;
    }

    private org.jgrapht.Graph<string, DefaultEdge> LoadImdbGraphFromDisk(bool directed)
    {
      org.jgrapht.Graph<string, DefaultEdge> graph = createGraphObject(directed);

      JsonLoader jsonLoader = new JsonLoader();
      Dictionary<string, Movie> moviesHash = new Dictionary<string, Movie>();
      jsonLoader.readMovieDocuments(moviesHash, true);
      CheckMemory(true, true, "loadImdbGraphFromDisk - after reading movies from file");

      Iterator<string> moviesIt = moviesHash.keySet().iterator();
      long movieNodesCreated = 0;
      long personNodesCreated = 0;
      long edgesCreated = 0;

      while (moviesIt.hasNext())
      {
        string tconst = moviesIt.next();
        Movie movie = moviesHash.get(tconst);
        if (!graph.containsVertex(tconst))
        {
          graph.addVertex(tconst);
          movieNodesCreated++;
        }

        Iterator<string> peopleIt = movie.getPeople().iterator();
        while (peopleIt.hasNext())
        {
          string nconst = peopleIt.next();
          if (!graph.containsVertex(nconst))
          {
            graph.addVertex(nconst);
            personNodesCreated++;
          }
          graph.addEdge(nconst, tconst);  // person-to-movie
          edgesCreated++;
          if (directed)
          {
            // just a single edge between vertices
          }
          else
          {
            graph.addEdge(tconst, nconst);  // movie-to-person
            edgesCreated++;
          }
        }
      }

      CheckMemory(true, true, "loadImdbGraphFromDisk - after building graph");
      _logger.LogWarning($"loadImdbGraphFromDisk - movieNodesCreated:  {movieNodesCreated}");
      _logger.LogWarning($"loadImdbGraphFromDisk - personNodesCreated: {personNodesCreated}");
      _logger.LogWarning($"loadImdbGraphFromDisk - edgesCreated:       {edgesCreated}");
      return graph;
    }

    private org.jgrapht.Graph<string, DefaultEdge> LoadImdbGraphFromCosmos(boolean directed)
    {
      uri = DataAppConfiguration.getInstance().uri;
      key = DataAppConfiguration.getInstance().key;
      dbName = DataAppConfiguration.getInstance().dbName;
      source = DataAppConfiguration.getInstance().imdbGraphSource;
      directed = DataAppConfiguration.getInstance().imdbGraphDirected;

      org.jgrapht.Graph<string, DefaultEdge> graph = CreateGraphObject(directed);

      CosmosAsyncClient client;
      CosmosAsyncDatabase database;
      CosmosAsyncContainer container;
      double requestCharge = 0;
      long documentsRead = 0;
      long movieNodesCreated = 0;
      long personNodesCreated = 0;
      long edgesCreated = 0;
      string sql = "select * from c where c.pk = '" + Constants.DOCTYPE_MOVIE_SEED + "'";
      int pageSize = 1000;
      string continuationToken = null;
      CosmosQueryRequestOptions queryOptions = new CosmosQueryRequestOptions();

      _logger.LogWarning("uri:    " + Uri);
      _logger.LogWarning("key:    " + Key);
      _logger.LogWarning("dbName: " + DbName);
      _logger.LogWarning("sql:    " + sql);

      CheckMemory(true, true, "loadImdbGraphFromCosmos - start");
      //long startMs = System.currentTimeMillis();

      client = new CosmosClientBuilder()
              .endpoint(Uri)
              .key(Key)
              .preferredRegions(DataAppConfiguration.getPreferredRegions())
              .consistencyLevel(ConsistencyLevel.SESSION)
              .contentResponseOnWriteEnabled(true)
              .buildAsyncClient();

      database = client.getDatabase(DbName);
      _logger.LogWarning("client connected to database Id: " + database.getId());

      container = database.getContainer(Constants.IMDB_SEED_CONTAINER);
      _logger.LogWarning("container: " + container.getId());

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

for (int i = 0; i<doc.GetAdjacentVertices().size(); i++)
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