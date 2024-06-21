using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SemanticSearch.Managers;

namespace SemanticSearch.Pages;

public class IndexModel(ILogger<IndexModel> logger) : PageModel
{
    private readonly ILogger<IndexModel> _logger = logger;

    public IActionResult OnPostSearch(string data, string query)
    {
        var dataEmbeddings = CohereManager.GetEmbeddings(data.Split(",")).GetAwaiter().GetResult();
        var queryEmbeddings = CohereManager.GetEmbeddings([query]).GetAwaiter().GetResult();
        var queryEmbedding = queryEmbeddings.First();

        List<Tuple<int, float>> similarities = [];
        for (int i=0; i<dataEmbeddings.Count; i++)
        {
            float similarity = CohereManager.CosineSimilarity(dataEmbeddings[i].Value, queryEmbedding.Value);
            similarities.Add(new Tuple<int, float>(i, similarity));
        }

        var result = similarities.OrderByDescending(x => x.Item2).Take(1).ToList();
        var index = result.First().Item1;

        var response = new Dictionary<string, object>
        {
            { "result", dataEmbeddings[index].Key },
            { "similarities", similarities.Select(x => new { index = x.Item1, phrase = dataEmbeddings[x.Item1].Key, similarity = x.Item2 }) }
        };

        return new JsonResult(response);
    }
}
