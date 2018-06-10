using Client.Data;
using Client.DbModels;
using Client.Logging;
using Client.Utils;
using System;
using System.Collections.Generic;

namespace Client
{
    public class Client
    {
        static void Main(string[] args)
        {
            ILogger logger = new Logger(@"log.txt");
            logger.Log($"{DateTime.UtcNow} : This was logged just now!!!");

            var dbSaver = new DbSaver(logger, new TaskBettingDbContext());
            dbSaver.InsertOrUpdate(new MyObjectWrapper
            {
                ObjectWrapperId = 2,
                Name = @"Second",
                ObjectAsByte = ObjectToSmthgConverter.ObjectToByteArray(new SerializableObject
                {
                    Description = @"13231123321",
                    Id = 35,
                    Name = @"132312213312",
                    Price = 14,
                    Value = 22,
                }),
                Items = new HashSet<MyCollectionItem>
                {
                    new MyCollectionItem(),
                    new MyCollectionItem()
                }
            });
        }
    }

    [Serializable]
    public class SerializableObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public int Value { get; set; }
        public decimal Price { get; set; }
    }
}
