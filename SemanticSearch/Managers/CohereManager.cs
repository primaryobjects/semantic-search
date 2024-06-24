using LlmTornado.Code;
using LlmTornado;
using LlmTornado.Embedding.Models;

namespace SemanticSearch.Managers
{
    public static class CohereManager
    {
        private static string _apiKey = Environment.GetEnvironmentVariable("CohereApiKey") ?? "";

        /// <summary>
        /// Returns a list of embeddings for each phrase provided as input.
        /// </summary>
        /// <param name="data">string[]</param>
        /// <returns>KeyValuePair<string, float[]></returns>
        public static async Task<List<KeyValuePair<string, float[]>>> GetEmbeddings(string[] data)
        {
            List<KeyValuePair<string, float[]>> result = [];

            // Setup the API.
            TornadoApi api = new([new ProviderAuthentication(LLmProviders.Cohere, _apiKey)]);

            // Retrieve the embeddings for the data.
            var embeddings = await api.Embeddings.GetEmbeddings(EmbeddingModel.Cohere.Gen3.Multilingual, data);
            if (embeddings != null)
            {
                for (int i = 0; i < embeddings.Count; i++)
                {
                    // Add a new embedding consisting of the text and an array of embedding values.
                    result.Add(new KeyValuePair<string,float[]>(data[i], embeddings[i]));
                }
            }

            return result;
        }
    }
}