using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics.CodeAnalysis;

namespace ED47.Stack.Sample.Entities.Migrations
{
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    public sealed class Configuration : DbMigrationsConfiguration<SampleContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        public static void Initialize()
        {
#if DEBUG
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<SampleContext, Configuration>());
#endif
        }

        protected override void Seed(SampleContext context)
        {
            /*
             * You can call seed methods here to seed with referencial data            
             SeedCountries.Seed(context);             
             */

#if DEBUG
            /*
             * You can call seed methods here to seed with development data            
             SeedRequests.Seed(context);             
             */
#endif
        }
    }
}
