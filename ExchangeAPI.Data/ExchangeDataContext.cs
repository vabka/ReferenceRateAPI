using System;
using ExchangeAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExchangeAPI.Data
{
    public class ExchangeDataContext : DbContext
    {
        public DbSet<ExchangeRate> Rates { get; init; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=E:\exchange.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ExchangeRate>()
                .HasKey(x => new {x.Date, x.Base, x.Currency});
            modelBuilder.Entity<ExchangeRate>()
                .Property(x => x.Date)
                .HasConversion(v => v.Date.ToFileTime(),
                    v => DateTimeOffset.FromFileTime(v));
            modelBuilder.Entity<ExchangeRate>()
                .Property(x => x.Base)
                .HasMaxLength(3);
        }
    }
}
