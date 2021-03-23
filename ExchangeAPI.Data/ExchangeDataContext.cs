using ExchangeAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExchangeAPI.Data
{
    public class ExchangeDataContext : DbContext
    {
        public DbSet<ExchangeRate> Rates { get; init; }
    }
}
