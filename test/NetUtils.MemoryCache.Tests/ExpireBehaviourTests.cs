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
        public async Task ExpireOnLastUpdate_ExpirationShouldBeCountedSinceLastUpdate(bool shouldReloadInBackground)
        {
            var cache = MemoryCache.GetNamedInstance(nameof(ExpireOnLastUpdate_ExpirationShouldBeCountedSinceLastUpdate), CacheExpirePolicy.ExpireOnLastUpdate);
            var key = Guid.NewGuid().ToString();
            var timeout = TimeSpan.FromMilliseconds(200);
            var ref1 = GetOrCreate(cache, key, timeout, shouldReloadInBackground: shouldReloadInBackground);
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            var ref2 = GetOrCreate(cache, key, timeout, shouldReloadInBackground: shouldReloadInBackground);
            await Task.Delay(TimeSpan.FromMilliseconds(150));
            var ref3 = GetOrCreate(cache, key, timeout, shouldReloadInBackground: shouldReloadInBackground);
            await Task.Delay(TimeSpan.FromMilliseconds(50));
            var ref4 = GetOrCreate(cache, key, timeout, shouldReloadInBackground: shouldReloadInBackground);

            ref1.Should().Be(ref2);
            ref1.Should().NotBe(ref4);
            if (shouldReloadInBackground)
            {
                ref3.Should().Be(ref2);
            }
            else
            {
                ref3.Should().Be(ref4);
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task ExpireOnLastAccess_ExpirationShouldBeCountedSinceLastUpdate(bool shouldReloadInBackground)
        {
            var cache = MemoryCache.GetNamedInstance(nameof(ExpireOnLastAccess_ExpirationShouldBeCountedSinceLastUpdate), CacheExpirePolicy.ExpireOnLastAccess);
            var key = Guid.NewGuid().ToString();
            var timeout = TimeSpan.FromMilliseconds(200);
            var ref1 = GetOrCreate(cache, key, timeout, shouldReloadInBackground: shouldReloadInBackground);
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            var ref2 = GetOrCreate(cache, key, timeout, shouldReloadInBackground: shouldReloadInBackground);
            await Task.Delay(TimeSpan.FromMilliseconds(150));
            var ref3 = GetOrCreate(cache, key, timeout, shouldReloadInBackground: shouldReloadInBackground);
            await Task.Delay(TimeSpan.FromMilliseconds(250));
            var ref4 = GetOrCreate(cache, key, timeout, shouldReloadInBackground: shouldReloadInBackground);
            await Task.Delay(TimeSpan.FromMilliseconds(150));
            var ref5 = GetOrCreate(cache, key, timeout, shouldReloadInBackground: shouldReloadInBackground);

            ref1.Should().Be(ref2);
            ref2.Should().Be(ref3);
            ref1.Should().NotBe(ref5);
            if (shouldReloadInBackground)
            {
                ref4.Should().Be(ref3);
            }
            else
            {
                ref4.Should().Be(ref5);
            }
        }

        public static object GetOrCreate(ICacheInstance cache, string key, TimeSpan timeout, bool shouldReloadInBackground)
        {
            return cache.GetDataOrCreate(key, () => new object(), timeout, shouldReloadInBackground: shouldReloadInBackground);
        }
    }
}
