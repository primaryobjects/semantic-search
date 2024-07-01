using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Update.Internal;
using SemanticSearch.Database;
using SemanticSearch.Types;

namespace SemanticSearch.Managers
{
    public static class DocumentManager
    {
        public static Document? FindByContent(string content)
        {
            var context = new DatabaseContext();
            return context.Documents.FirstOrDefault(document => document.Content == content);
        }

        public static Document? FindById(Guid id)
        {
            using var context = new DatabaseContext();
            return context.Documents.FirstOrDefault(document => document.Id == id);
        }

        public static Document? Update(Document document)
        {
            Document result = document;

            using var context = new DatabaseContext();
            var existingDocument = context.Documents.Find(document.Id);
            if (existingDocument != null)
            {
                existingDocument.Content = document.Content;
                existingDocument.Embeddings = document.Embeddings;
                existingDocument.DateUpdated = DateTime.Now;

                result = existingDocument;
            }
            else
            {
                context.Documents.Add(document);
            }

            context.SaveChanges();

            return result;
        }
    }
}