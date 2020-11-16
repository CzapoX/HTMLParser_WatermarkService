using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Limalima.Backend.Data
{
    public class ArtDbContext : DbContext
    {
        public DbSet<Art> Arts { get; set; }

        public ArtDbContext(DbContextOptions options) : base(options) { }
    }
}
