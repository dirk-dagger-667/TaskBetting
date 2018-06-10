using Client.Data;
using Client.DbModels;
using Client.Logging;
using System.Data.Entity;
using System.Linq;

namespace Client
{
    public class DbSaver
    {
        private readonly ILogger logger;
        private readonly TaskBettingDbContext dbContext;

        public DbSaver(ILogger logger, TaskBettingDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        public MyObjectWrapper InsertOrUpdate(MyObjectWrapper myObjectWrapper)
        {
            this.dbContext.Database.Log = message => logger.Log(message);
            MyObjectWrapper resultingObject;

            if (myObjectWrapper.ObjectWrapperId == default(int) 
                ||  !this.dbContext
                    .MyObjectWrappers
                    .Any(wrapper => wrapper.ObjectWrapperId == myObjectWrapper.ObjectWrapperId))
            {
                resultingObject = this.dbContext.MyObjectWrappers.Add(myObjectWrapper);
            }
            else
            {
                var objectWrapper = this.dbContext
                .MyObjectWrappers
                .Include(ow => ow.Items)
                .FirstOrDefault(ow => ow.ObjectWrapperId == myObjectWrapper.ObjectWrapperId);

                objectWrapper.Name = myObjectWrapper.Name;
                objectWrapper.ObjectAsByte = myObjectWrapper.ObjectAsByte;

                foreach (var item in myObjectWrapper.Items)
                {
                    if (item.MyCollectionItemId == default(int) 
                        || !objectWrapper
                           .Items
                           .Any(it => it.MyCollectionItemId == item.MyCollectionItemId))
                    {
                        objectWrapper.Items.Add(item);
                    }
                    else if (item.MyObjectWrapper == null)
                    {
                        var itemToRemove = objectWrapper
                            .Items
                            .FirstOrDefault(i => i.MyCollectionItemId == item.MyCollectionItemId);
                        objectWrapper.Items.Remove(itemToRemove);
                    }
                    else
                    {
                        //Update Properties of item
                    }
                }

                resultingObject = objectWrapper;
            }

            this.dbContext.SaveChanges();
            return resultingObject;
        }
    }
}
