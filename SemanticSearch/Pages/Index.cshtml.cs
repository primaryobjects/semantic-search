using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SemanticSearch.Managers;

namespace SemanticSearch.Pages;

public class IndexModel(ILogger<IndexModel> logger) : PageModel
{
    private readonly ILogger<IndexModel> _logger = logger;

    public async Task<IActionResult> OnPostSearch(string data, string query)
    {
        // Split the data by comma and append the query to the end of the list.
        var items = string.Join(",", data, query).Split(",");
        items = items.Select(s => s.Trim()).ToArray();

        // Get embeddings.
        var dataEmbeddings = await CohereManager.GetEmbeddings(items);

        // Get the query embedding from the end of the list.
        var queryEmbedding = dataEmbeddings.Last();

        // Get the similarity scores and best match.
        var similarities = SimilarityManager.GetSimilarities(queryEmbedding, dataEmbeddings.Take(dataEmbeddings.Count - 1));

        return new JsonResult(new {
            result = similarities.Key,
            similarities = similarities.Value.Select(similarity => new { phrase = similarity.Key, score = similarity.Value })
        });
    }
}
