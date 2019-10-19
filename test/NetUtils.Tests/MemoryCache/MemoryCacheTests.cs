using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace NetUtils.MemoryCache.Tests
{
    [TestFixture]
    public class MemoryCacheTests
    {
        [Test]
        public void TestDefaultInstance()
        {
            MemoryCache.DefaultInstance.Should().NotBeNull();
        }

        [Test]
        public void TestGetCurrentClassNamedCache()
        {
            var cache = MemoryCache.GetCurrentClassNamedCacheInstance();
            cache.Name.Should().Be(GetType().FullName);
        }

        [Test]
        public void TestCreateDeleteInstance()
        {
            var cacheName = Guid.NewGuid().ToString();
            ICacheInstance cache = MemoryCache.GetNamedInstance(cacheName);
            cache.Should().NotBeNull();

            var data = new DummyDisposable();
            cache.SetData(nameof(data), data, Timeout.InfiniteTimeSpan);

            MemoryCache.GetNamedInstance(cacheName).Should().Be(cache);

            data.IsDisposed.Should().BeFalse();
            MemoryCache.TryDeleteNamedInstance(cacheName).Should().BeTrue();
            data.IsDisposed.Should().BeTrue();

            MemoryCache.GetNamedInstance(cacheName).Should().NotBe(cache);
        }
    }
}
