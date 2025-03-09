//using System;
//using System.Collections.Concurrent;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata;
//using Xunit;
//using LinqApi.Context;
//using LinqApi.Context.LinqApi.Context;

//namespace LinqApi.Tests
//{
//    // Test için kullanılacak basit dummy entity
//    public class DummyEntity
//    {
//        public int Id { get; set; }
//        public string Name { get; set; }
//    }

//    public class DynamicDbContextTests
//    {
//        // Test context'ini oluşturmak için yardımcı metod
//        private DynamicDbContext CreateContext(
//            ConcurrentDictionary<string, Type> dynamicEntities,
//            ConcurrentDictionary<string, string> primaryKeyMappings)
//        {
//            var options = new DbContextOptionsBuilder<DynamicDbContext>()
//                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//                .Options;

//            return new DynamicDbContext(options, dynamicEntities, primaryKeyMappings);
//        }

//        [Fact]
//        public void OnModelCreating_Configures_TableAndSchema_WithPrimaryKeyMapping()
//        {
//            // Arrange: Dinamik entity ve primary key mapping ayarları
//            var dynamicEntities = new ConcurrentDictionary<string, Type>();
//            // Key formatı: "schema.table"
//            dynamicEntities.TryAdd("dbo.Dummy", typeof(DummyEntity));

//            var primaryKeyMappings = new ConcurrentDictionary<string, string>();
//            // "Id" property'sinin "DummyId" kolonuna eşleneceğini belirtiyoruz.
//            primaryKeyMappings.TryAdd("dbo.Dummy", "DummyId");

//            using (var context = CreateContext(dynamicEntities, primaryKeyMappings))
//            {
//                // Act: Modelin oluşturulduğunu garanti ediyoruz
//                var model = context.Model;
//                var entityType = model.FindEntityType(typeof(DummyEntity));
//                Assert.NotNull(entityType);

//                // Assert: Tablo adı ve şemanın doğru ayarlandığını kontrol ediyoruz
//                Assert.Equal("Dummy", entityType.GetTableName());
//                Assert.Equal("dbo", entityType.GetSchema());

//                // Assert: "Id" property'sinin primary key olarak ayarlandığını ve kolon adının "DummyId" olduğunu kontrol edelim
//                var idProperty = entityType.FindProperty("Id");
//                Assert.NotNull(idProperty);
//                var columnName = idProperty.GetColumnName(StoreObjectIdentifier.Table(entityType.GetTableName(), entityType.GetSchema()));
//                Assert.Equal("DummyId", columnName);
//            }
//        }

//        [Fact]
//        public void OnModelCreating_Configures_TableAndSchema_WithoutPrimaryKeyMapping()
//        {
//            // Arrange: Sadece dinamik entity ekliyoruz, primary key mapping olmadan
//            var dynamicEntities = new ConcurrentDictionary<string, Type>();
//            dynamicEntities.TryAdd("dbo.Dummy", typeof(DummyEntity));

//            var primaryKeyMappings = new ConcurrentDictionary<string, string>();
//            // Primary key mapping eklenmedi.

//            using (var context = CreateContext(dynamicEntities, primaryKeyMappings))
//            {
//                // Act: Modelin oluşturulmasını sağlıyoruz
//                var model = context.Model;
//                var entityType = model.FindEntityType(typeof(DummyEntity));
//                Assert.NotNull(entityType);

//                // Assert: Tablo adı ve şemanın doğru ayarlandığını kontrol edelim
//                Assert.Equal("Dummy", entityType.GetTableName());
//                Assert.Equal("dbo", entityType.GetSchema());

//                // Assert: Primary key konfigürasyonu, EF Core'un konvansiyonlarına göre "Id" üzerinden belirlenecektir.
//                var primaryKey = entityType.FindPrimaryKey();
//                Assert.NotNull(primaryKey);
//                Assert.Contains(primaryKey.Properties, p => p.Name == "Id");

//                // EF Core varsayılan olarak property adını kolon adı olarak kullanır. Bu nedenle "Id" olmalı.
//                var idProperty = entityType.FindProperty("Id");
//                var columnName = idProperty.GetColumnName(StoreObjectIdentifier.Table(entityType.GetTableName(), entityType.GetSchema()));
//                Assert.Equal("Id", columnName);
//            }
//        }
//    }
//}
