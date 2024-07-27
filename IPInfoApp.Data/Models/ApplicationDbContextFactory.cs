using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Data.Models
{
    /// <summary>
    /// Factory class that handles the creation of db instances during the design time of the creation of migrations
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            //Benefits
            //Seperation of concern, moves the configuration to a different entity
            //
           var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("data.settings.json")
                .Build();

            var dbContextOptionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            dbContextOptionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new ApplicationDbContext(dbContextOptionsBuilder.Options);
        }
    }
}
