using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SemanticSearch.Types;
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

        // Check if we have document embeddings already saved.
        GetItemsProcessed(items, out List<string> itemsToProcess, out List<KeyValuePair<int, Document>> itemsProcessed);

        // Get LLM embeddings.
        var dataEmbeddings = await CohereManager.GetEmbeddings([.. itemsToProcess]);

        // Merge loaded documents and save new documents.
        UpdateDocuments(itemsProcessed, dataEmbeddings);

        // Get the query embedding from the end of the list.
        var queryEmbedding = dataEmbeddings.Last();

        // Get the similarity scores and best match.
        var similarities = SimilarityManager.GetSimilarities(queryEmbedding, dataEmbeddings.Take(dataEmbeddings.Count - 1));

        return new JsonResult(new
        {
            result = similarities.Key,
            similarities = similarities.Value.Select(similarity => new { phrase = similarity.Key, score = similarity.Value })
        });
    }

    #region Helper Methods

    private static void UpdateDocuments(List<KeyValuePair<int, Document>> itemsProcessed, List<KeyValuePair<string, float[]>> dataEmbeddings)
    {
        // Save new embeddings.
        for (var i = 0; i < dataEmbeddings.Count; i++)
        {
            DocumentManager.Update(new Document()
            {
                Content = dataEmbeddings[i].Key,
                Embeddings = dataEmbeddings[i].Value
            });
        }

        // Include the embeddings that were loaded from the database.
        for (var i = 0; i < itemsProcessed.Count; i++)
        {
            if (i < dataEmbeddings.Count)
            {
                dataEmbeddings.Insert(itemsProcessed[i].Key, new KeyValuePair<string, float[]>(itemsProcessed[i].Value.Content, itemsProcessed[i].Value.Embeddings));
            }
            else
            {
                dataEmbeddings.Add(new KeyValuePair<string, float[]>(itemsProcessed[i].Value.Content, itemsProcessed[i].Value.Embeddings));
            }
        }
    }

    private static void GetItemsProcessed(string[] items, out List<string> itemsToProcess, out List<KeyValuePair<int, Document>> itemsProcessed)
    {
        // Have we already processed this document before?
        itemsToProcess = [];
        itemsProcessed = [];
        for (var i = 0; i < items.Length; i++)
        {
            string item = items[i];

            var document = DocumentManager.FindByContent(item);
            if (document != null)
            {
                // Use the stored embedding.
                itemsProcessed.Add(new KeyValuePair<int, Document>(i, document));
            }
            else
            {
                itemsToProcess.Add(item);
            }
        }

        Console.WriteLine($"Loaded {itemsProcessed.Count} documents. Calling LLM for {itemsToProcess.Count} documents.");
    }

    #endregion
}
