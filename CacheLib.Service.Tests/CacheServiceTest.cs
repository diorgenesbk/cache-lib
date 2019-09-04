using Cache.Domain;
using Cache.Service.Business;
using NUnit.Framework;
using System;
using System.Collections.Generic;

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
            throw new NotImplementedException();
        }

        [Test]
        public void GetFromCache()
        {
            Person person = new Person("Di�rgenes", 26);
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
            Person person = new Person("Di�rgenes", 26);
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

    }
}