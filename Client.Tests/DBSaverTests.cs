using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;
using Client.DbModels;
using Client.Utils;
using Client.Data;
using Client.Logging;

namespace Client.Tests
{
    [TestClass]
    public class DBSaverTests
    {
        private Mock<ILogger> mockedLogger;

        [TestInitialize]
        public void Initialize()
        {
            this.mockedLogger = new Mock<ILogger>();
            this.mockedLogger.Setup(logger => logger.Log(It.IsAny<string>()));
        }

        [TestMethod]
        public void InsertOrUpdate_Saves_A_NewObjectWrapper_Via_Context()
        {
            var wrapperParent = this.GetDummyData();
            wrapperParent.ObjectWrapperId = 0;
            var mockWrapperSet = this.GetMockSet<MyObjectWrapper>(new List<MyObjectWrapper>());

            var mockContext = this.BuildMockContext(mockWrapperSet);

            var dbSaverService = new DbSaver(this.mockedLogger.Object, mockContext.Object);
            var createdWrapper = dbSaverService.InsertOrUpdate(wrapperParent);

            Assert.IsNotNull(mockWrapperSet);
            Assert.IsTrue(createdWrapper.Name == wrapperParent.Name);
            Assert.IsTrue(createdWrapper.ObjectAsByte == wrapperParent.ObjectAsByte);
            Assert.IsTrue(createdWrapper.Items.Count() == 2);
            mockWrapperSet.Verify(set => set.Add(It.IsAny<MyObjectWrapper>()), Times.Once);
            mockWrapperSet.Verify(set => set.Include(It.IsAny<string>()), Times.Never);
            mockContext.Verify(context => context.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void InsertOrUpdate_Updates_AllPropsChanged_NewItems_ExistingObjectWrapper_Via_Context()
        {
            var wrapperToUpdate = this.GetDummyData();
            wrapperToUpdate.Items.First().MyCollectionItemId = default(int);
            wrapperToUpdate.Items.Last().MyCollectionItemId = default(int);

            var beforeUpdateWrapper = this.GetDummyData();
            beforeUpdateWrapper.Name = "Third";
            beforeUpdateWrapper.ObjectAsByte = ObjectToSmthgConverter.ObjectToByteArray(new SerializableObject
            {
                Description = @"1hehf(%&#U)JJF",
                Id = 123123,
                Name = @"132312#$%@%213312",
                Price = 23333,
                Value = 17777,
            });

            var mockWrapperSet = this.GetMockSet<MyObjectWrapper>(new List<MyObjectWrapper> { beforeUpdateWrapper });

            var mockContext = this.BuildMockContext(mockWrapperSet);

            var dbSaverService = new DbSaver(this.mockedLogger.Object, mockContext.Object);
            var updatedWrapper = dbSaverService.InsertOrUpdate(wrapperToUpdate);

            Assert.IsNotNull(mockWrapperSet);
            Assert.IsTrue(updatedWrapper.Name == wrapperToUpdate.Name);
            Assert.IsTrue(updatedWrapper.ObjectAsByte == wrapperToUpdate.ObjectAsByte);
            Assert.IsTrue(updatedWrapper.Items.Count() == 4);
            mockWrapperSet.Verify(set => set.Add(It.IsAny<MyObjectWrapper>()), Times.Never);
            mockWrapperSet.Verify(set => set.Include(It.IsAny<string>()), Times.Once);
            mockContext.Verify(context => context.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void InsertOrUpdate_Updates_OneNewItem_OneExistingItem_ExistingObjectWrapper_Via_Context()
        {
            var wrapperToUpdate = this.GetDummyData();
            wrapperToUpdate.Items.First().MyCollectionItemId = 1;
            wrapperToUpdate.Items.Last().MyCollectionItemId = default(int);
            var beforeUpdateWrapper = this.GetDummyData();

            var mockWrapperSet = this.GetMockSet<MyObjectWrapper>(new List<MyObjectWrapper> { beforeUpdateWrapper });
            var mockContext = this.BuildMockContext(mockWrapperSet);
            var dbSaverService = new DbSaver(this.mockedLogger.Object, mockContext.Object);

            var updatedWrapper = dbSaverService.InsertOrUpdate(wrapperToUpdate);

            Assert.IsNotNull(mockWrapperSet);
            Assert.IsTrue(updatedWrapper.Name == wrapperToUpdate.Name);
            Assert.IsTrue(updatedWrapper.ObjectAsByte == wrapperToUpdate.ObjectAsByte);
            Assert.IsTrue(updatedWrapper.Items.Count() == 3);
            mockWrapperSet.Verify(set => set.Add(It.IsAny<MyObjectWrapper>()), Times.Never);
            mockWrapperSet.Verify(set => set.Include(It.IsAny<string>()), Times.Once);
            mockContext.Verify(context => context.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void InsertOrUpdate_Updates_RemoveOneItem_ExistingObjectWrapper_Via_Context()
        {
            var wrapperToUpdate = this.GetDummyData();
            wrapperToUpdate.Items.First().MyObjectWrapper = null;
            var beforeUpdateWrapper = this.GetDummyData();

            var mockWrapperSet = this.GetMockSet<MyObjectWrapper>(new List<MyObjectWrapper> { beforeUpdateWrapper });
            var mockContext = this.BuildMockContext(mockWrapperSet);
            var dbSaverService = new DbSaver(this.mockedLogger.Object, mockContext.Object);

            var updatedWrapper = dbSaverService.InsertOrUpdate(wrapperToUpdate);

            Assert.IsNotNull(mockWrapperSet);
            Assert.IsTrue(updatedWrapper.Name == wrapperToUpdate.Name);
            Assert.IsTrue(updatedWrapper.ObjectAsByte == wrapperToUpdate.ObjectAsByte);
            Assert.IsTrue(updatedWrapper.Items.Count() == 1);
            mockWrapperSet.Verify(set => set.Add(It.IsAny<MyObjectWrapper>()), Times.Never);
            mockWrapperSet.Verify(set => set.Include(It.IsAny<string>()), Times.Once);
            mockContext.Verify(context => context.SaveChanges(), Times.Once);
        }

        private Mock<TaskBettingDbContext> BuildMockContext(Mock<DbSet<MyObjectWrapper>> mockedWrapperSet)
        {
            var mockContext = new Mock<TaskBettingDbContext>();
            mockContext.Setup(mc => mc.MyObjectWrappers).Returns(mockedWrapperSet.Object);

            return mockContext;
        }

        private Mock<DbSet<T>> GetMockSet<T>(IList<T> list) where T : class
        {
            var queryable = list.AsQueryable();
            var mockList = new Mock<DbSet<T>>(MockBehavior.Loose);

            mockList.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockList.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockList.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockList.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockList.Setup(m => m.Include(It.IsAny<string>())).Returns(mockList.Object);
            mockList.Setup(m => m.Add(It.IsAny<T>())).Returns((T a) => { list.Add(a); return a; });

            return mockList;
        }

        private MyObjectWrapper GetDummyData()
        {
            var wrapperParent = new MyObjectWrapper
            {
                ObjectWrapperId = 1,
                Name = @"Second",
                ObjectAsByte = ObjectToSmthgConverter.ObjectToByteArray(new SerializableObject
                {
                    Description = @"132asde-+/*31123321",
                    Id = 35,
                    Name = @"132312#$%@%213312",
                    Price = 14,
                    Value = 22,
                })
            };

            var collectionItemChildA = new MyCollectionItem
            {
                MyCollectionItemId = 1,
                MyObjectWrapperId = 1,
                MyObjectWrapper = wrapperParent
            };
            var collectionItemChildB = new MyCollectionItem
            {
                MyCollectionItemId = 2,
                MyObjectWrapperId = 1,
                MyObjectWrapper = wrapperParent
            };
            wrapperParent.Items.Add(collectionItemChildA);
            wrapperParent.Items.Add(collectionItemChildB);

            return wrapperParent;
        }
    }
}
