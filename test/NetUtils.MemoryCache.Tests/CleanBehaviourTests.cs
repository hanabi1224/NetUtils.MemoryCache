using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace NetUtils.MemoryCache.Tests
{
    [TestFixture]
    public class CleanBehaviourTests
    {
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task TestCacheCleanBehaviour(bool shouldReloadInBackground)
        {
            MemoryCache.CacheCleanCheckInternal = TimeSpan.FromMilliseconds(200);

            var cacheName = $"{nameof(TestCacheCleanBehaviour)}|{shouldReloadInBackground}";
            var cache = MemoryCache.GetNamedInstance(cacheName, CacheExpirePolicy.ExpireOnLastUpdate);

            cache.CleanInternal = TimeSpan.FromSeconds(1);
            cache.Size.Should().Be(0);
            ExpireBehaviourTests.GetOrCreate(cache, Guid.NewGuid().ToString(), TimeSpan.FromMilliseconds(300), shouldReloadInBackground: shouldReloadInBackground);
            cache.Size.Should().Be(1);
            ExpireBehaviourTests.GetOrCreate(cache, Guid.NewGuid().ToString(), TimeSpan.FromMilliseconds(1300), shouldReloadInBackground: shouldReloadInBackground);
            cache.Size.Should().Be(2);
            await Task.Delay(TimeSpan.FromMilliseconds(1300));
            cache.Size.Should().Be(1);
            await Task.Delay(TimeSpan.FromMilliseconds(1300));
            cache.Size.Should().Be(0);

            MemoryCache.TryDeleteNamedInstance(cacheName).Should().BeTrue();

            MemoryCache.CacheCleanCheckInternal = MemoryCache.DefaultCacheCleanCheckInternal;
        }
    }
}
