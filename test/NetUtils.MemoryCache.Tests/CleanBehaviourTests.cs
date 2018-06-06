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
        public async Task TestCacheCleanBehaviourAsync(bool shouldReloadInBackground)
        {
            var cacheName = $"{nameof(TestCacheCleanBehaviourAsync)}|{shouldReloadInBackground}";
            var cache = MemoryCache.GetNamedInstance(cacheName);

            cache.CleanInternal = TimeSpan.FromMilliseconds(200);

            cache.Size.Should().Be(0);
            GetOrCreate(cache, Guid.NewGuid().ToString(), TimeSpan.FromMilliseconds(200), shouldReloadInBackground: shouldReloadInBackground);
            cache.Size.Should().Be(1);
            GetOrCreate(cache, Guid.NewGuid().ToString(), TimeSpan.FromMilliseconds(500), shouldReloadInBackground: shouldReloadInBackground);
            cache.Size.Should().Be(2);
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            cache.Size.Should().Be(2);
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
            cache.Size.Should().Be(0);

            MemoryCache.TryDeleteNamedInstance(cacheName).Should().BeTrue();
        }

        private static void GetOrCreate(ICacheInstance cache, string key, TimeSpan timeToLive, bool shouldReloadInBackground)
        {
            cache.GetAutoReloadDataWithInterval(key, () => new object(), timeToLive, TimeSpan.MaxValue, shouldReloadInBackground: shouldReloadInBackground);
        }
    }
}
