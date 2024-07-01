using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SemanticSearch.Types
{
    public class Document
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Content { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime? DateUpdated { get; set; }
        public float[]? Embeddings { get; set; }
    }
}