using FASTER.core;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace WebCore.test
{
    [TestFixture]
    public class Cache_KVCache_Tests
    {
        private Cache.KVCache cache;

        [SetUp]
        public void Setup()
        {
            var cacheDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
            cache = new Cache.KVCache(cacheDirectory);
        }

        [Test]
        public void BasicTest()
        {
            // Create a clean directory
            cache.GetDirectory().CreateDirectory();

            const int totalRecords = 200;

            // Write
            var fht = cache.GetCache();
            var session = cache.GetSession();
            for (int i = 0; i < totalRecords; i++)
            {
                var key = $"{i}";
                var value = $"{i}";
                cache.Set(key, Encoding.UTF8.GetBytes(value));
            }
            session.CompletePending(true);
            Assert.AreEqual(totalRecords, fht.EntryCount);

            // Read
            var waitTimes = 0;
            for (int i = 0; i < totalRecords; i++)
            {
                Cache.DataValue input = default, output = default;
                var key = new Cache.Md5Key($"{i}");
                var value = $"{i}";

                if (session.Read(ref key, ref input, ref output) == Status.PENDING)
                {
                    waitTimes++;
                    session.CompletePending(true);
                }
                else
                {
                    Assert.AreEqual(value, Encoding.UTF8.GetString(output.Value));
                }
            }

            if (waitTimes > 0) Assert.Warn($"wait times: {waitTimes}");

            Assert.Pass();
        }

        [Test]
        public void RecoverTest()
        {
            // Create a clean directory
            cache.GetDirectory().CreateDirectory();

            const int totalRecords = 200;

            // Write
            var fht = cache.GetCache();
            var session = cache.GetSession();
            for (int i = 0; i < totalRecords; i++)
            {
                var key = $"{i}";
                var value = $"{i}";
                cache.Set(key, Encoding.UTF8.GetBytes(value));
            }
            session.CompletePending(true);
            Assert.AreEqual(totalRecords, fht.EntryCount);

            // Save
            cache.SaveSnapshot();
            TearDown();
            // Recover
            Setup();
            fht = cache.GetCache();
            Assert.AreEqual(totalRecords, fht.EntryCount);
            session = cache.GetSession();

            // Read
            var waitTimes = 0;
            for (int i = 0; i < totalRecords; i++)
            {
                Cache.DataValue input = default, output = default;
                var key = new Cache.Md5Key($"{i}");
                var value = $"{i}";

                if (session.Read(ref key, ref input, ref output) == Status.PENDING)
                {
                    waitTimes++;
                    session.CompletePending(true);
                }
                else
                {
                    Assert.AreEqual(value, Encoding.UTF8.GetString(output.Value));
                }
            }

            if (waitTimes > 0) Assert.Warn($"wait times: {waitTimes}");

            Assert.Pass();
        }

        [TearDown]
        public void TearDown()
        {
            cache.Dispose();
        }
    }
}
