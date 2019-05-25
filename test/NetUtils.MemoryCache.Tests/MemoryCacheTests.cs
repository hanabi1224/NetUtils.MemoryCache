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
        public void TestCreateDeleteInstance()
        {
            var cacheName = Guid.NewGuid().ToString();
            ICacheInstance cache = MemoryCache.GetNamedInstance(cacheName);
            cache.Should().NotBeNull();

            var isDisposed = false;
            var data = new DummyDisposable(() => { isDisposed = true; });
            cache.SetData(nameof(data), data, Timeout.InfiniteTimeSpan);

            MemoryCache.GetNamedInstance(cacheName).Should().Be(cache);

            isDisposed.Should().BeFalse();
            MemoryCache.TryDeleteNamedInstance(cacheName).Should().BeTrue();
            isDisposed.Should().BeTrue();

            MemoryCache.GetNamedInstance(cacheName).Should().NotBe(cache);
        }
    }
}
