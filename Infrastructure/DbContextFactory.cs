using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Domain;

namespace Infrastructure
{
    public class DbContextFactory
    {
        public static BugMgrDbContext Create()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<BugMgrDbContext>();
            optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"));

            return new BugMgrDbContext(optionsBuilder.Options);
        }
    }
}
