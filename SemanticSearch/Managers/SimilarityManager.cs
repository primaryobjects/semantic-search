using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SemanticSearch.Managers
{
    public static class SimilarityManager
    {
        /// <summary>
        /// Returns a list of similarity scores for a query against a list of phrases.
        /// The input is a KeyValuePair for text and embeddings.
        /// The method returns a KeyValuePair with the best matching text and a list of text with similarity scores.
        /// </summary>
        /// <param name="query">KeyValuePair<string, float[]></param>
        /// <param name="data">IEnumerable<KeyValuePair<string, float[]>></param>
        /// <returns>KeyValuePair<string, IEnumerable<KeyValuePair<string, float>>></returns>
        public static KeyValuePair<string, IEnumerable<KeyValuePair<string, float>>> GetSimilarities(KeyValuePair<string, float[]> query, IEnumerable<KeyValuePair<string, float[]>> data)
        {
            // Calculate the cosine similarity for the query against each data string.
            List<Tuple<int, float>> similarities = [];
            for (int i=0; i<data.Count(); i++)
            {
                float similarity = CosineSimilarity(data.ElementAt(i).Value, query.Value);
                similarities.Add(new Tuple<int, float>(i, similarity));
            }

            // Sort the results.
            var result = similarities.OrderByDescending(x => x.Item2);

            // Get the index of the best match.
            var bestMatchIndex = result.First().Item1;
            var bestMatchText = data.ElementAt(bestMatchIndex).Key;

            // Get each phrase and its similarity score.
            IEnumerable<KeyValuePair<string, float>> phraseSimilarities = similarities.Select(x => new KeyValuePair<string, float>(data.ElementAt(x.Item1).Key, x.Item2));

            return new (bestMatchText, phraseSimilarities);
        }

        /// <summary>
        /// Returns the cosine similarity between two vectors of floats. Range is 0.0 - 1.0.
        /// </summary>
        /// <param name="vector1">float[]</param>
        /// <param name="vector2">float[]</param>
        /// <returns>float</returns>
        private static float CosineSimilarity(float[] vector1, float[] vector2)
        {
            if (vector1.Length != vector2.Length)
            {
                throw new Exception("Vectors must be the same length");
            }

            float dotProduct = 0.0f;
            float magnitude1 = 0.0f;
            float magnitude2 = 0.0f;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += (float)Math.Pow(vector1[i], 2);
                magnitude2 += (float)Math.Pow(vector2[i], 2);
            }

            return dotProduct / ((float)Math.Sqrt(magnitude1) * (float)Math.Sqrt(magnitude2));
        }
    }
}