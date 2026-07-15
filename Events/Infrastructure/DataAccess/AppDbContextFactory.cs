using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.DataAccess
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var curDir = Directory.GetCurrentDirectory();
            var presentationDir = curDir.Replace("Infrastructure", "Presentation");
            var configuration = new ConfigurationBuilder()
                            .SetBasePath(presentationDir)
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .Build();
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
