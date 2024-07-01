using SemanticSearch.Types;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;


namespace SemanticSearch.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Document> Documents { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Load environment variables.
            Env.Load();

            // Configure connection string.
            optionsBuilder.UseSqlite("data source=Documents.sqlite");
        }
    }
}