using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using ED47.BusinessAccessLayer;

namespace ED47.Communicator.Entities
{
    public class SampleContext : DbContext
    {
        public SampleContext()
        {
            Configuration.LazyLoadingEnabled = true;
            ConnectionString = ConfigurationManager.ConnectionStrings[GetType().FullName].ConnectionString;
        }

        /// <summary>
        ///   Creates the DbContext with a specific connection string name.
        /// </summary>
        /// <param name="connectionStringName"> The name of the connection string in the configuration. </param>
        public SampleContext(string connectionStringName)
            : base(connectionStringName)
        {
            ConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
        }

        /// <summary>
        ///  The full connection string used for this context
        /// </summary>
        public string ConnectionString { get; set; }

        /*
         * Add you entities here
         * Samples:
        public DbSet<House> Houses { get; set; }
        public DbSet<Room> Rooms { get; set; }
        */

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Removed On to many cascade delete convention
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            //Run OnModelCreating() methods on entities to prevent enormous OnModelCreating() here
            var assembly = GetType().Assembly;
            var dbEntityType = typeof(DbEntity);
            var baseEntity = typeof(BaseDbEntity);
            var dbEntities = assembly.GetTypes().Where(el => el.BaseType == dbEntityType || el.BaseType == baseEntity).ToArray();
            foreach (var mi in dbEntities.Select(t => t.GetMethod("OnModelCreating")).Where(mi => mi != null))
            {
                mi.Invoke(null, new object[] { modelBuilder });
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
