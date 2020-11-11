using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Transaction.Entity.Entity;

namespace Transaction.Entity
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

        public DbSet<TransactionEntity> TransactionEntity { get; set; }
        public DbSet<TransactionErrorLog> TransactionErrorLog { get; set; }
        public DbSet<TransactionFile> TransactionFile { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
        }
    }
}
