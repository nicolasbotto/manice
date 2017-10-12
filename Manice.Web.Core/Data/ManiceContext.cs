using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Manice.Web.Core.Models
{
    public class ManiceContext : DbContext
    {
        public ManiceContext (DbContextOptions<ManiceContext> options)
            : base(options)
        {
        }

        public DbSet<Manice.Web.Core.Models.Beer> Beer { get; set; }
    }
}
