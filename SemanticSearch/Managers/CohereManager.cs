using System;
using System.Collections.Generic;
using LlmTornado.Embedding;
using DotNetEnv;
using LlmTornado.Code;
using LlmTornado;
using LlmTornado.Chat.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;

namespace SemanticSearch.Managers
{
    public static class CohereManager
    {
        private static readonly HttpClient _client = new HttpClient();

        public static async Task<List<KeyValuePair<string, List<float>>>> GetEmbeddings(string[] data)
        {
            List<KeyValuePair<string, List<float>>> result = [];

            string apiKey = Environment.GetEnvironmentVariable("CohereApiKey") ?? "";

            TornadoApi api = new([new ProviderAuthentication(LLmProviders.Cohere, apiKey)]);
            /*EmbeddingResult result = api.Embeddings.CreateEmbeddingAsync("cats and dogs").GetAwaiter().GetResult();
            float[]? embeddings = result.Data.FirstOrDefault()?.Embedding;

            if (embeddings != null)
            {
                for (var i=0; i<embeddings.Length; i++)
                {
                    Console.WriteLine(embeddings[i]);
                }
            }*/

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.cohere.com/v1/embed"),
                Headers =
                {
                    { "Authorization", $"Bearer {apiKey}" }
                },
                Content = new StringContent(JsonConvert.SerializeObject(new
                {
                    model = "embed-english-v3.0",
                    texts = data,
                    input_type = "classification",
                    truncate = "NONE"
                }), Encoding.UTF8, "application/json")
            };

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            JObject jsonObject = JObject.Parse(responseBody);
            JArray embeddings = (JArray)jsonObject["embeddings"]!;

            int index = 0;
            foreach (JArray embedding in embeddings.Cast<JArray>())
            {
                List<float> sublist = [];

                List<double> embeddingValues = embedding.ToObject<List<double>>()!;
                foreach (double value in embeddingValues)
                {
                    sublist.Add((float)value);
                    //Console.WriteLine(value);
                }

                result.Add(new KeyValuePair<string, List<float>>(data[index++], sublist));
            }

            return result;
        }

        public static float CosineSimilarity(List<float> vector1, List<float> vector2)
        {
            if (vector1.Count != vector2.Count)
            {
                throw new Exception("Vectors must be the same length");
            }

            float dotProduct = 0.0f;
            float magnitude1 = 0.0f;
            float magnitude2 = 0.0f;

            for (int i = 0; i < vector1.Count; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += (float)Math.Pow(vector1[i], 2);
                magnitude2 += (float)Math.Pow(vector2[i], 2);
            }

            return dotProduct / ((float)Math.Sqrt(magnitude1) * (float)Math.Sqrt(magnitude2));
        }
    }
}