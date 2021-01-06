using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Spice.Models;

namespace Spice.Data
{
    public class SpiceContext : DbContext
    {
        public SpiceContext (DbContextOptions<SpiceContext> options)
            : base(options)
        {
        }

        public DbSet<Spice.Models.Subcategory> Subcategory { get; set; }
    }
}
