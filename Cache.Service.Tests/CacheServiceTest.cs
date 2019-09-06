using Cache.Domain;
using Cache.Service.Business;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Cache.Service.Tests
{
    public class CacheServiceTest
    {
        private CacheService CacheService { get; set; }

        [SetUp]
        public void Setup()
        {
            this.CacheService = new CacheService();
        }

        [Test]
        public void AddToCache()
        {
            Assert.IsTrue(this.CacheService.Set("CacheServiceTest:AddToCache", 1));
        }

        [Test]
        public void AddToCacheWithTime()
        {
            string key = "CacheServiceTest:AddToCache";

            this.CacheService.Set(key, 1, 5);

            Thread.Sleep(6000);

            var cacheditem = this.CacheService.Get<int?>(key);

            Assert.IsNull(cacheditem);
        }

        [Test]
        public void GetFromCache()
        {
            Person person = new Person("Diórgenes", 26);
            this.CacheService.Set("CacheServiceTest:GetFromCache", person);

            var cachedItem = this.CacheService.Get<Person>("CacheServiceTest:GetFromCache");

            Assert.AreEqual(person.Name, cachedItem.Name);
        }

        [Test]
        public void GetFromCacheWithInvalidKey()
        {
            var cachedItem = this.CacheService.Get<Person>("test");

            Assert.IsNull(cachedItem);
        }

        [Test]
        public void GetFromCacheWithNullKey()
        {
            var cachedItem = this.CacheService.Get<Person>(null);

            Assert.IsNull(cachedItem);
        }

        [Test]
        public void DeleteFromCache()
        {
            Person person = new Person("Diórgenes", 26);
            this.CacheService.Set("CacheServiceTest:DeleteFromCache", person);

            this.CacheService.Del("CacheServiceTest:DeleteFromCache");

            var cachedItem = this.CacheService.Get<Person>("CacheServiceTest:DeleteFromCache");

            Assert.IsNull(cachedItem);
        }

        [Test]
        public void DeleteListFromCache()
        {
            Person ana = new Person("Ana", 26);
            Person joao = new Person("Joao", 26);
            Person maria = new Person("Maria", 26);

            string keyAna = $"CacheServiceTest:DeleteListFromCache:{ana.Name}";
            string keyJoao = $"CacheServiceTest:DeleteListFromCache:{joao.Name}";
            string keyMaria = $"CacheServiceTest:DeleteListFromCache:{maria.Name}";

            this.CacheService.Set(keyAna, ana);

            this.CacheService.Set(keyJoao, joao);

            this.CacheService.Set(keyMaria, maria);

            this.CacheService.Del(new List<string>() { keyAna, keyJoao, keyMaria });

            var dbSize = this.CacheService.DbSize();

            Assert.AreEqual(0, dbSize);
        }

        [Test]
        public void DbSizeWithZeroElements()
        {
            Assert.AreEqual(0, this.CacheService.DbSize());
        }

        [Test]
        public void IncrWithNoCachedItem()
        {
            string key = "CacheServiceTest:IncrWithNoCachedItem";

            this.CacheService.Incr(key);

            var cachedItem = this.CacheService.Get<int>(key);

            Assert.AreEqual(0, cachedItem);
        }

        [Test]
        public void IncrWithExistingCachedItem()
        {
            string key = "CacheServiceTest:IncrWithExistingCachedItem";

            this.CacheService.Incr(key);
            this.CacheService.Incr(key);
            this.CacheService.Incr(key);

            var cachedItem = this.CacheService.Get<int>(key);

            Assert.AreEqual(2, cachedItem);
        }

        [Test]
        public void InsertMembersZADD()
        {
            string key = "CacheServiceTest:InsertMembersZADD";

            this.CacheService.Zadd(key, new string[] { "1 member1","2 member2","3 member3","4 member4","5 member5"});

            var cachedItem = this.CacheService.Get<string[]>(key);

            Assert.IsNotNull(cachedItem);
        }

        [Test]
        public void InsertAndOrderMembersZADD()
        {
            string key = "CacheServiceTest:InsertAndOrderMembersZADD";

            this.CacheService.Zadd(key, new string[] { "3 member3", "5 member5", "2 member2", "4 member4", "1 member1" });

            var cachedItem = this.CacheService.Get<string[]>(key);

            Assert.IsTrue(
                cachedItem[0] == "1 member1" 
                && cachedItem[1] == "2 member2" 
                && cachedItem[2] == "3 member3" 
                && cachedItem[3] == "4 member4" 
                && cachedItem[4] == "5 member5");
        }

        [Test]
        public void UpdateAndReOrderMembersZADD()
        {
            string key = "CacheServiceTest:InsertAndOrderMembersZADD";

            this.CacheService.Zadd(key, new string[] { "3 member3", "5 member5", "2 member2", "4 member4", "1 member1" });

            this.CacheService.Zadd(key, new string[] { "5 member1" });
            
            var cachedItem = this.CacheService.Get<string[]>(key);

            Assert.IsTrue(
                cachedItem[0] == "2 member2" 
                && cachedItem[1] == "3 member3" 
                && cachedItem[2] == "4 member4" 
                && cachedItem[3] == "5 member5" 
                && cachedItem[4] == "6 member1");
        }

        [Test]
        public void InsertUpdateAndReOrderMembersZADD()
        {
            string key = "CacheServiceTest:InsertUpdateAndReOrderMembersZADD";

            this.CacheService.Zadd(key, new string[] { "3 member3", "5 member5", "2 member2", "4 member4", "1 member1" });

            this.CacheService.Zadd(key, new string[] { "5 member1", "1 member6" });

            var cachedItem = this.CacheService.Get<string[]>(key);

            Assert.IsTrue(
                cachedItem[0] == "1 member6" 
                && cachedItem[1] == "2 member2" 
                && cachedItem[2] == "3 member3" 
                && cachedItem[3] == "4 member4" 
                && cachedItem[4] == "5 member5" 
                && cachedItem[5] == "6 member1");
        }

        [Test]
        public void GetZcard()
        {
            string key = "CacheServiceTest:GetZcard";

            this.CacheService.Zadd(key, new string[] { "1 member1", "2 member2", "3 member3", "4 member4", "5 member5" });

            var cachedItem = this.CacheService.Zcard(key);

            Assert.AreEqual(cachedItem, 5);
        }

        [Test]
        public void GetZrank()
        {
            string key = "CacheServiceTest:GetZrank";

            this.CacheService.Zadd(key, new string[] { "1 member1", "2 member2", "3 member3", "4 member4", "5 member5" });

            var cachedItem = this.CacheService.ZRank(key, "member4");

            Assert.AreEqual("3", cachedItem);
        }

        [Test]
        public void GetZrange()
        {
            string key = "CacheServiceTest:GetZrank";
            this.CacheService.Zadd(key, new string[] { "1 member1", "2 member2", "3 member3", "4 member4", "5 member5" });

            var cachedItem = this.CacheService.ZRange(key, 1, 2);

            Assert.IsTrue(cachedItem[0] == "2 member2" && cachedItem[1] == "3 member3");
        }
    }
}