using System;
using ExchangeAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExchangeAPI.Data
{
    public sealed class ReferenceRatesDbContext : DbContext
    {
        public ReferenceRatesDbContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<ExchangeRate> Rates { get; init; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ExchangeRate>()
                .HasKey(x => new {x.Date, x.Currency});
            if (Database.IsSqlite())
                modelBuilder.Entity<ExchangeRate>()
                    .Property(x => x.Date)
                    .HasConversion(v => v.ToUniversalTime().ToFileTime(),
                        v => DateTimeOffset.FromFileTime(v));
        }
    }
}
