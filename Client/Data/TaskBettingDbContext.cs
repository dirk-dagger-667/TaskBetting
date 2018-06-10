using Client.DbModels;
using System.Data.Entity;

namespace Client.Data
{
    public class TaskBettingDbContext : DbContext
    {
        public TaskBettingDbContext()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<TaskBettingDbContext>());
        }

        public virtual IDbSet<MyObjectWrapper> MyObjectWrappers { get; set; }

        public virtual IDbSet<MyCollectionItem> MyCollectionItems { get; set; }
    }
}
