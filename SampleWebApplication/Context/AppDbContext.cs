using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Configuration;

namespace SampleWebApplication.Context
{
    public class AppDbContext : DbContext
    {
        private readonly ILoggerFactory? _loggerFactory;
        public DbSet<Note> Notes { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options, ILoggerFactory? loggerFactory) : base(options)
        {
            _loggerFactory = loggerFactory;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_loggerFactory != null)
                optionsBuilder.UseLoggerFactory(_loggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Note>().HasIndex(r => r.id).IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }

    public record Note(int id)
    {
        public string text { get; set; } = default!;
        public bool done { get; set; } = default!;
    }
}
