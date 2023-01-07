using System.Text.Json;
using altgraph_shared_app.Options;
using altgraph_shared_app.Services.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.CosmosRepository;
using Microsoft.Extensions.Options;
using altgraph_shared_app.Repositories.Imdb;
using altgraph_shared_app.Models.Imdb;
using altgraph_shared_app.Services.Graph.v2;

namespace altgraph_web_app.Areas.ImdbGraph.Pages;

public class IndexModel : PageModel
{
  private readonly ILogger<IndexModel> _logger;
  [BindProperty]
  public FormFunctionEnum FormFunction { get; set; } = FormFunctionEnum.GraphStats;
  [BindProperty]
  public string Value1 { get; set; } = string.Empty;
  [BindProperty]
  public string? Value2 { get; set; } = string.Empty;
  [BindProperty]
  public string? ElapsedMs { get; set; } = string.Empty;
  [BindProperty(SupportsGet = true)]
  public string? EdgesStruct { get; set; } = string.Empty;
  //[BindProperty(SupportsGet = true)]
  //public string? VertexInfo { get; set; } = string.Empty;
  private IJGraph _jGraph;
  private readonly MovieRepository _movieRepository;
  private readonly PersonRepository _personRepository;
  private readonly ICache _cache;
  private readonly CacheOptions _cacheOptions;
  private readonly PathsOptions _pathsOptions;
  private readonly ImdbOptions _imdbOptions;

  public IndexModel(ILogger<IndexModel> logger, IRepository<Movie> movieRepository, IRepository<Person> personRepository, ICache cache, IOptions<CacheOptions> cacheOptions, IOptions<PathsOptions> pathsOptions, IOptions<ImdbOptions> imdbOptions, IJGraph jgraph)
  {
    _logger = logger;
    _movieRepository = new MovieRepository(movieRepository);
    _personRepository = new PersonRepository(personRepository);
    _cache = cache;
    _logger = logger;
    _cacheOptions = cacheOptions.Value;
    _pathsOptions = pathsOptions.Value;
    _imdbOptions = imdbOptions.Value;
    _jGraph = jgraph;

    int[] counts = _jGraph.GetVertexAndEdgeCounts();
    _logger.LogWarning($"jgraph vertices: {counts[0]}");
    _logger.LogWarning($"jgraph edges:    {counts[1]}");
  }

  public async Task<JsonResult?> OnGetImdbVertexAsync(string imdbConst)
  {
    _logger.LogWarning($"OnGetImdbVertexAsync, imdbConst: {imdbConst}");

    try
    {
      if (imdbConst.StartsWith("tt"))
      {
        Movie? m = await LookupMovieAsync(imdbConst);
        if (m != null)
        {
          return new JsonResult(m);
        }
      }
      else if (imdbConst.StartsWith("nm"))
      {
        Person? p = await LookupPersonAsync(imdbConst);
        if (p != null)
        {
          return new JsonResult(p);
        }
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, ex.Message);
    }
    return null;
  }

  private async Task<Movie?> LookupMovieAsync(String imdbConst)
  {
    foreach (Movie movie in await _movieRepository.FindByIdAndPkAsync(imdbConst, imdbConst))
    {
      return movie;
    }

    return null;
  }

  private async Task<Person?> LookupPersonAsync(String imdbConst)
  {
    foreach (Person person in await _personRepository.FindByIdAndPkAsync(imdbConst, imdbConst))
    {
      return person;
    }

    return null;
  }

  public async Task<IActionResult> OnPostAsync()
  {
    if (!ModelState.IsValid)
    {
      return Page();
    }

    DateTime start = DateTime.Now;

    TranslateShortcutValues();

    _logger.LogWarning($"formObject, getFormFunction:     {FormFunction}");
    _logger.LogWarning($"formObject, getValue1:           {Value1}");
    _logger.LogWarning($"formObject, getValue2:           {Value2}");
    _logger.LogWarning($"formObject, getSessionId (form): {HttpContext.Session.Id}");

    if (_jGraph.Graph == null)
    {
      await _jGraph.RefreshAsync();
    }

    try
    {
      switch (FormFunction)
      {
        case FormFunctionEnum.GraphStats:
          break;
        case FormFunctionEnum.PageRank:
          if (!IsValue1AnInteger())
          {
            Value1 = "100";
          }
          break;
        case FormFunctionEnum.Network:
          if (!IsValue2AnInteger())
          {
            Value2 = "1";
          }
          break;
        case FormFunctionEnum.ShortestPath:
          break;
        default:
          throw new NotImplementedException(FormFunction.ToString());
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, ex.Message);
    }

    DateTime end = DateTime.Now;

    ElapsedMs = $"{Math.Round((end - start).TotalMilliseconds)} ms";

    return Page();
  }

  private Task HandleShortestPathAsync()
  {
    throw new NotImplementedException();
  }

  public JsonResult? OnGetStarNetwork(string vertex, string degree)
  {
    _logger.LogDebug($"OnGetStarNetwork, vertex: {vertex}, degree: {degree}");

    if (degree != null)
    {
      JStarNetwork? star = _jGraph.StarNetworkFor(vertex, int.Parse(degree));
      if (star != null)
      {
        return new JsonResult(star.AsEdgesStruct());
      }
    }
    return null;
  }

  private Task HandlePageRankAsync()
  {
    throw new NotImplementedException();
  }

  private Task HandleGraphStatsAsync()
  {
    throw new NotImplementedException();
  }

  public void TranslateShortcutValues()
  {
    if (Value1.Equals("kb", StringComparison.InvariantCultureIgnoreCase))
    {
      Value1 = ImdbConstants.PERSON_KEVIN_BACON;
    }
    if (Value1.Equals("cr", StringComparison.InvariantCultureIgnoreCase))
    {
      Value1 = ImdbConstants.PERSON_CHARLOTTE_RAMPLING;
    }
    if (Value1.Equals("jr", StringComparison.InvariantCultureIgnoreCase))
    {
      Value1 = ImdbConstants.PERSON_JULIA_ROBERTS;
    }
    if (Value1.Equals("jl", StringComparison.InvariantCultureIgnoreCase))
    {
      Value1 = ImdbConstants.PERSON_JENNIFER_LAWRENCE;
    }
    if (Value1.Equals("fl", StringComparison.InvariantCultureIgnoreCase))
    {
      Value1 = ImdbConstants.MOVIE_FOOTLOOSE;
    }

    if (Value2 != null)
    {
      if (Value2.Equals("kb", StringComparison.InvariantCultureIgnoreCase))
      {
        Value2 = ImdbConstants.PERSON_KEVIN_BACON;
      }
      if (Value2.Equals("cr", StringComparison.InvariantCultureIgnoreCase))
      {
        Value2 = ImdbConstants.PERSON_CHARLOTTE_RAMPLING;
      }
      if (Value2.Equals("jr", StringComparison.InvariantCultureIgnoreCase))
      {
        Value2 = ImdbConstants.PERSON_JULIA_ROBERTS;
      }
      if (Value2.Equals("jl", StringComparison.InvariantCultureIgnoreCase))
      {
        Value2 = ImdbConstants.PERSON_JENNIFER_LAWRENCE;
      }
      if (Value2.Equals("fl", StringComparison.InvariantCultureIgnoreCase))
      {
        Value2 = ImdbConstants.MOVIE_FOOTLOOSE;
      }
    }
  }

  public bool IsValue1AnInteger()
  {
    try
    {
      int.Parse(Value1);
      return true;
    }
    catch (FormatException)
    {
      return false;
    }
  }

  public bool IsValue2AnInteger()
  {
    if (Value2 == null)
    {
      return false;
    }
    try
    {
      int.Parse(Value2);
      return true;
    }
    catch (FormatException)
    {
      return false;
    }
  }

  public String ReloadFlag()
  {
    if (Value1.ToLower().Contains("reload"))
    {
      return "reload";
    }
    return "no";
  }
}
