using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace atm.Models
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options)

           : base(options)

        {

            Database.EnsureCreated();
        }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Transfer> Transfers { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Withdrawal> Withdrawals { get; set; }

        public DbSet<TransactionType> TransactionTypes { get; set; }
    }
}