#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QLess.Web.Models;

namespace QLess.Web.Data
{
    public class QLessContext : DbContext
    {
        public QLessContext (DbContextOptions<QLessContext> options)
            : base(options)
        {
        }

        public DbSet<QLess.Web.Models.TransportCard> TransportCards { get; set; }
        public DbSet<QLess.Web.Models.Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransportCard>().ToTable("TransportCard");
            modelBuilder.Entity<Transaction>().ToTable("Transaction");
        }
    }
}
