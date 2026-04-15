using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Itereta.Data.Entities;

namespace Itereta.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<VocabularyEntry> Entries { get; set; }

        public DbSet<Iteration> Iterations { get; set; }
        public DbSet<Iterette> Iterettes { get; set; }
        

        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }
    }
}
