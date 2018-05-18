using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace NetUtils.MemoryCache.Tests
{
    [TestFixture]
    public class ExpireBehaviourTests
    {
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task GetAutoReloadDataWithIntervalTests(bool shouldReloadInBackground)
        {
            var cache = MemoryCache.GetNamedInstance(nameof(GetAutoReloadDataWithIntervalTests));
            var key = Guid.NewGuid().ToString();
            var dataReloadInterval = TimeSpan.FromMilliseconds(200);
            var ref1 = GetOrCreate(cache, key, dataReloadInterval, shouldReloadInBackground: shouldReloadInBackground);
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            var ref2 = GetOrCreate(cache, key, dataReloadInterval, shouldReloadInBackground: shouldReloadInBackground);
            await Task.Delay(TimeSpan.FromMilliseconds(150));
            var ref3 = GetOrCreate(cache, key, dataReloadInterval, shouldReloadInBackground: shouldReloadInBackground);
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            var ref4 = GetOrCreate(cache, key, dataReloadInterval, shouldReloadInBackground: shouldReloadInBackground);

            ref1.Should().Be(ref2);
            if (shouldReloadInBackground)
            {
                ref2.Should().Be(ref3);
                ref3.Should().NotBe(ref4);
            }
            else
            {
                ref2.Should().NotBe(ref3);
                ref3.Should().Be(ref4);
            }
        }

        private static object GetOrCreate(ICacheInstance cache, string key, TimeSpan dataReloadInterval, bool shouldReloadInBackground)
        {
            return cache.GetAutoReloadDataWithInterval(key, () => new object(), TimeSpan.MaxValue, dataReloadInterval, shouldReloadInBackground: shouldReloadInBackground);
        }
    }
}
